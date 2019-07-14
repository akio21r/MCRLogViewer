//==========================================================================
// ログファイルの読み込み
//==========================================================================
using System;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MCRLogViewer
{
    partial class frmMain
    {
		//==================================================================
		//変数
		//==================================================================
		int		WorkAddress, BuffAddress;
		int		n=0, i;
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

			path = filename;
			txtPath.Text = filename;

			fs = new FileStream(path, FileMode.Open, FileAccess.Read);
			fileSize = (int)fs.Length;		// ファイルのサイズ
			buf = new byte[fileSize];		// データ格納用配列

            //            
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
				n = 0;
                do{
                    Line = TextFile.ReadLine();
                    if(Line == "<END>") break;
                    if(n++ > 128) break;
                    txtHead.Text += Line + System.Environment.NewLine;
                }while(Line != null);
            }

			txtHead.Text += String.Format("Log_Version = {0,3:d3}", LOG_Version) + System.Environment.NewLine;
			txtHead.Select(0, 0);

            TextFile.Close();
            TextFile.Dispose();

            //
            // バイナリログデータの読み込み
            //
            if(c == '#') WorkAddress = TXT_header_sectors * 512;
            else         WorkAddress = 0;
            BuffAddress = 0;

            n = 0;
            fs.Seek(TXT_header_sectors * 512, SeekOrigin.Begin);

			lstView.Hide();
            lstView.Items.Clear();
			readSize = fs.Read(buf, WorkAddress, fileSize - WorkAddress);

			time = 0;			

			//==========================================================
			//ログデータの読み込み
			//==========================================================
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
			}

			//==========================================================
			//==========================================================
			lstView.Show();

			if(LOG_Version >= 9){
				chkImg.Visible = true;
				chkLstImg.Visible = true;
				chkImg.Checked = true;
			}
			else{
				chkImg.Visible = false;
				chkLstImg.Visible = false;
				chkImg.Checked = false;
			}

			//==========================================================
			//画素ログの読み込み [Camera]  LOG_Version==9
			//==========================================================
			lstImg.Hide();
			if(LOG_Version == 9){
				WorkAddress += 512;			//次のセクタへ
				BuffAddress = 0;
				byte[] imgLogBuf = new byte[20];

				lstImg.Items.Clear();
				imgLog_Count = 0;

				for(imgLog_Count=0; WorkAddress + BuffAddress < fileSize - 512; imgLog_Count++){
					str  = new StringBuilder(String.Format("{0, 6}", imgLog_Count));
					str.Append(" ");

					// １レコード分の切り出し
					for(int j=0; j<20; j++){
						imgLogBuf[j] = buf[WorkAddress + BuffAddress++];
						str.Append( imgLogBuf[j].ToString("x2") );
						if(j<=2 || j==18) str.Append( " " );
					}

					// img セクションのログ終了コードを検出したら抜ける
					if( imgLogBuf[0] == 0xfd || imgLogBuf[0] == 0x00 ) break;

					// imgLog[] へのデータ追加
					imgLog[imgLog_Count].Center	= imgLogBuf[1];
					imgLog[imgLog_Count].Sens	= imgLogBuf[2];
					imgLog[imgLog_Count].Sens  &= 0x7f;				// Sensの最上位ビットを消す
					imgLog[imgLog_Count].data	= new byte[32];
					for(int j=0; j<16; j++){
						byte d = imgLogBuf[3+j];
						imgLog[imgLog_Count].data[j*2]   = (byte)((d >> 4) & 0x0f);
						imgLog[imgLog_Count].data[j*2+1] = (byte)(d & 0x0f);
					}

					// sens を追加
					str.Append( "  " );
					byte s = imgLog[imgLog_Count].Sens;
					s <<= 1;
					for(i=0; i<7; i++){
						if((s & 0x80) == 0)
							str.Append("-");
						else
							str.Append("*");
						s <<= 1;
					}

					// lstImg へ追加
					lstImg.Items.Add(str);
				}
				WorkAddress += BuffAddress;

				DrawGraph3();
				
				chkImg.Visible = true;
				chkLstImg.Visible = true;
				chkImg.Checked = true;
			}

			//==========================================================
			//==========================================================
			log_count = n;
			LogFileSize = WorkAddress + 1024;		//実質のサイズを保存用に記録しておく
			
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
			// ハードディスクなら自動保存
			//==========================================================
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
