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
			LogData			l		= new LogData();
			ImgLogData		imgl	= new ImgLogData();
			StringBuilder	sbSens1, sbSens2;
			byte			sens;
			int				n		= 0;
			int				gasoBuffPos;
			byte			tmp;

			lblHead2.Text = "                               A   B     F    G                   ";
			lblHead1.Text = "  time mode   sens2     sens  cam hnd    L    R                   ";

			//遠方のセンター値が有効かどうかをセット
			if(LOG_RecordBytes >= 11)	enableCenter2 = true;
			else						enableCenter2 = false;

			while (WorkAddress < fileSize - 512){
				//----------------------------------------------
				// log[] へのデータセット
				mode			= (sbyte)buf[WorkAddress + BuffAddress + 0];	//mode
				l.mode			= mode;

				d_int			=  buf[WorkAddress + BuffAddress + 1];			//time
				d_int			<<= 8;
				d_int			+= buf[WorkAddress + BuffAddress + 2];
				l.time			=  d_int;

				l.angle			= (sbyte)buf[WorkAddress + BuffAddress + 4];	//handle
				l.fl			= (sbyte)buf[WorkAddress + BuffAddress + 5];	//mot_l
				l.fr			= (sbyte)buf[WorkAddress + BuffAddress + 6];	//mot_r

				l.center		= (sbyte)buf[WorkAddress + BuffAddress + 7];	//cam.Center
				l.side			= (sbyte)buf[WorkAddress + BuffAddress + 8];	//cam.halfLine

				tmp				= buf[WorkAddress + BuffAddress + 8];		//halfLine | centerIndex
				l.side			= tmp >> 6;
				imgl.Center		= (byte)(tmp & 0x3f);

				if(enableCenter2)
					imgl.Center2	= buf[WorkAddress + BuffAddress + 10];
				else
					imgl.Center2	= 255;
				
				//==================================================================
				//画素データの読み込み
				//==================================================================
				byte s			= buf[WorkAddress + BuffAddress + 3];
				byte s1			= (byte)((s >> 1) & 0x70);
				byte s2			= (byte)(s & 0x0f);
				imgl.Sens		= (byte)(s1 | s2);

				imgl.data		= new byte[GASO_HW * GASO_VW];
				gasoBuffPos 	= 0;
				for(int y=0; y<GASO_VW; y++){
					for(int x=0; x<GASO_HW / 2; x++){
						byte d = buf[WorkAddress + BuffAddress + LOG_RecordBytes + gasoBuffPos++];
						imgl.data[GASO_HW * y + x*2]   = (byte)((d >> 4) & 0x0f);
						imgl.data[GASO_HW * y + x*2+1] = (byte)(d & 0x0f);
					}
				}

				//----------------------------------------------
				//sens を文字列化
				sens			= buf[WorkAddress + BuffAddress + 3];	//sens
				ErrorCount		= (int)sens;

				sbSens1 = new StringBuilder(" ");
				if((l.side & 0x02) != 0) sbSens1.Append("[");
				else                     sbSens1.Append(" ");

				for(i=0; i<8; i++){
					switch(i){
						case 0: case 1: case 2:  case 5: case 6: case 7:
							if((sens & 0x80) == 0)	sbSens1.Append("-");
							else				sbSens1.Append("*");
							break;
						case 3:
							if((sens & 0x80) == 0)	sbSens1.Append("-");
							else				sbSens1.Append("+");
							break;
						case 4:
							break;
					}
					sens <<= 1;
				}
				if((l.side & 0x01) != 0) sbSens1.Append("]");
				else                     sbSens1.Append(" ");

				//----------------------------------------------
				//sens2 を文字列化
				sens			= buf[WorkAddress + BuffAddress + 9];	//sens2
			//	ErrorCount		= (int)sens;

				sbSens2 = new StringBuilder(" ");
			//	if((l.side & 0x02) != 0) sbSens2.Append("[");
			//	else                     sbSens2.Append(" ");
				sbSens2.Append(" ");

				for(i=0; i<8; i++){
					switch(i){
						case 0: case 1: case 2: case 5: case 6: case 7:
							if((sens & 0x80) == 0)	sbSens2.Append("-");
							else					sbSens2.Append("*");
							break;
						case 3:
							if((sens & 0x80) == 0)	sbSens2.Append("-");
							else					sbSens2.Append("+");
							break;
						case 4:
							break;
					}		
						
					sens <<= 1;
				}
			//	if((l.side & 0x01) != 0) sbSens2.Append("]");
			//	else                     sbSens2.Append(" ");
				sbSens2.Append(" ");

				//----------------------------------------------
				// 文字情報のセット
				str  = new StringBuilder(String.Format("{0, 6}", l.time));
				str.Append(String.Format("{0, 4}", l.mode));
				str.Append(sbSens2);
				str.Append(sbSens1);
				str.Append(String.Format("{0, 4}", l.center));
				str.Append(String.Format("{0, 4}", l.angle));
				str.Append(String.Format("{0, 5}", l.fl));
				str.Append(String.Format("{0, 5}", l.fr));

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
					BuffAddress += LOG_RecordBytes + (GASO_HW * GASO_VW / 2);
					lstView.Items.Add(str);
					log.Add(l);
					imgLog.Add(imgl);
					n++; if (n > max_log_data_counts) break;
				}
			//	log.Add(l);
			//	imgLog.Add(imgl);
			}
			log.Add(l);
			imgLog.Add(imgl);

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
