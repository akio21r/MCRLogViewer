using System;
using System.Text;

namespace MCRLogViewer
{
    partial class frmMain
    {
		public void fileOpen_v10(){
			lblHead2.Text = "                     A   B    C    D   E   F   G   H   I     J         K     L                          ";
			lblHead1.Text = "  time mode  sens   hnd ang  sv   vt  v   fl  fr  rl  rr     x  slc  Slope Gyro  L1  L2   L   R  R2  R1 ";

			while (WorkAddress < fileSize - 512){
				mode	=   (sbyte)buf[WorkAddress + BuffAddress + 0];
				sens	=   buf[WorkAddress + BuffAddress + 1];

				angle_t	=   (sbyte)buf[WorkAddress + BuffAddress + 2];
				angle	=   (sbyte)buf[WorkAddress + BuffAddress + 3];

				sv_pow	=	(sbyte)buf[WorkAddress + BuffAddress + 4];
			
				vt		=   (sbyte)buf[WorkAddress + BuffAddress + 5];
				v		=   (sbyte)buf[WorkAddress + BuffAddress + 6];
				fl		=   (sbyte)buf[WorkAddress + BuffAddress + 7];
				fr		=	(sbyte)buf[WorkAddress + BuffAddress + 8];
				rl		=	(sbyte)buf[WorkAddress + BuffAddress + 9];
				rr		=	(sbyte)buf[WorkAddress + BuffAddress + 10];
			
				slope	=   (sbyte)buf[WorkAddress + BuffAddress + 11];
	
				trip	=   buf[WorkAddress + BuffAddress + 12];
				trip	<<= 8;
				trip	+=  buf[WorkAddress + BuffAddress + 13];

				batt	=          buf[WorkAddress + BuffAddress + 14];	//batt
				gyroEx	=   (sbyte)buf[WorkAddress + BuffAddress + 14];	//gyroEx
				gyro	=   (sbyte)buf[WorkAddress + BuffAddress + 15];	//gyro

				anL1	= buf[WorkAddress + BuffAddress + 17];
				anL2	= buf[WorkAddress + BuffAddress + 18];
				anL		= buf[WorkAddress + BuffAddress + 19];
				anR		= buf[WorkAddress + BuffAddress + 20];
				anR2	= buf[WorkAddress + BuffAddress + 21];
				anR1	= buf[WorkAddress + BuffAddress + 22];

				ErrorCount = (int)sens;

				log[n].mode			= mode;
				log[n].angle_t		= angle_t;
				log[n].angle		= angle;
				log[n].sv_pow		= sv_pow;
				log[n].vt			= vt;
				log[n].v			= v;
				log[n].fl			= fl;
				log[n].fr			= fr;
				log[n].rl			= rl;
				log[n].rr			= rr;

				log[n].slope_mode	= (slope >> 6) & 0x03;
				log[n].slope_sw		= (slope >> 4) & 0x03;
				log[n].slope_cnt	= slope & 0x0f;
				log[n].trip			= trip;

				log[n].batt         = batt;

				log[n].gyroEx       = gyroEx;
				log[n].gyro			= gyro;

				log[n].anL1			= anL1;
				log[n].anL2			= anL2;
				log[n].anL			= anL;
				log[n].anR			= anR;
				log[n].anR2			= anR2;
				log[n].anR1			= anR1;

				//ラインセンサ
				log[n].sens = new StringBuilder(" ");
				int s = sens;
				for(i=0; i<8; i++){
					switch(i){
						case 0: case 1: case 3: case 6: case 7:
							if((s & 0x80) == 0)
								log[n].sens.Append("-");
							else
								log[n].sens.Append("*");
							break;
						case 4:
							break;
						case 2: case 5:
							if((s & 0x80) == 0)
								log[n].sens.Append("-");
							else
								log[n].sens.Append("+");
							break;
					}		
						
					s <<= 1;
				}
				log[n].sens.Append(" ");

				str  = new StringBuilder(String.Format("{0, 6}", time));
				time += 5;
				str.Append(String.Format("{0, 4}", log[n].mode));
				str.Append(log[n].sens);
				str.Append(" ");
				str.Append(String.Format("{0, 3}", log[n].angle_t));
				str.Append(" ");
				str.Append(String.Format("{0, 3}", log[n].angle));
				str.Append(" ");
				str.Append(String.Format("{0, 4}", log[n].sv_pow));
				str.Append(" ");
				str.Append(String.Format("{0, 3}", log[n].vt));
				str.Append(String.Format("{0, 4}", log[n].v));
				str.Append(" ");
				str.Append(String.Format("{0, 4}", log[n].fl));
				str.Append(String.Format("{0, 4}", log[n].fr));
				str.Append(String.Format("{0, 4}", log[n].rl));
				str.Append(String.Format("{0, 4}", log[n].rr));
				str.Append(String.Format("{0, 7}", log[n].trip));
				str.Append("  ");
				str.Append(String.Format("{0, 1}", log[n].slope_mode));
				str.Append(String.Format("{0, 1}", log[n].slope_sw));
				str.Append(String.Format("{0, 1}", log[n].slope_cnt));
				str.Append(String.Format("{0, 6}", log[n].gyroEx));
				str.Append(String.Format("{0, 6}", log[n].gyro));

				//アナログセンサ値
				str.Append(String.Format("{0, 4}", log[n].anL1));
				str.Append(String.Format("{0, 4}", log[n].anL2));
				str.Append(String.Format("{0, 4}", log[n].anL ));
				str.Append(String.Format("{0, 4}", log[n].anR ));
				str.Append(String.Format("{0, 4}", log[n].anR2));
				str.Append(String.Format("{0, 4}", log[n].anR1));

				//--------------------------------------------------
				// imgLog[] へのデータ追加
				imgLog[n].Center	= 15;

			//	imgLog[n].Sens		= sens;
			//	imgLog[n].Sens	   &= 0x7f;				// Sensの最上位ビットを消す
				imgLog[n].Sens		= (byte)(((sens >> 1) & 0x70) | sens & 0x0f);
				
				imgLog[n].data		= new byte[32];
				for(int j=0; j<32; j++){
					imgLog[n].data[j]   = 0;
				}
				imgLog[n].data[ 2] = (byte)(log[n].anL1 >> 4);
				imgLog[n].data[ 7] = (byte)(log[n].anL2 >> 4);
				imgLog[n].data[12] = (byte)(log[n].anL  >> 4);
				imgLog[n].data[18] = (byte)(log[n].anR  >> 4);
				imgLog[n].data[23] = (byte)(log[n].anR2 >> 4);
				imgLog[n].data[28] = (byte)(log[n].anR1 >> 4);

				//--------------------------------------------------
				if (mode == -2)             //次のセクタへ
				{
					int ii;
					WorkAddress += 512;
				//	readSize = fs.Read(buf, WorkAddress, 512);
					BuffAddress = 0;

					time -= 5;

					//エラーの時はその数の分空行挿入
					if(LOG_Version >= 2){
						for(ii=0; ii<ErrorCount; ii++){
							log[n].mode			= 0;
							log[n].angle_t		= 0;
							log[n].angle		= 0;
							log[n].sv_pow		= 0;
							log[n].vt			= 0;
							log[n].v			= 0; 
							log[n].fl			= 0;
							log[n].fr			= 0;
							log[n].rl			= 0;
							log[n].rr			= 0;
							log[n].slope_mode	= 0;
							log[n].slope_sw		= 0;
							log[n].slope_cnt	= 0;
							log[n].trip			= 0;
							log[n].batt			= 0;
							log[n].gyroEx       = 0;
							log[n].gyro			= 0;
							log[n].side			= 0;

							lstView.Items.Add("Err");
							n++; if (n > 10000) break;
						}
					}
				}
				else
				{
					BuffAddress += LOG_RecordBytes;
					lstView.Items.Add(str);
					n++; if (n > 10000) break;
				}

				if (mode == 0) break;				//modeが0なら終了
			}

			//画素データ描画
			imgLog_Count = n-1;
			DrawGraph3();
		}
	}
}
