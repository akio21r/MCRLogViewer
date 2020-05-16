//==========================================================================
// LOG_Version = 51  Camera Class 用2Dログ  2020.05.09
//==========================================================================
using System;
using System.Text;
using System.Drawing;

namespace MCRLogViewer
{
    partial class frmMain
    {
		//==================================================================
		//==================================================================
		public void fileOpen_v51(){
			int		n = 0, gasoBuffPos;
			lblHead2.Text = "                      K  A   B    C    D  E   F   G   H   I     J         L          ";
			lblHead1.Text = "  time mode    sens  cam hnd ang  sv   vt v   fl  fr  rl  rr     x  slc  Gyr  L   R  ";

			while (WorkAddress < fileSize - 512){
				//----------------------------------------------
				// log[] へのデータセット
				mode			= (sbyte)buf[WorkAddress + BuffAddress + 0];	//mode
				log[n].mode		= mode;

				d_int			=  buf[WorkAddress + BuffAddress + 1];			//time
				d_int			<<= 8;
				d_int			+= buf[WorkAddress + BuffAddress + 2];
				log[n].time		=  d_int;

				sens			= buf[WorkAddress + BuffAddress + 3];			//sens
				ErrorCount		= (int)sens;

				log[n].angle_t	= (sbyte)buf[WorkAddress + BuffAddress + 4];	//handle
				log[n].rl		= (sbyte)buf[WorkAddress + BuffAddress + 5];	//mot_l
				log[n].rr		= (sbyte)buf[WorkAddress + BuffAddress + 6];	//mot_r

				log[n].center	= (sbyte)buf[WorkAddress + BuffAddress + 7];	//cam.Center
				log[n].side		= (sbyte)buf[WorkAddress + BuffAddress + 8];	//cam.halfLine

				//----------------------------------------------
				// imgLog[] へのデータセット
				imgLog[n].Center	= buf[WorkAddress + BuffAddress + 9];
				imgLog[n].data		= new byte[GASO_HW * GASO_VW];
				gasoBuffPos = 0;
				for(int y=0; y<GASO_VW; y++){
					for(int x=0; x<GASO_HW / 2; x++){
						byte d = buf[WorkAddress + BuffAddress + LOG_RecordBytes + gasoBuffPos++];
						imgLog[n].data[GASO_HW * y + x*2]   = (byte)((d >> 4) & 0x0f);
						imgLog[n].data[GASO_HW * y + x*2+1] = (byte)(d & 0x0f);
					}
				}

				//----------------------------------------------
				//ラインセンサを文字列化
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

				//----------------------------------------------
				// 文字情報のセット
				str  = new StringBuilder(String.Format("{0, 6}", log[n].time));
			//	str  = new StringBuilder(String.Format("{0, 6}", time));
			//	time += 5;
				str.Append(String.Format("{0, 4}", log[n].mode));
				str.Append(log[n].sens);
				str.Append(String.Format("{0, 4}", log[n].center));
				str.Append(String.Format("{0, 4}", log[n].angle_t));
				str.Append(String.Format("{0, 4}", log[n].angle));
				str.Append(String.Format("{0, 5}", log[n].sv_pow));
				str.Append(String.Format("{0, 4}", log[n].vt));
				str.Append(String.Format("{0, 3}", log[n].v));
				str.Append(String.Format("{0, 5}", log[n].fl));
				str.Append(String.Format("{0, 4}", log[n].fr));
				str.Append(String.Format("{0, 4}", log[n].rl));
				str.Append(String.Format("{0, 4}", log[n].rr));
				str.Append(String.Format("{0, 7}", log[n].trip));
				str.Append(String.Format("{0, 2}", log[n].slope_mode));
				str.Append(String.Format("{0, 1}", log[n].slope_sw));
				str.Append(String.Format("{0, 1}", log[n].slope_cnt));
				str.Append(String.Format("{0, 5}", log[n].gyro));
				str.Append(String.Format("{0, 4}", log[n].hlCntL));
				str.Append(String.Format("{0, 4}", log[n].hlCntR));

				if(mode == -1)					//終了コード(-1)なら終了
					break;

				else if(mode == -2){			//次のセクタへ
					int ii;
					WorkAddress += 512;
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
					BuffAddress += LOG_RecordBytes + (GASO_HW * GASO_VW / 2);
					lstView.Items.Add(str);
					n++; if (n > 10000) break;
				}

			}

			//==================================================================
			//画素データの読み込み
			//==================================================================
		//	WorkAddress += 512;			//次のセクタへ
		//	BuffAddress = 0;
/*			BuffAddress += (18 + 2);	// 18 + 2;


			for(imgLog_Count=0; WorkAddress + BuffAddress < fileSize - 512; imgLog_Count++){


			}
*/
			line_vPos = 20;
			imgLog_Count = log_count = n;		//ログデータの個数
			WorkAddress += BuffAddress;

			DrawGraph3();
			DrawGraph2(0);
			pnlGraph3.AutoScrollPosition = new Point(0, 0);

			chkImg.Visible = true;
			chkImg.Checked = true;

			//------------------------------
			LogFileSize = WorkAddress + 512;	//実質のサイズ (not 1024)
		}
	}
}
