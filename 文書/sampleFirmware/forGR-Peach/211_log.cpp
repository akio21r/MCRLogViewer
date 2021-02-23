//==============================================================================
//SDカードへのログ記録
//==============================================================================
#include "mbed.h"
#include "iodefine.h"
#include "DisplayBace.h"
#include "image_process.h"

#include "FATFileSystem.h"				//FAT file system
#include "SDBlockDevice_GRBoard.h"		//SD Card

#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <string.h>

#include "000_setup.h"
#include "300_camera.h"					//Camera クラス
#include "210_log.h"					//Log クラス

//==================================================================//
//グローバル変数
//==================================================================//
//struct SDLOG_T	LogData[30000]__attribute((section("NC_BSS")));
struct SDLOG_T		LogData[LOG_NUM];	//ログデータ本体
FILE				*fp;				//FILE ポインタ

//==================================================================//
//インスタンス生成
//==================================================================//
SDLOG					sdLog;			//Logクラスのインスタンス生成

FATFileSystem			fs("storage");	//FAT file system
SDBlockDevice_GRBoard	sd;				//SD Card

//==================================================================//
//コンストラクタ
//==================================================================//
SDLOG::SDLOG(void)
{
	data			= LogData;			//ログデータ本体へのポインタ
	log_p			= 0;				//ログデータのindex
	sdEnabled		= false;			//SD Card 使用不可
	recEnabled		= false;			//ログ記録不許可
}

//==================================================================//
//SD Card Init
//==================================================================//
void SDLOG::Init(void)
{
	//----------------------------------------------
	//SDに接続
	if(sd.connect()){
		fs.mount(&sd);
	//	pc.printf("SDBlockDevice: mount\r\n");
		sdEnabled = true;
	}
	else{
	//	pc.printf("SD Error!\r\n");
		sdEnabled = false;
		led_rgb.setMode(LED_MODE_SDERROR);
	}
}

//==================================================================//
//バイナリログ記録  カメラ情報入力後に16ms毎ログ記録
//==================================================================//
void SDLOG::Rec()
{
	if(!sdLog.recEnabled)		//ログ記録不許可の場合は帰る
		return;

	if(log_p < LOG_NUM){
		data[log_p].mode		= ms.mode;
		data[log_p].sens		= cam.getSens();
		data[log_p].sens2		= cam.getSens2();
		data[log_p].cnt0		= (unsigned int)ms.cnt1;
		data[log_p].handle		= (signed char)ms.angle;
		data[log_p].powL		= (signed char)ms.motL;
		data[log_p].powR		= (signed char)ms.motR;
		data[log_p].center		= (signed char)ms.center;
		data[log_p].hl_cIndex	= (cam.getHalfLine() << 6)
								 | cam.getCenterIndex();
		data[log_p].centerIndex2 = cam.getCenterIndex2();

		data[log_p].halfLine	= (cam.getHalfLine1() << 6)
								| (cam.getHalfLine2() << 3)
								| (cam.getHalfLine3());

		data[log_p].ex1			= (unsigned char)cam.ex1;
		data[log_p].ex2			= (unsigned char)cam.ex2;

		//画素データ記録（上位4ビットのみ記録し，２画素分を１バイトに収める）
		int n = 0;
		unsigned char d1, d2;
		for(int y=0; y<GASO_VW; y++){
			for(int x=0; x<GASO_HW; x+=2){
				d1 = ImageComp_B[GASO_HW * y + x  ] & 0xf0;
				d2 = ImageComp_B[GASO_HW * y + x+1] >> 4;
				data[log_p].gaso[n++] = d1 | d2;
			}
		}

		if(++log_p >= LOG_NUM){
			sdLog.recEnd();					//バイナリログ記録終了
			led_out(1);
		}
	}
}

