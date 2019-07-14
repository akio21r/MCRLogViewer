using System;
using System.Text;

namespace MCRLogViewer
{
    partial class frmMain
    {
		public void fileOpen_v01_03(){
			lblHead1.Text = "  time mode  sens   hnd ang  pow  vt   v slc    trip  diff   Batt   Gyro  ";
			lblHead2.Text = "                     A   B    C    D   E                      F      G    ";
				
			while (WorkAddress < fileSize - 512){
				mode	=   (sbyte)buf[WorkAddress + BuffAddress + 0];
				sens	=   buf[WorkAddress + BuffAddress + 1];
				v		=   (sbyte)buf[WorkAddress + BuffAddress + 2];
				vt		=   (sbyte)buf[WorkAddress + BuffAddress + 3];
				angle_t	=   (sbyte)buf[WorkAddress + BuffAddress + 4];
				angle	=   (sbyte)buf[WorkAddress + BuffAddress + 5];
				power	=   (sbyte)buf[WorkAddress + BuffAddress + 6];
				slope	=   (sbyte)buf[WorkAddress + BuffAddress + 7];
				trip	=   buf[WorkAddress + BuffAddress + 8];
				trip	<<= 8;
				trip	+=  buf[WorkAddress + BuffAddress + 9];
				batt	=   buf[WorkAddress + BuffAddress + 10];
				gyro	=   (sbyte)buf[WorkAddress + BuffAddress + 11];

				ErrorCount = (int)sens;

				log[n].mode			= mode;
				log[n].v			= v & 0x3f;
				log[n].vt			= vt;
				log[n].angle		= angle;
				log[n].angle_t		= angle_t;
				log[n].power		= power;
				log[n].slope_mode	= (slope >> 6) & 0x03;
				log[n].slope_sw		= (slope >> 4) & 0x03;
				log[n].slope_cnt	= slope & 0x0f;
				log[n].trip			= trip;
				log[n].batt			= batt;
				log[n].gyro			= gyro;

				//log_count			= n;

				if((v & 0x80) != 0) log[n].sens = new StringBuilder("S");
				else                log[n].sens = new StringBuilder(" ");
				for(i=0; i<8; i++){
					if(i != 4){
						if((sens & 0x80) != 0) log[n].sens.Append("*");
						else                   log[n].sens.Append("-");
					}
					sens <<= 1;
				}
				if((v & 0x40) != 0) log[n].sens.Append("S");
				else                log[n].sens.Append(" ");

				str  = new StringBuilder(String.Format("{0, 6}", time));
				time += 4;
				str.Append(String.Format("{0, 4}", log[n].mode));
				str.Append(log[n].sens);
				str.Append(String.Format("{0, 4}", log[n].angle_t));
				str.Append(" ");
				str.Append(String.Format("{0, 3}", log[n].angle));
				str.Append(" ");
				str.Append(String.Format("{0, 4}", log[n].power));
				str.Append(String.Format("{0, 4}", log[n].vt));
				str.Append(String.Format("{0, 4}", log[n].v));
				str.Append(" ");
				str.Append(String.Format("{0, 1}", log[n].slope_mode));
				str.Append(String.Format("{0, 1}", log[n].slope_sw));
				str.Append(String.Format("{0, 1}", log[n].slope_cnt));
				str.Append(String.Format("{0, 8}", log[n].trip));
				str.Append(String.Format("{0, 6}", log[n].trip));
				str.Append(String.Format("{0, 7:f1}", log[n].batt));
				str.Append(String.Format("{0, 7}", log[n].gyro));

				if (mode == -2)             //次のセクタへ
				{
					int ii;
					WorkAddress += 512;
				//	readSize = fs.Read(buf, WorkAddress, 512);
					BuffAddress = 0;

					time -= 4;

					for(ii=0; ii<ErrorCount; ii++){		//エラーの時はその分空行挿入
						log[n].mode			= 0;
						log[n].v			= 0;
						log[n].vt			= 0;
						log[n].angle		= 0;
						log[n].angle_t		= 0;
						log[n].power		= 0;
						log[n].slope_mode	= 0;
						log[n].slope_sw		= 0;
						log[n].slope_cnt	= 0;
						log[n].trip			= 0;
						log[n].batt			= 0;
						log[n].gyro			= 0;
						
						lstView.Items.Add("Err");
						n++; if (n > 10000) break;
					}
				}
				else
				{
					BuffAddress += 12;
					lstView.Items.Add(str);
					n++; if (n > 10000) break;
				}

				if (mode == 0) break;       //modeが0なら終了
			}
		}
	}
}
