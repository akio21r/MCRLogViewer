//==========================================================================
// ログファイルの読み込み
//==========================================================================
using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace MCRLogViewer
{
    partial class frmMain
    {
		//==================================================================
		//変数
		//==================================================================
		int		WorkAddress, BuffAddress;
	//	int		n=0, i;
		int		i;
		int		d_int;		// バッファ読込一時作業用
		sbyte	d_sb;		// バッファ読込一時作業用

        FileStream fs;
		int fileSize;		// ファイルのサイズ
        byte[] buf;			// データ格納用配列

        int readSize;		// Readメソッドで読み込んだバイト数
		int ErrorCount = 0;	// エラーの数

        //パラメータ
		int		time;		// スタートからの経過時間	[ms]
		sbyte   mode;		// モード
        byte    sens;		// デジタルセンサ状態
	
		//==================================================================
		//ファイルを開く
		//==================================================================
		public void FileOpen(string filename)
		{
			//拡張子のチェック
			if(filename.Substring(filename.Length - 4).ToUpper() != ".LOG"){
				MessageBox.Show("MCR用のLOGファイルではありません！");
				return;
			}

			//前回開いたログデータの初期化
			for(i=0; i<max_log_data_counts; i++){
				log[i].mode		= 0;		//mode
				//log[i].sens.Clear();		//センサの状態
				log[i].v		= 0;
				log[i].vt		= 0;		//速度、目標速度
				log[i].angle	= 0;
				log[i].angle_t	= 0;		//ハンドル角、目標角度
				log[i].power	= 0;		//モータ出力
				log[i].sv_pow	= 0;		//サーボモータの出力
				log[i].fl		= 0;
				log[i].fr		= 0;
				log[i].rl		= 0;
				log[i].rr		= 0;		//各輪のモータ出力
				log[i].slope_mode	= 0;	//slope_mode;
				log[i].slope_sw		= 0;	//坂SWの状態
				log[i].slope_cnt	= 0;	//出発してからの坂の数
				log[i].trip		= 0;		//トリップメータ
				log[i].gyro		= 0;
				log[i].gyroEx	= 0;		//ジャイロ出力値
				log[i].side		= 0;		//サイドセンサの状態, ハーフライン
				log[i].time		= 0;		//時間[ms]
				log[i].floor	= 0;		//階

				//Camera用
				log[i].center	= 0;		//Cameraのセンター値
				log[i].etc		= 0;		//他
				log[i].hlCntL	= 0;
				log[i].hlCntR	= 0;		//ハーフライン検出数カウント

				//Remote Sens用
				log[i].anL1	= 0;
				log[i].anL2	= 0;
				log[i].anL	= 0;
				log[i].anR	= 0;
				log[i].anR2	= 0;
				log[i].anR1	= 0;			//anセンサ値

				//以下は現在使っていないもの。
				log[i].pre_sens	= 0;		//先読みセンサ
				log[i].batt		= 0;        //バッテリ電圧
			}
			vPos = 0;
			vPos2 = 0;

			path = filename;
			txtPath.Text = filename;

			fs = new FileStream(path, FileMode.Open, FileAccess.Read);
			fileSize = (int)fs.Length;		// ファイルのサイズ
			buf = new byte[fileSize];		// データ格納用配列

			//==========================================================
			// テキストログの読み込み
			//
			System.IO.StreamReader TextFile;
			TextFile = new System.IO.StreamReader(path, System.Text.Encoding.Default);
			int c;
			c = TextFile.Read();
			txtHead.Clear();

			if(c == '#'){
				string Line;

				//ログのバージョンを読み込む
				Line = TextFile.ReadLine();
				LOG_Version = int.Parse(Line);

				//1レコードのバイト数を決定
				if(LOG_Version <= 3){
					LOG_RecordBytes = 12;
				}
				else if(LOG_Version <= 7){
					Line = TextFile.ReadLine();
					LOG_RecordBytes = int.Parse(Line);
				}
				else{
					Line = TextFile.ReadLine();
					LOG_RecordBytes = int.Parse(Line);
					Line = TextFile.ReadLine();
					TXT_header_sectors = int.Parse(Line);
				}

				//テキストログの読み込み
				i = 0;
				do{
					Line = TextFile.ReadLine();
					if(Line == "<END>") break;
					if(i++ > 128) break;
					txtHead.Text += Line + System.Environment.NewLine;
				}while(Line != null);
				}

			txtHead.Text += String.Format("Log_Version = {0,3:d3}", LOG_Version) + System.Environment.NewLine;
			txtHead.Select(0, 0);

			TextFile.Close();
			TextFile.Dispose();

			//----------------------------------------------
			// テキスト中の特定文字列から値を読み取る
			String	tmpStr;

			// hlPos の値を取得		記述例：hlPos=8,
			tmpStr	= Regex.Match(txtHead.Text, @"hlPos=\d*,").Value;
			if(tmpStr.Length > 0){
				int		p0		= tmpStr.IndexOf("=") + 1;
				int		p1		= tmpStr.Length - 1;
				if(p1-p0 > 0)
					hlPos	= int.Parse(tmpStr.Substring(p0, p1-p0));
				lblHlPos.Text = "hlPos=" + hlPos.ToString();
			}

			// vPos の値を取得		記述例：vPos=20,
			tmpStr	= Regex.Match(txtHead.Text, @"vPos=\d*,").Value;
			if(tmpStr.Length > 0){
				int		p0		= tmpStr.IndexOf("=") + 1;
				int		p1		= tmpStr.Length - 1;
				if(p1-p0 > 0)
					vPos	= int.Parse(tmpStr.Substring(p0, p1-p0));
				if(vPos == 0) vPos = 20;
				lblVPos.Text = "vPos=" + vPos.ToString();
			}

			// vPos2 の値を取得		記述例：vPos2=12,
			tmpStr	= Regex.Match(txtHead.Text, @"vPos2=\d*,").Value;
			if(tmpStr.Length > 0){
				int		p0		= tmpStr.IndexOf("=") + 1;
				int		p1		= tmpStr.Length - 1;
				if(p1-p0 > 0)
					vPos2	= int.Parse(tmpStr.Substring(p0, p1-p0));
			//	if(vPos2 == 0) vPos2 = 12;
				lblVPos2.Text = "vPos2=" + vPos2.ToString();
			}


			//==========================================================
			// バイナリログデータの読み込み
			//
			if(c == '#') WorkAddress = TXT_header_sectors * 512;
			else         WorkAddress = 0;
			BuffAddress = 0;

			fs.Seek(TXT_header_sectors * 512, SeekOrigin.Begin);

			lstView.Hide();
			lstView.Items.Clear();
			readSize = fs.Read(buf, WorkAddress, fileSize - WorkAddress);

			time = 0;

			//ログデータの読み込み
			switch(LOG_Version){
				case  1:
				case  2:
				case  3:
					fileOpen_v01_03();
					break;
				case  4:
				case  5:
				case  6:
				case  7:
					fileOpen_v04_07();
					break;
				case  8: fileOpen_v08(); break;
				case  9: fileOpen_v09(); break;
				case 10: fileOpen_v10(); break;
				case 11: fileOpen_v11(); break;
				case 12: fileOpen_v12(); break;
				case 50: fileOpen_v50(); break;		// C Class
				case 51: fileOpen_v51(); break;		// C Class 2DLog
			}

			//==========================================================
			//==========================================================
			lstView.Show();

			if(LOG_Version >= 9){
				chkImg.Visible = true;
				chkImg.Checked = true;
			}
			else{
				chkImg.Visible = false;
				chkImg.Checked = false;
			}

			//==========================================================
			//==========================================================
			fs.Dispose();
			menuFileSaveTXT.Enabled = true;

			//左側のデータ表示領域のサイズ再設定
			splitContainer1.SplitterDistance = lblHead1.Size.Width + SCROLLBAR_WIDTH;

			//グラフのサイズ調整
			InitGraph();

			//グラフのサイズ調整
			pctGraph.Width = pnlGraph.Width;
			pctGraph.Height = pnlGraph.Height - SCROLLBAR_WIDTH;

			//グラフ描画
			DrawGraph();
			
			btnToubai.Enabled = true;
			btnX2.Enabled = true;
			btnX4.Enabled = true;
			btnX8.Enabled = true;
			
			//==========================================================
			// ハードディスクなら自動保存 (Log_Version < 50 のときのみ)
			//==========================================================
			if(LOG_Version < 50){
				System.IO.DriveType DType;
				string drive_a, drive_b;
				drive_a = path.ToString().Substring(0,1);
				foreach(System.IO.DriveInfo DInfo in System.IO.DriveInfo.GetDrives()){
					DType = DInfo.DriveType;
					drive_b = DInfo.ToString().Substring(0,1);
					if( drive_a == drive_b ){
						if( DType == System.IO.DriveType.Fixed ){
							FileSave();
							menuFileSave.Enabled = true;
						}
						else{
							menuFileSave.Enabled = false;
						}
					}
				}
			}
		}
	}
}
