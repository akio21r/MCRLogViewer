//==========================================================================
// MCR Log Viewer
//==========================================================================
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.ApplicationServices;
using System.Collections.Generic;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Linq;

namespace MCRLogViewer
{
	//メインフォーム
	public partial class frmMain : Form
	{
		//==================================================================
		//ログデータ関連
		//==================================================================
		static public int TXT_header_sectors = 3;	//TXT領域のセクタ数
		int LOG_Version		= 6;			//ログのバージョン
		int LOG_RecordBytes	= 17;			//ログの1レコードサイズ
		const int Camera_N	= 32;			//画素数
		const int GASO_HW	= 32;			//画素数（横）
		const int GASO_VW	= 24;			//画素数（縦）
		int hlPos			= 0;			//ハーフラインを読む位置(Camera)
		int vPos			= 0;			//中央線を読む縦の位置(Cam)
		int vPos2			= 0;			//遠方中央線を読む縦の位置(Cam)

		public const int max_log_data_counts = 5000000;	//500万行分のデータ★
		public struct LogData{				//ログデータ
			public int		mode;			//mode
		//	public StringBuilder	sens;	//センサの状態
			public int		v, vt;			//速度、目標速度
			public int		angle, angle_t;	//ハンドル角、目標角度
			public int		power;			//モータ出力
			public int		sv_pow;			//サーボモータの出力
			public int		fl,fr,rl,rr;	//各輪のモータ出力
			public int		slope_mode;		//slope_mode;
			public int		slope_sw;		//坂SWの状態
			public int		slope_cnt;		//出発してからの坂の数
			public int		trip;			//トリップメータ
			public int		gyro, gyroEx;	//ジャイロ出力値
			public int		side;			//サイドセンサの状態, ハーフライン
			public int		time;			//時間[ms]
			public int		floor;			//階

			//Camera用
			public int		center;			//Cameraのセンター値
			public int		etc;			//他
			public int		hlCntL, hlCntR;	//ハーフライン検出数カウント

			//Remote Sens用
			public int		anL1,anL2,anL,anR,anR2,anR1;	//anセンサ値

			//以下は現在使っていないもの。
			public int		pre_sens;		//先読みセンサ
			public float	batt;           //バッテリ電圧
		}
		static public List<LogData> log = new List<LogData>();
	
		public struct ImgLogData{			//画素ログ
			public byte		Center;
			public byte		Center2;		//遠方
			public byte		Sens;
			public byte[]	data;
		}
		static public List<ImgLogData> imgLog = new List<ImgLogData>();
		int imgLog_Count = 0;
		bool enableCenter2 = false;			//遠方センター値のデータがあるか

		static public int log_count;		//
		public string path;					//
		StringBuilder str;
		int LogFileSize;					//ログファイルの実質のサイズ

		public frmOption frmOption1 = new frmOption();
		static public int SCROLLBAR_WIDTH = 20;
//		System.Threading.Mutex mut;			//多重起動禁止用のMutexオブジェクト

		//==================================================================
		//グラフ関連
		//==================================================================
		public const int graph_points = 12;
		public struct myGraphPoints{
			public bool enabled;
			public Single y, y1;
			public Pen pen;
			public Single scale, max, min;
		}
		myGraphPoints[] gp = new myGraphPoints[graph_points];
		SolidBrush[] brsh = new SolidBrush[16];


		static public int y0;							//X軸
		static public int cur_n1=0, cur_x=0, cur_x1=0;	//グラフ上の現在,前の位置
		static public int cur3_n1=0, cur3_y=0, cur3_y1=0;	//グラフ3上の現在,前の位置
		static public int cur3b_y1=0;					//グラフ3上の前の位置
		static public bool cur3b_show = false;			//カーソル3表示
		static public bool cur_show = false;			//カーソル表示
		static public bool cur3_show = false;			//カーソル3表示
		static public Single graph_v;					//グラフの増分
		static public Single graph3_vx, graph3_vy;		//グラフの増分
		static public Point scrPoint1, scrPoint2;		//グラフのスクロール座標

		//==================================================================
		//バイナリファイルの圧縮保存
		//==================================================================
		public void FileSave()
		{
		//	string path_save = path.Substring(0, path.Length - 4) + "_new.LOG";
	
			FileStream fsr = new FileStream(path, FileMode.Open, FileAccess.Read);
			int fileSize = (int)fsr.Length;				// ファイルのサイズ
			byte[] buf = new byte[fileSize + 1024];		// データ格納用配列
			fsr.Read(buf, 0, fileSize);
			fsr.Close();
			fsr.Dispose();

			FileStream fsw = new FileStream(path, FileMode.Create, FileAccess.Write);
			fsw.Write(buf, 0, LogFileSize);
			fsw.Close();
			fsw.Dispose();
		}		
		
