using System;
using System.Text;

namespace MCRLogViewer
{
    partial class frmMain
    {
		public void fileOpen_v09(){
			lblHead2.Text = "                         A   B    C     D   E   F   G   H   I     J         K    L  ";
			lblHead1.Text = "  time mode   sens   cam hnd ang  sv    vt  v   fl  fr  rl  rr     x  slc  nb  Gyr  ";

			while (WorkAddress < fileSize - 512){
				mode	= (sbyte)buf[WorkAddress + BuffAddress + 0];
				sens	=        buf[WorkAddress + BuffAddress + 1];
				angle_t	= (sbyte)buf[WorkAddress + BuffAddress + 2];
				angle	= (sbyte)buf[WorkAddress + BuffAddress + 3];
				sv_pow	= (sbyte)buf[WorkAddress + BuffAddress + 4];
				vt		= (sbyte)buf[WorkAddress + BuffAddress + 5];
				v		= (sbyte)buf[WorkAddress + BuffAddress + 6];
				fl		= (sbyte)buf[WorkAddress + BuffAddress + 7];
				fr		= (sbyte)buf[WorkAddress + BuffAddress + 8];
				rl		= (sbyte)buf[WorkAddress + BuffAddress + 9];
				rr		= (sbyte)buf[WorkAddress + BuffAddress + 10];
				slope	= (sbyte)buf[WorkAddress + BuffAddress + 11];
				trip	=        buf[WorkAddress + BuffAddress + 12];
				trip	<<= 8;
				trip	+=       buf[WorkAddress + BuffAddress + 13];
				gyroEx	= (sbyte)buf[WorkAddress + BuffAddress + 14];
				gyro	= (sbyte)buf[WorkAddress + BuffAddress + 15];

				floor	= (sbyte)buf[WorkAddress + BuffAddress + 16];

			//	center	= (sbyte)buf[WorkAddress + BuffAddress + 17];	//cam.Center
			//	side	= (sbyte)buf[WorkAddress + BuffAddress + 18];	//cam.halfLine
			//	etc		= (sbyte)buf[WorkAddress + BuffAddress + 19];	//10*cam.LineNum + sci_recvNum
				center	= angle_t;

				ErrorCount = (int)sens;

				log[n].mode			= mode;
				log[n].center		= center;
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

				log[n].gyroEx       = gyroEx;
				log[n].gyro			= gyro;
				log[n].side			= log[n].slope_sw;

				//log_count			= n;

				//ラインセンサ
				log[n].sens = new StringBuilder(" ");
				if((log[n].side & 0x02) != 0) log[n].sens.Append("[");
				else                          log[n].sens.Append(" ");

				for(i=0; i<8; i++){
					switch(i){
						case 0: case 1: case 3: case 6: case 7:
							if((sens & 0x80) == 0)
								log[n].sens.Append("-");
							else
								log[n].sens.Append("*");
							break;
						case 4:
							break;
						case 2: case 5:
							if((sens & 0x80) == 0)
								log[n].sens.Append("-");
							else
								log[n].sens.Append("+");
							break;
					}		
						
					sens <<= 1;
				}
				if((log[n].side & 0x01) != 0) log[n].sens.Append("]");
				else                          log[n].sens.Append(" ");

				str  = new StringBuilder(String.Format("{0, 6}", time));
				time += 5;
				str.Append(String.Format("{0, 4}", log[n].mode));
				str.Append(log[n].sens);
				str.Append(" ");
				str.Append(String.Format("{0, 3}", log[n].center));
				str.Append(" ");
				str.Append(String.Format("{0, 3}", log[n].angle_t));
				str.Append(" ");
				str.Append(String.Format("{0, 3}", log[n].angle));
				str.Append(" ");
				str.Append(String.Format("{0, 4}", log[n].sv_pow));
				str.Append(" ");
				str.Append(String.Format("{0, 4}", log[n].vt));
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
				str.Append(String.Format("{0, 3}", log[n].gyroEx));
				str.Append(String.Format("{0, 5}", log[n].gyro));

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
		}
	}
}