//==================================================================//
//SD Card へログデータ出力
//==================================================================//
void SDLOG::writeLogData(void)
{
	char logBuff[LOG_RECORD_BYTES];				//SDログ記録用バッファ
	unsigned int d;

	if(!sdEnabled) return;

	//----------------------------------------------
	//ファイルポインタを TXT_SECTORSIZE セクタ分後方へ移動
	fseek(fp, 512L * TXT_SECTORSIZE, SEEK_SET);

	//----------------------------------------------
	for(unsigned int i=0; i< log_p; i++){
		logBuff[ 0] = data[i].mode;			//モード

		d = data[i].cnt0;					//走行時間[ms]
		logBuff[ 1] = (d >> 8) & 0xff;
		logBuff[ 2] = d & 0xff;

		logBuff[ 3] = data[i].sens;			//デジタルセンサ
		logBuff[ 4] = data[i].handle;		//指定ハンドル角
		logBuff[ 5] = data[i].powL;			//後左出力
		logBuff[ 6] = data[i].powR;			//後右出力

		logBuff[ 7] = data[i].center;		//カメラのセンター値
		logBuff[ 8] = data[i].hl_cIndex;	//halfLine | centerIndex
		logBuff[ 9] = data[i].sens2;		//遠方のデジタルセンサ
		logBuff[10] = data[i].centerIndex2;	//CenterIndex2(遠方)
		logBuff[11] = data[i].halfLine;		//halfLine

		logBuff[12] = data[i].ex1;			//ext
		logBuff[13] = data[i].ex2;			//ext

		fwrite(logBuff, 1, LOG_RECORD_BYTES, fp);			//log
		fwrite(data[i].gaso, 1, GASO_HW * GASO_VW / 2, fp);	//画素
	}

	//終了コード-1で埋める
	for(int i=0; i<LOG_RECORD_BYTES; i++){
		logBuff[i] = -1;
	}
	fwrite(logBuff, 1, LOG_RECORD_BYTES, fp);

	for(int i=0; i<GASO_HW * GASO_VW / 2; i++){
		data[0].gaso[i] = -1;
	}
	fwrite(data[0].gaso, 1, GASO_HW * GASO_VW / 2, fp);
}

//==================================================================//
//ログ記録開始
//==================================================================//
void SDLOG::recStart(void)
{
	sdLog.recEnabled = true;
}

//==================================================================//
//ログ記録終了
//==================================================================//
void SDLOG::recEnd(void)
{
	sdLog.recEnabled = false;
}

//==================================================================//
//==================================================================//
int SDLOG::sdPrintf(const char *fmt, ...)
{
	va_list argptr;
	int		ret = 0;

	char lbuf[80];

	va_start(argptr, fmt);
	ret = vsprintf( lbuf, fmt, argptr );
	va_end(argptr);

	if( ret > 0 ){		//vsprintfが正常ならfprintfへ転送
		if(sdEnabled) fprintf(fp, lbuf);
	}
	return ret;
}