		//==================================================================
		//ファイルを開く（メニュー及びコマンドボタンより）
		//==================================================================
		private void FileOpen_Click(object sender, EventArgs e)
		{
			//“開く”ダイアログボックス
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.InitialDirectory = txtPath.Text;
			ofd.Filter = "MCRログファイル (*.LOG)|*.LOG|" + "すべてのファイル (*.*)|*.*";
			ofd.FilterIndex = 1;
			ofd.Multiselect = false;
			if (ofd.ShowDialog() == DialogResult.OK){
				FileOpen(ofd.FileName);
			}
			ofd.Dispose();
		}

		//==================================================================
		//ファイルの保存
		//==================================================================
		private void menuFileSave_Click(object sender, EventArgs e)
		{
			FileSave();
		}

		//==================================================================
		//フォームのリサイズ
		//==================================================================
		private void frmMain_Resize(object sender, EventArgs e)
		{
			try{
				if(lblHead1.Size.Width > 0){
					splitContainer1.SplitterDistance = lblHead1.Size.Width + SCROLLBAR_WIDTH;
				}
			}
			catch(Exception){
			}
		}

		//==================================================================
		//スプリットバー操作
		//==================================================================
		private void splitContainer2_Panel1_Resize(object sender, EventArgs e)
		{
			txtHead.Height = splitContainer2.Panel1.Height - txtHead.Location.Y;
		}

		private void splitContainer2_SizeChanged(object sender, EventArgs e)
		{
			txtHead.Width = splitContainer2.Panel1.Width - txtHead.Location.X * 3;
			txtPath.Width = txtHead.Width;
			lstView.Width = txtHead.Width;
			lstView.Height = splitContainer2.Panel2.Height - lstView.Location.Y;
		}

		private void splitContainer1_Panel2_Resize(object sender, EventArgs e)
		{
			InitGraph();
		}

		//==================================================================
		//エクスプローラからのファイルドラッグエンター
		//==================================================================
		private void frmMain_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.All;
		}

