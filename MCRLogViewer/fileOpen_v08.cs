//==========================================================================
// LOG_Version = 08  アームセンサ用ログ  2019年度
//==========================================================================
using System;
using System.Text;

namespace MCRLogViewer
{
    partial class frmMain
    {
		public void fileOpen_v08(){
			LogData			l = new LogData();
			StringBuilder	sbSens1;
			byte			sens;
			int				n = 0;

			lblHead2.Text = "                      A   B    C     D   E   F   G   H   I     J         K     L    ";
			lblHead1.Text = "  time mode   sens   hnd ang  sv    vt  v   fl  fr  rl  rr     x  slc  Slope Gyro   ";

			while (WorkAddress < fileSize - 512){
				mode		= (sbyte)buf[WorkAddress + BuffAddress + 0];
				l.mode		= mode;

				sens		= buf[WorkAddress + BuffAddress + 1];
				ErrorCount	= (int)sens;

				l.angle_t	= (sbyte)buf[WorkAddress + BuffAddress + 2];
				l.angle		= (sbyte)buf[WorkAddress + BuffAddress + 3];

				l.sv_pow	= (sbyte)buf[WorkAddress + BuffAddress + 4];
			
				l.vt		= (sbyte)buf[WorkAddress + BuffAddress + 5];
				l.v			= (sbyte)buf[WorkAddress + BuffAddress + 6];
				l.fl		= (sbyte)buf[WorkAddress + BuffAddress + 7];
				l.fr		= (sbyte)buf[WorkAddress + BuffAddress + 8];
				l.rl		= (sbyte)buf[WorkAddress + BuffAddress + 9];
				l.rr		= (sbyte)buf[WorkAddress + BuffAddress + 10];
			
				d_sb			= (sbyte)buf[WorkAddress + BuffAddress + 11];
				l.slope_mode	= (d_sb >> 6) & 0x03;
				l.slope_sw		= (d_sb >> 4) & 0x03;
				l.slope_cnt		= d_sb & 0x0f;

				d_int		=   buf[WorkAddress + BuffAddress + 12];
				d_int		<<= 8;
				d_int		+=  buf[WorkAddress + BuffAddress + 13];
				l.trip		=   d_int;

				l.gyroEx	= (sbyte)buf[WorkAddress + BuffAddress + 14];	//gyroEx
				l.gyro		= (sbyte)buf[WorkAddress + BuffAddress + 15];	//gyro
				l.floor		=        buf[WorkAddress + BuffAddress + 16];

				//ラインセンサ
				sbSens1 = new StringBuilder("  ");

				for(i=0; i<8; i++){
					switch(i){
						case 0: case 1: case 3: case 6: case 7:
							if((sens & 0x80) == 0)
								sbSens1.Append("-");
							else
								sbSens1.Append("*");
							break;
						case 4:
							break;
						case 2: case 5:
							if((sens & 0x80) == 0)
								sbSens1.Append("-");
							else
								sbSens1.Append("+");
							break;
					}		
						
					sens <<= 1;
				}

				str  = new StringBuilder(String.Format("{0, 6}", time));
				time += 5;
				str.Append(String.Format("{0, 4}", l.mode));
				str.Append(sbSens1);
				str.Append(String.Format("{0, 4}", l.angle_t));
				str.Append(String.Format("{0, 4}", l.angle));
				str.Append(String.Format("{0, 5}", l.sv_pow));
				str.Append(String.Format("{0, 6}", l.vt));
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