//==================================================================//
//SD Card へログデータ出力
//==================================================================//
void SDLOG::print_data(void)
{
	char cbuf[9];
	unsigned long	cntA_int_tmp = ms.cntA_int;
	unsigned long	cntA_alw_tmp = ms.cntA_alw;

	ms.flagA.bit.stop = 1;
	sdLog.recEnd();					//バイナリログ記録終了

	trace_mode(0);
	motor2(0, 0);
	ms.mode = 7;
	led_rgb.setMode(LED_MODE_STOP);

	if(!sdEnabled){
		led_rgb.setMode(LED_MODE_SDERROR);
		while(1) always_process();	//SDが使えなければここで動作停止
	}

	//----------------------------------------------
	//ファイル名の決定（ディレクトリ中のファイル数 +1 ）
	char fname[32];				//ファイル名
	int  no=0;					//ファイル番号
	char DIRPATH[]="/storage";
	DIR *dir;
	struct dirent *entry;
	dir = opendir(DIRPATH);
	if ( dir != NULL ) {
		while ( (entry = readdir(dir)) != NULL ) {
			no++;
		//	pc.printf("%s\r\n", entry->d_name);
		}
	}
	closedir(dir);

	sprintf(fname, "/storage/cam%05d.log", no);
//	pc.printf("file name = %s\r\n", fname);

	//----------------------------------------------
	//ファイルオープン
	if(fp = fopen(fname, "wb")){
		sdEnabled = true;
	//	pc.printf("fopen success.\r\n");
	}
	else{
		sdEnabled = false;
	//	pc.printf("fopen error!\r\n");
		led_rgb.setMode(LED_MODE_SDERROR);
		while(1) always_process();	//動作停止
	}

	//----------------------------------------------
	led_rgb.setMode(LED_MODE_SD);
	led_out(3);
//	pc.printf("Log Data Output!\r\n");

	//----------------------------------------------
	//SDへデータ出力
	sdPrintf("#%03d\r\n", LOG_VERSION);		//バージョン記録
	sdPrintf("%03d\r\n", LOG_RECORD_BYTES);	//1レコードのバイト数
	sdPrintf("%d\r\n", TXT_SECTORSIZE);		//TXT領域のセクタサイズ
	sdPrintf("%s\r\n", PROGRAM_VERSION);	//プログラム ver.

	//----------------------------------------------
	if(ms.best_lap == 0xffffffff)
		sprintf(cbuf, "--.---");
	else
		sprintf(cbuf, "%lu.%03lu", ms.best_lap/1000UL, ms.best_lap%1000UL);
	sdPrintf("best_lap=%s\r\n", cbuf);

	sdPrintf("dipsw=0x%02x, timeLimit=%lu, cnt0=%lu\r\n",
		dipsw_get(), ms.timeLimit/1000UL, ms.cnt0/1000UL);
	sdPrintf("h1..12=%3d,%3d,%3d,%3d,%3d, %3d,%3d, %3d,%3d, %3d, %3d,%3d\r\n",
		ms.h1, ms.h2, ms.h3, ms.h4, ms.h5, ms.h6,
		ms.h7, ms.h8, ms.h9, ms.h10, ms.h11, ms.h12);
	sdPrintf("s1..12=%3d,%3d,%3d,%3d,%3d, %3d,%3d, %3d,%3d, %3d,%3d, %3d\r\n",
		ms.s1, ms.s2, ms.s3, ms.s4, ms.s5, ms.s6,
		ms.s7, ms.s8, ms.s9, ms.s10, ms.s11, ms.s12);
	sdPrintf("t1..12=%3d,%3d,%3d,%3d,%3d, %3d,%3d, %3d,%3d, %3d, %3d, %3d\r\n",
		ms.t1, ms.t2, ms.t3, ms.t4, ms.t5, ms.t6,
		ms.t7, ms.t8, ms.t9, ms.t10, ms.t11, ms.t12);

	sdPrintf("handleGain=%.1f, inWheelDown=%d, detect_HL,CR,LC=%d,%d,%d\r\n",
		ms.handleGain, ms.inWheelDown,
		ms.detect_HL, ms.detect_CR, ms.detect_LC);
	sdPrintf("SV_cycle_ms=%d, SV_center=%d, H_step=%d,  ",
		ms.pwm_cycle_ms, ms.servo_center, ms.servo_step);

	sdPrintf("cr_n=%d, lc_n=%d\r\n", ms.cr_n, ms.lc_n);

	sdPrintf("CRANK.power    time   delay      LC.power    time   delay\r\n");
	for(int i=0; i<STRUCT_CR_LC_NUM; i++){
		sdPrintf("[%d]%8d%8d%8d      %8d%8d%8d\r\n", i,
			ms.cr[i].power, ms.cr[i].time, ms.cr[i].delay,
			ms.lc[i].power, ms.lc[i].time, ms.lc[i].delay);
	}
	sdPrintf("in_out=%d, cr_num=%d, lc_num=%d, InCourseStartIndex: CR=%d, LC=%d\r\n",
		ms.in_out, ms.cr_num, ms.lc_num,
		ms.CRInCourseStartIndex, ms.LCInCourseStartIndex);

	sdPrintf("pass_time=%ld, time_cr_clear=%ld, time_lc_clear=%ld\r\n",
		ms.pass_time, ms.time_cr_clear, ms.time_lc_clear);

	sdPrintf("cntA=%3lu%%, int-alw(10s)=%4lu, =%4lu\n",
		(cntA_alw_tmp * 100UL) / cntA_int_tmp,
		(cntA_int_tmp - cntA_alw_tmp) * 10000UL / cntA_int_tmp,
		cntA_int_tmp - cntA_alw_tmp);

	sdPrintf("threshold=%d,%d hlPos=%d, vPos=%d, vPos2=%d, thMax=%d, thMin=%d\r\n",
		cam.threshold, cam.thresholdHL, cam.hlPos, cam.vPos, cam.vPos2,
		cam.th_max, cam.th_min);
	
	sdPrintf("dp2=");
	for(int i=7; i>=0; i--){
		sdPrintf("%d,",cam.dgPoint2[i]);
	}
	sdPrintf("\r\ndp1=");
	for(int i=7; i>=0; i--){
		sdPrintf("%d,",cam.dgPoint[i]);
	}
	sdPrintf("\r\n");

	sdPrintf("BlackOut Up=%lu, Down=%lu\r\n",
		cam.blackOutUp, cam.blackOutDown);

	sdPrintf("\r\n<END>\r\n");

	//----------------------------------------------
	//各時刻毎の記録及び2D画像ログをSD Cardに出力
	//----------------------------------------------
	writeLogData();

	//----------------------------------------------
	fclose(fp);
//	pc.printf("Log Data Output Complete!\r\n");
	led_out(0);
	led_rgb.setMode(LED_MODE_SDEND);
	while(1) always_process();					//動作停止
}

//End of file =========================================================
