//==========================================================================
// LOG_Version = 04-07  アームセンサ用ログ  ポジションセンサ付
//==========================================================================
using System;
using System.Text;

namespace MCRLogViewer
{
    partial class frmMain
    {
		public void fileOpen_v04_07(){
			LogData l = new LogData();
			int		n = 0;
			sbyte	pos_sens;
			int		pre_sens;

			lblHead2.Text     = "                              A   B    C     D   E   F   G   H   I     J         K     L    ";
			if(LOG_Version >= 7)
				lblHead1.Text = "  time mode     sens    pos  hnd ang  sv    vt  v   fl  fr  rl  rr     x  slc  Slope Gyro   ";
			else
				lblHead1.Text = "  time mode     sens    pos  hnd ang  sv    vt  v   fl  fr  rl  rr     x  slc   Batt  Gyro  ";

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

				d_sb		=	(sbyte)buf[WorkAddress + BuffAddress + 16];
				d_sb		>>= 6;
				d_sb		&=  0x03;
				l.side		=   d_sb;

				d_sb		=   (sbyte)buf[WorkAddress + BuffAddress + 16];
				d_sb		>>= 5;
				d_sb		&=  0x01;
				pre_sens	= d_sb;

				pos_sens	=  (sbyte)buf[WorkAddress + BuffAddress + 16];
				pos_sens	&= 0x1f;

				//先読みセンサ
				if(pre_sens == 1)	l.sens = new StringBuilder(" P ");
				else				l.sens = new StringBuilder("   ");

				//ラインセンサ
				if((l.side & 0x02) != 0) l.sens.Append("S");		//★
				else                     l.sens.Append(" ");

				for(i=0; i<8; i++){
					switch(i){
						case 0: case 1: case 3: case 6: case 7:
							if((sens & 0x80) == 0)
								l.sens.Append("-");
							else
								l.sens.Append("*");
							break;
						case 4:
							break;
						case 2: case 5:
							if((sens & 0x80) == 0)
								l.sens.Append("-");
							else
								l.sens.Append("+");
							break;
					}		
						
					sens <<= 1;
				}
				if((l.side & 0x01) != 0) l.sens.Append("S");		//★
				else                          l.sens.Append(" ");

				//ポジションセンサなし
				if(LOG_Version == 4){
					l.sens.Append("      ");
				}
				//ポジションセンサあり
				else{
					l.sens.Append(" ");
					pos_sens <<= 3;
					for(i=0; i<5; i++){
						if((pos_sens & 0x80) == 0)
							l.sens.Append("-");
						else
							l.sens.Append("*");
						pos_sens <<= 1;
					}
				}

				str  = new StringBuilder(String.Format("{0, 6}", time));
				time += 5;
				str.Append(String.Format("{0, 4}", l.mode));
				str.Append(l.sens);
				str.Append(" ");
				str.Append(String.Format("{0, 3}", l.angle_t));
				str.Append(" ");
				str.Append(String.Format("{0, 3}", l.angle));
				str.Append(" ");
				str.Append(String.Format("{0, 4}", l.sv_pow));
				str.Append(" ");
				str.Append(String.Format("{0, 4}", l.vt));
				str.Append(String.Format("{0, 4}", l.v));
				str.Append(" ");
				str.Append(String.Format("{0, 4}", l.fl));
				str.Append(String.Format("{0, 4}", l.fr));
				str.Append(String.Format("{0, 4}", l.rl));
				str.Append(String.Format("{0, 4}", l.rr));
				str.Append(String.Format("{0, 7}", l.trip));
				str.Append("  ");
				str.Append(String.Format("{0, 1}", l.slope_mode));
				str.Append(String.Format("{0, 1}", l.slope_sw));
				str.Append(String.Format("{0, 1}", l.slope_cnt));
				if(LOG_Version >= 7)
					str.Append(String.Format("{0, 6}", l.gyroEx));
				else
					str.Append(String.Format("{0, 6}", l.batt));
				str.Append(String.Format("{0, 6}", l.gyro));

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
							n++; if (n > max_log_data_counts) break;
						}
					}
				}
				else
				{
					BuffAddress += LOG_RecordBytes;
					lstView.Items.Add(str);
					log.Add(l);
					n++; if (n > max_log_data_counts) break;
				}

				if (mode == 0) break;				//modeが0なら終了
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
