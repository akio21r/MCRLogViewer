//==========================================================================
// LOG_Version = 10  RemoteSens用ログ
//==========================================================================
using System;
using System.Text;
using System.Drawing;

namespace MCRLogViewer
{
    partial class frmMain
    {
		public void fileOpen_v10(){
			LogData		l		= new LogData();
			ImgLogData	imgl	= new ImgLogData();
			int			n		= 0;

			lblHead2.Text = "                     A   B    C    D   E   F   G   H   I     J         K     L                          ";
			lblHead1.Text = "  time mode  sens   hnd ang  sv   vt  v   fl  fr  rl  rr     x  slc  Slope Gyro  L1  L2   L   R  R2  R1 ";

			while (WorkAddress < fileSize - 512){
				mode		= (sbyte)buf[WorkAddress + BuffAddress + 0];
				l.mode		= mode;

				sens		= buf[WorkAddress + BuffAddress + 1];
				ErrorCount	= (int)sens;

				l.angle_t	=   (sbyte)buf[WorkAddress + BuffAddress + 2];
				l.angle		=   (sbyte)buf[WorkAddress + BuffAddress + 3];

				l.sv_pow	=	(sbyte)buf[WorkAddress + BuffAddress + 4];
			
				l.vt		=   (sbyte)buf[WorkAddress + BuffAddress + 5];
				l.v			=   (sbyte)buf[WorkAddress + BuffAddress + 6];
				l.fl		=   (sbyte)buf[WorkAddress + BuffAddress + 7];
				l.fr		=	(sbyte)buf[WorkAddress + BuffAddress + 8];
				l.rl		=	(sbyte)buf[WorkAddress + BuffAddress + 9];
				l.rr		=	(sbyte)buf[WorkAddress + BuffAddress + 10];
			
				d_sb			= (sbyte)buf[WorkAddress + BuffAddress + 11];
				l.slope_mode	= (d_sb >> 6) & 0x03;
				l.slope_sw		= (d_sb >> 4) & 0x03;
				l.slope_cnt		= d_sb & 0x0f;

				d_int		=  buf[WorkAddress + BuffAddress + 12];
				d_int		<<= 8;
				d_int		+= buf[WorkAddress + BuffAddress + 13];
				l.trip		=  d_int;

				l.batt		=          buf[WorkAddress + BuffAddress + 14];	//batt
				l.gyroEx	=   (sbyte)buf[WorkAddress + BuffAddress + 14];	//gyroEx
				l.gyro		=   (sbyte)buf[WorkAddress + BuffAddress + 15];	//gyro

				l.anL1		= buf[WorkAddress + BuffAddress + 17];
				l.anL2		= buf[WorkAddress + BuffAddress + 18];
				l.anL		= buf[WorkAddress + BuffAddress + 19];
				l.anR		= buf[WorkAddress + BuffAddress + 20];
				l.anR2		= buf[WorkAddress + BuffAddress + 21];
				l.anR1		= buf[WorkAddress + BuffAddress + 22];

				//ラインセンサ
				l.sens = new StringBuilder(" ");
				int s = sens;
				for(i=0; i<8; i++){
					switch(i){
						case 0: case 1: case 3: case 6: case 7:
							if((s & 0x80) == 0)
								l.sens.Append("-");
							else
								l.sens.Append("*");
							break;
						case 4:
							break;
						case 2: case 5:
							if((s & 0x80) == 0)
								l.sens.Append("-");
							else
								l.sens.Append("+");
							break;
					}		
						
					s <<= 1;
				}
				l.sens.Append(" ");

				str  = new StringBuilder(String.Format("{0, 6}", time));
				time += 5;
				str.Append(String.Format("{0, 4}", l.mode));
				str.Append(l.sens);
				str.Append(String.Format("{0, 4}", l.angle_t));
				str.Append(String.Format("{0, 4}", l.angle));
				str.Append(String.Format("{0, 5}", l.sv_pow));
				str.Append(String.Format("{0, 4}", l.vt));
				str.Append(String.Format("{0, 4}", l.v));
				str.Append(String.Format("{0, 5}", l.fl));
				str.Append(String.Format("{0, 4}", l.fr));
				str.Append(String.Format("{0, 4}", l.rl));
				str.Append(String.Format("{0, 4}", l.rr));
				str.Append(String.Format("{0, 7}", l.trip));
				str.Append(String.Format("{0, 3}", l.slope_mode));
				str.Append(String.Format("{0, 1}", l.slope_sw));
				str.Append(String.Format("{0, 1}", l.slope_cnt));
				str.Append(String.Format("{0, 6}", l.gyroEx));
				str.Append(String.Format("{0, 6}", l.gyro));

				//アナログセンサ値
				str.Append(String.Format("{0, 4}", l.anL1));
				str.Append(String.Format("{0, 4}", l.anL2));
				str.Append(String.Format("{0, 4}", l.anL ));
				str.Append(String.Format("{0, 4}", l.anR ));
				str.Append(String.Format("{0, 4}", l.anR2));
				str.Append(String.Format("{0, 4}", l.anR1));

				//--------------------------------------------------
				// imgLog[] へのデータ追加
				imgl.Center	= 15;

				imgl.Sens		= (byte)(((sens >> 1) & 0x70) | sens & 0x0f);
				
				imgl.data		= new byte[32];
				for(int j=0; j<32; j++){
					imgl.data[j]   = 0;
				}
				imgl.data[ 2] = (byte)(l.anL1 >> 4);
				imgl.data[ 7] = (byte)(l.anL2 >> 4);
				imgl.data[12] = (byte)(l.anL  >> 4);
				imgl.data[18] = (byte)(l.anR  >> 4);
				imgl.data[23] = (byte)(l.anR2 >> 4);
				imgl.data[28] = (byte)(l.anR1 >> 4);

				//--------------------------------------------------
				if (mode == -2)             //次のセクタへ
				{
					int ii;
					WorkAddress += 512;
					BuffAddress = 0;

					time -= 5;

					//エラーの時はその数の分空行挿入
					if(LOG_Version >= 2){
						for(ii=0; ii<ErrorCount; ii++){
							l.mode			= 0;
							l.angle_t		= 0;
							l.angle			= 0;
							l.sv_pow		= 0;
							l.vt			= 0;
							l.v				= 0; 
							l.fl			= 0;
							l.fr			= 0;
							l.rl			= 0;
							l.rr			= 0;
							l.slope_mode	= 0;
							l.slope_sw		= 0;
							l.slope_cnt		= 0;
							l.trip			= 0;
							l.batt			= 0;
							l.gyroEx		= 0;
							l.gyro			= 0;
							l.side			= 0;

							lstView.Items.Add("Err");
							log.Add(l);
							imgLog.Add(imgl);
							n++; if (n > max_log_data_counts) break;
						}
					}
				}
				else
				{
					BuffAddress += LOG_RecordBytes;
					lstView.Items.Add(str);
					log.Add(l);
					imgLog.Add(imgl);
					n++; if (n > max_log_data_counts) break;
				}

				if (mode == 0) break;				//modeが0なら終了
			//	log.Add(l);
			//	imgLog.Add(imgl);
			}
			log.Add(l);
			imgLog.Add(imgl);

			//画素データ描画
			imgLog_Count = n-1;
			DrawGraph3();
			DrawGraph2(0);
			pnlGraph3.AutoScrollPosition = new Point(0, 0);

			//------------------------------
			//ログデータの個数，サイズを記録
			log_count = n;						//バイナリログデータの個数
			LogFileSize = WorkAddress + 1024;	//実質のサイズ
		}
	}
}