		//==================================================================
		//エクスプローラからのファイルドロップ
		//==================================================================
		private void frmMain_DragDrop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)){
				foreach (string fileName in (string[])e.Data.GetData(DataFormats.FileDrop)){
					FileOpen(fileName);
				}
			}
		}

		//==================================================================
		//メインフォームのコンストラクタ
		//==================================================================
		public frmMain()
		{
			InitializeComponent();

			//マウスホイールのイベント追加
			this.lstView.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.lstView_MouseWheel);
		}

		//==================================================================
		// 縦カーソルの描画
		//==================================================================
		private void lstView_SelectedIndexChanged(object sender, EventArgs e)
		{
		//	draw_cursol();
		}

		private void lstView_Click(object sender, EventArgs e)
		{
			draw_cursol();
		}

		private void lstView_KeyPress(object sender, KeyPressEventArgs e)
		{
			draw_cursol();
		}

		//==================================================================
		//pctGraphのマウスホイール操作イベント
		//==================================================================
		private void lstView_MouseWheel(object sender, MouseEventArgs e)
		{
			int width;

			width = pctGraph.Width;

			if (e.Delta > 0){
				width *= 2;
				if(width > pnlGraph.Width * 32) width = pnlGraph.Width * 16;
			}
			else{
				width /= 2;
				if(width < pnlGraph.Width) width = pnlGraph.Width;
			}

			pctGraph.Width = width;
			DrawGraph();
		}

		//==================================================================
		//メインフォームの起動
		//==================================================================
		private void frmMain_Load(object sender, EventArgs e)
		{
		//	//多重起動の禁止
		//	mut = new System.Threading.Mutex(false, "myMutex");
		//	if(mut.WaitOne(0, false) == false){
		//		this.Close();
		//	}

			//起動時のファイル名取得
			string[] cmds = System.Environment.GetCommandLineArgs();
			if(cmds.Length > 1){
				for( int i=1; i < cmds.Length; i++ ){
					FileOpen(cmds[i]);
				}
			}

			InitGraph();

			//PictureBoxのサイズ設定
			pctGraph.Width = pnlGraph.Width;
			pctGraph.Height = pnlGraph.Height;
		}

		//==================================================================
		//メインフォームの終了
		//==================================================================
		private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			//mut.Close();
		}

		//==================================================================
		//テキスト形式でファイルセーブ
		//==================================================================
		private void menuFileSaveTXT_Click(object sender, EventArgs e)
		{
			string path_txt;
			int i, n=0;

			if (path == ""){
				MessageBox.Show("ファイルがありません");
				return;
			}

			if(lstView.Items.Count == 0){
				return;
			}

			path_txt = path.Substring(0, path.Length - 3) + "TXT";
			//“保存”ダイアログボックス
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.InitialDirectory = path_txt;// txtPath.Text;
			sfd.FileName = path_txt;
			sfd.Filter = "MCR TXTログファイル (*.TXT)|*.TXT|" + "すべてのファイル (*.*)|*.*";
			sfd.FilterIndex = 1;
			if (sfd.ShowDialog() == DialogResult.OK){
				path_txt = sfd.FileName;
			}
			else{
				sfd.Dispose();
				return;
			}
			sfd.Dispose();

			//            
			// テキストデータの書き込み
			//
			System.IO.StreamWriter TextFile;
			TextFile = new System.IO.StreamWriter(path_txt);
			if(txtHead.Text.Length > 0){
				TextFile.WriteLine(txtHead.Text);
			}
					
			TextFile.WriteLine(lblHead1.Text);
			for(i = 0; i < lblHead1.Text.Length; i++){
				TextFile.Write("-");
			}	
			TextFile.WriteLine();
		
			for(n=0; n<lstView.Items.Count; n++){
				TextFile.WriteLine(lstView.Items[n]);
			}
			TextFile.Close();
			TextFile.Dispose();
			MessageBox.Show(path_txt, "書き込み終了");
		}

		//==================================================================
		//メニュー：終了
		//==================================================================
		private void menuFileExit_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		//==================================================================
		//グラフ更新
		//==================================================================
		private void btnOK_Click(object sender, EventArgs e)
		{
			InitGraph();
			DrawGraph();
			if(chkImg.Checked){
				DrawGraph3();
				DrawGraph2(0);
			}
		}

		//==================================================================
		//グラフ初期化
		//==================================================================
		public void InitGraph()
		{
			pnlImage.Visible = chkImg.Checked;
			if(chkImg.Checked)
				pnlGraph.Width = splitContainer1.Panel2.Width - SCROLLBAR_WIDTH - pnlImage.Width;
			else
				pnlGraph.Width = splitContainer1.Panel2.Width - SCROLLBAR_WIDTH;
			pnlGraph.Height = splitContainer1.Panel2.Height - pnlGraph.Top - SCROLLBAR_WIDTH;
			pnlGraph3.Height = splitContainer1.Panel2.Height - pnlGraph3.Top;

			//濃淡グラフのブラシ濃度
			for(i=0; i<16; i++){
				brsh[i] = new SolidBrush(Color.FromArgb(i*17, i*17, i*17));
			}

		}

		//==================================================================
		//グラフ縮尺関連
		//==================================================================
		private void btnToubai_Click(object sender, EventArgs e){
			pctGraph.Width = log_count;
			DrawGraph();
		}
		private void btnX2_Click(object sender, EventArgs e){
			pctGraph.Width = log_count * 2;
			DrawGraph();
		}
		private void btnX4_Click(object sender, EventArgs e){
			pctGraph.Width = log_count * 4;
			DrawGraph();
		}

		private void btnX8_Click(object sender, EventArgs e){
			pctGraph.Width = log_count * 8;
			DrawGraph();
		}

		private void btnX1_Click(object sender, EventArgs e){
			pctGraph.Width = pnlGraph.Width;
			DrawGraph();
		}

		private void btnW2_Click(object sender, EventArgs e){
			pctGraph.Width = pnlGraph.Width * 2;
			DrawGraph();
		}

		private void btnW4_Click(object sender, EventArgs e){
			pctGraph.Width = pnlGraph.Width * 4;
			DrawGraph();
		}

		private void btnW8_Click(object sender, EventArgs e){
			pctGraph.Width = pnlGraph.Width * 8;
			DrawGraph();
		}

		//==================================================================
		//オプションボタン
		//==================================================================
		private void btnGraphOption_Click(object sender, EventArgs e)
		{
			if (frmOption1.ShowDialog() == DialogResult.OK){
				DrawGraph();
			}
		}

		private void chkImg_CheckedChanged(object sender, EventArgs e)
		{
			pnlImage.Visible = chkImg.Checked;
		}
	}

	//==================================================================
	//二重起動の禁止と、最初のインスタンスに後で起動した引数を渡す処理
	//==================================================================
	class myApplication:WindowsFormsApplicationBase{
		public myApplication() : base() {
			this.EnableVisualStyles = true;
			this.IsSingleInstance = true;
			this.MainForm = new frmMain();//スタートアップフォームを設定
			this.StartupNextInstance += 
				new StartupNextInstanceEventHandler(myApplication_StartupNextInstance);
		}
		void myApplication_StartupNextInstance(object sender, StartupNextInstanceEventArgs e) {
			//ここに二重起動されたときの処理を書く
			//e.CommandLineでコマンドライン引数を取得出来る
			frmMain frmMain1 = (frmMain)MainForm;

			//起動時のファイル名取得
			foreach(string cmd in e.CommandLine){
				frmMain1.FileOpen(cmd);
			}
		}
	}
}
