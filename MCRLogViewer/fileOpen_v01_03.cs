//==========================================================================
// LOG_Version = 01-03
//==========================================================================
using System;
using System.Text;

namespace MCRLogViewer
{
    partial class frmMain
    {
		public void fileOpen_v01_03(){
			LogData l = new LogData();
			int		n = 0;

			lblHead1.Text = "  time mode  sens   hnd ang  pow  vt   v slc    trip  diff   Batt   Gyro  ";
			lblHead2.Text = "                     A   B    C    D   E                      F      G    ";
				
			while (WorkAddress < fileSize - 512){
				mode		= (sbyte)buf[WorkAddress + BuffAddress + 0];
				l.mode		= mode;

				sens		= buf[WorkAddress + BuffAddress + 1];
				ErrorCount	= (int)sens;


				l.v			= (sbyte)buf[WorkAddress + BuffAddress + 2];
				l.vt		= (sbyte)buf[WorkAddress + BuffAddress + 3];
				l.angle_t	= (sbyte)buf[WorkAddress + BuffAddress + 4];
				l.angle		= (sbyte)buf[WorkAddress + BuffAddress + 5];
				l.power		= (sbyte)buf[WorkAddress + BuffAddress + 6];

				d_sb			= (sbyte)buf[WorkAddress + BuffAddress + 7];
				l.slope_mode	= (d_sb >> 6) & 0x03;
				l.slope_sw		= (d_sb >> 4) & 0x03;
				l.slope_cnt		= d_sb & 0x0f;

				d_int			=  buf[WorkAddress + BuffAddress + 8];
				d_int			<<= 8;
				d_int			+= buf[WorkAddress + BuffAddress + 9];
				l.trip			=  d_int;


				l.batt			=   buf[WorkAddress + BuffAddress + 10];
				l.gyro			=   (sbyte)buf[WorkAddress + BuffAddress + 11];

				if((l.v & 0x80) != 0) l.sens = new StringBuilder("S");
				else                  l.sens = new StringBuilder(" ");
				for(i=0; i<8; i++){
					if(i != 4){
						if((sens & 0x80) != 0) l.sens.Append("*");
						else                   l.sens.Append("-");
					}
					sens <<= 1;
				}
				if((l.v & 0x40) != 0) l.sens.Append("S");
				else                  l.sens.Append(" ");

				str  = new StringBuilder(String.Format("{0, 6}", time));
				time += 4;
				str.Append(String.Format("{0, 4}", l.mode));
				str.Append(l.sens);
				str.Append(String.Format("{0, 4}", l.angle_t));
				str.Append(" ");
				str.Append(String.Format("{0, 3}", l.angle));
				str.Append(" ");
				str.Append(String.Format("{0, 4}", l.power));
				str.Append(String.Format("{0, 4}", l.vt));
				str.Append(String.Format("{0, 4}", l.v));
				str.Append(" ");
				str.Append(String.Format("{0, 1}", l.slope_mode));
				str.Append(String.Format("{0, 1}", l.slope_sw));
				str.Append(String.Format("{0, 1}", l.slope_cnt));
				str.Append(String.Format("{0, 8}", l.trip));
				str.Append(String.Format("{0, 6}", l.trip));
				str.Append(String.Format("{0, 7:f1}", l.batt));
				str.Append(String.Format("{0, 7}", l.gyro));

				if (mode == -2)             //次のセクタへ
				{
					int ii;
					WorkAddress += 512;
					BuffAddress = 0;

					time -= 4;

					for(ii=0; ii<ErrorCount; ii++){		//エラーの時はその分空行挿入
						l.mode			= 0;
						l.v				= 0;
						l.vt			= 0;
						l.angle			= 0;
						l.angle_t		= 0;
						l.power			= 0;
						l.slope_mode	= 0;
						l.slope_sw		= 0;
						l.slope_cnt		= 0;
						l.trip			= 0;
						l.batt			= 0;
						l.gyro			= 0;
						
						lstView.Items.Add("Err");
						log.Add(l);
						n++; if (n > max_log_data_counts) break;
					}
				}
				else
				{
					BuffAddress += 12;
					lstView.Items.Add(str);
					log.Add(l);
					n++; if (n > max_log_data_counts) break;
				}

				if (mode == 0) break;       //modeが0なら終了
			//	log.Add(l);
			}
			log.Add(l);

			//------------------------------
			//ログデータの個数，サイズを記録
			log_count = n;						//バイナリログデータの個数
			LogFileSize = WorkAddress + 1024;	//実質のサイズ
		}
	}
}
