//======================================================================
// SDカードのFATアクセス
// 2009 球磨工業高校
//======================================================================
//20110529 ログのレコードサイズを可変に
//20131220 TXT領域のセクタサイズを可変に ver8
#include	"./_setup.h"			// マシン固有の各種パラメータ


//======================================================================
// インクルード
//======================================================================
//#include	<no_float.h>				// stdioの簡略化 最初に置く
#include	<stdio.h>
#include	<stdarg.h>
#include	"lib_microsd.h" 			// microSD制御用
#include	"my_sdfat.h"
#include	<string.h>
#include	"my_sci.h"
#include	"205_camera.h"
//#include	"lib_printf.h"

//======================================================================
// シンボル定義
//======================================================================
//#define		TXT_SECTORSIZE		4		//TXT領域のセクタサイズ

//======================================================================
// グローバル変数の宣言
//======================================================================
static int 				sdBuffAddress; 			// 一時記録バッファ書込アドレス
int 					sdError = 1;			// 通常 0、各種エラー発生時 1
static unsigned long	sdStartAddress;			// 記録開始アドレス
static unsigned long	sdEndAddress;			// 記録終了アドレス
static unsigned long	sdWorkAddress; 			// 作業用アドレス
unsigned char			sdError_RecLogCnt=0;	// RecLogでのエラー発生回数
unsigned char			sdError_RecLog=0;		// RecLogでのエラー発生の有無

int sdPrintBuffPtr, sdPrintTXTsize, sdPrintEnabled = 1;
unsigned long adSDPrintTxtImg;

// FAT16 or FAT12 BPB
#define sdBuff_uchar(a)	(sdBuff[(a)] & 0xff)
#define	sdBuff_uint(a)	((((unsigned int)sdBuff[(a)+1] << 8) & 0xff00) | ((unsigned int)sdBuff[(a)] & 0xff))
#define	sdBuff_ulong(a)	((((unsigned long)sdBuff[(a)+3]<<24)&0xff000000)|(((unsigned long)sdBuff[(a)+2]<<16)&0xff0000)|(((unsigned long)sdBuff[(a)+1]<<8)&0xff00)|((unsigned long)sdBuff[(a)]&0xff))

struct FAT16BPB_t {
	unsigned int	bytesPerSector; 	//bytes/sector
	unsigned int	sectorsPerCluster;  //sectors/cluster
	unsigned int	reservedSectors;	//reserved sector, beginning with sector 0
	unsigned int	numberOfFATs;		//file allocation table
	unsigned int	rootEntries;		//root entry (512)
	unsigned int	sectorsPerFAT;		//sector/FAT (FAT32 always zero: see bigSectorsPerFAT)
	unsigned long	hiddenSectors;		//hidden sector number
	unsigned int	totalSectors;		//partion total secter
	unsigned long	bigTotalSectors;	//total sector number

	unsigned long	adFAT, adDIR, adDAT;//Address

	unsigned long	adLOG, sizeLOG;		//バイナリログのアドレスとサイズ
	unsigned long	adTXT, sizeTXT;		//テキストログのアドレスとサイズ
	unsigned long	adIMG, sizeIMG;		//画素ログのアドレスとサイズ [Camera]

	unsigned long	adLOG_DIR;			//テキストログ書込可能ファイルが
										//  あるディレクトリエントリセク
										//  タのアドレス
	unsigned int	adLOG_DIR_n;		//上記のオフセット（'_'の場所）
	char			logfilename[9];		//TXTファイル名8文字分
	char 			sdFlag;				// 1:データ記録 0:記録しない
} fat;

//======================================================================
// SDカードを初期化し、各種情報を取得
// 　返値：0=正常　0以外=エラー
//======================================================================
int sdInitFAT( void )
{
	int i, ret;

	for(i=0; i<9; i++)
		fat.logfilename[i] = '\0';

	//==================================================================
	// microSD初期化
	//==================================================================
	ret = initMicroSD();
	if(ret){
	//	printf( "\nmicroSD Init Error!!, %d\n", ret );
		return 1;
	}

	//==================================================================
	// MBR,BPBの読込
	//==================================================================
	sdWorkAddress = 0;
	ret = readMicroSD( sdWorkAddress , (signed char*)sdBuff );
	if(ret){
	//	printf( "\nmicroSD Read Error!! (Ad0), %d\n", ret );
		return 2;
	}
	if(sdBuff[0] != (char)0xeb){		//BPBの読込
		//printf("[0]=%x, -> Read BPB\n", (char)sdBuff[0]);
		sdWorkAddress  = (sdBuff[0x1c9] << 24) & 0xff000000;
		sdWorkAddress |= (sdBuff[0x1c8] << 16) & 0x00ff0000;
		sdWorkAddress |= (sdBuff[0x1c7] <<  8) & 0x0000ff00;
		sdWorkAddress |= (sdBuff[0x1c6]      ) & 0x000000ff;
		sdWorkAddress *= 512;
		//printf("sdWorkAddress = %lx\n", sdWorkAddress);
		ret = readMicroSD( sdWorkAddress , (signed char*)sdBuff );
		if(ret){
		//	printf( "\nmicroSD Read Error!! (BPB), %d\n", ret );
			return 3;
		}
	}

	//==================================================================
	// BPB内の情報の読込
	//==================================================================
	fat.bytesPerSector		= sdBuff_uint (0x0b);
	fat.sectorsPerCluster	= sdBuff_uchar(0x0d);
	fat.reservedSectors		= sdBuff_uint (0x0e);
	fat.numberOfFATs		= sdBuff_uchar(0x10);
	fat.rootEntries			= sdBuff_uint (0x11);
	fat.sectorsPerFAT		= sdBuff_uint (0x16);
	fat.hiddenSectors		= sdBuff_ulong(0x1c);
	if(sdBuff_uint(0x13))
		fat.totalSectors	= (unsigned long)sdBuff_uint (0x13) & 0xffff;
	else
		fat.totalSectors	= sdBuff_ulong(0x20);

	//==================================================================
	//FAT,ディレクトリエントリ,ユーザデータ領域のアドレス計算
	//==================================================================
	fat.adFAT = ((unsigned long)fat.hiddenSectors
				+ (unsigned long)fat.reservedSectors)
				* (unsigned long)fat.bytesPerSector;
	fat.adDIR = fat.adFAT + (unsigned long)fat.sectorsPerFAT
				* (unsigned long)fat.numberOfFATs
				* (unsigned long)fat.bytesPerSector;
	fat.adDAT = fat.adDIR + (unsigned long)fat.rootEntries
				* (unsigned long)32;

	//==================================================================
	//ディレクトリエントリの読込
	//==================================================================
	sdWorkAddress = fat.adDIR;
	ret = readMicroSD( sdWorkAddress , (signed char*)sdBuff );
	if(ret){
	//	printf( "\nmicroSD Read Error!! (DIR), %d\n", ret );
		return 4;
	}
	sdBuffAddress = 0;
	while(sdBuff_uchar(sdBuffAddress) != 0x00){
  		if(sdBuff_uchar(sdBuffAddress + 0x0a) == '_'){	//ログ書込ファイル
			fat.adTXT = fat.adDAT + (unsigned long)fat.bytesPerSector
				* ((unsigned long)(sdBuff_uint(sdBuffAddress + 0x1a) - 2)
				* (unsigned long)fat.sectorsPerCluster);
			fat.sizeTXT = TXT_SECTORSIZE * fat.bytesPerSector;

			fat.adLOG = fat.adTXT + fat.sizeTXT;
			fat.sizeLOG = sdBuff_ulong(sdBuffAddress + 0x1c) - fat.sizeTXT;

			fat.adLOG_DIR = sdWorkAddress;		//'LO_'->'LOG'変換用
			fat.adLOG_DIR_n = sdBuffAddress + 0x0a;

			for(i=0; i<8; i++)					//LOGファイル名をセット
				fat.logfilename[i] = sdBuff[sdBuffAddress + i];
			break;	//while()ループを抜ける
		}
		sdBuffAddress += 0x20;
		if(sdBuffAddress > fat.bytesPerSector - 0x20){
			sdWorkAddress += fat.bytesPerSector;
			//ディレクトリエントリ末尾
			if(sdWorkAddress >= fat.adDIR + fat.rootEntries * 0x20){
			//	printf("no useful files.\n");
				return -1;
			}

			ret = readMicroSD( sdWorkAddress , (signed char*)sdBuff );
			if(ret){
			//	printf( "\nmicroSD Read Error!! (DIR), %d\n", ret );
				return 6;
			}
			sdBuffAddress = 0;
		}
	}
	return 0;
}
//======================================================================
// TXTファイル名（8文字分）を取得
// 　返値：文字列へのポインタ
//======================================================================
far char* getTXTfilename( void )
{
	return fat.logfilename;
}

//======================================================================
// 書き込んだログファイルの拡張子を"～.LOG"に変更する
// ○○○.LO_ -> ○○○.LOG
// 　返値：0=正常　0以外=エラー
//======================================================================
int sdChangeLogExt( void )
{
	int ret, n=0;
	if(!fat.adLOG_DIR) return 1;
	do{
		for(ret=0; ret<32760; ret++);
		ret = readMicroSD( fat.adLOG_DIR , (signed char*)sdBuff );
		if(++n > 10){	// 10回以上エラーで繰り返したら抜ける
		//	printf( "\nmicroSD Read Error!! (LO_), %d\n", ret);
			return 2;
		}
	} while(ret);

	if(sdBuff[fat.adLOG_DIR_n] == '_'){
		sdBuff[fat.adLOG_DIR_n] = 'G';
		ret = writeMicroSD( fat.adLOG_DIR , (signed char*)sdBuff );
		if(ret){
		//	printf("\nmicroSD Write Error!! (LO_), %d\n", ret );
			return 3;
		}
	}
	return 0;
}

//==============================================================
// 書き込み済のログファイルの拡張子を"～.LO_"に戻す
// ○○○.LOG -> ○○○.LO_
// 　返値：0=正常　0以外=エラー
//==============================================================
int sdEraseLogExt( void )
{
	sdWorkAddress = fat.adDIR;
	if( readMicroSD(sdWorkAddress, (signed char*)sdBuff) ) return 1;
	sdBuffAddress = 0;
	while(sdBuff_uchar(sdBuffAddress) != 0x00 && sdBuff_uchar(sdBuffAddress) != '_'){
  		if(sdBuff_uchar(sdBuffAddress + 0x0a) == 'G'){			//ログファイル
			sdBuff[sdBuffAddress + 0x0a] = '_';					//LOG -> LO_
		}
		sdBuffAddress += 0x20;
		if(sdBuffAddress > fat.bytesPerSector - 0x20){			//バッファ末尾
			if( writeMicroSD(sdWorkAddress, (signed char*)sdBuff) ) return 2;

			sdWorkAddress += fat.bytesPerSector;
			if(sdWorkAddress >= fat.adDIR + fat.rootEntries * 0x20)
				break;	//ディレクトリエントリ末尾に達したら終了

			if( readMicroSD(sdWorkAddress, (signed char*)sdBuff) ) return 3;
			sdBuffAddress = 0;
		}
	}
	if( writeMicroSD(sdWorkAddress, (signed char*)sdBuff) ) return 4;
	return 0;
}

//======================================================================
// ログ記録処理
//======================================================================
int sdLogStart( void )
{
	int ret;
	sdStartAddress = fat.adLOG;
	sdEndAddress   = sdStartAddress + fat.sizeLOG;
	
	ret = microSDProcessStart( sdStartAddress );
	if(ret){
	//	printf("\nmicroSD microSDProcess Error!!\n");
		return 1;
	}
	sdBuffAddress = 0;
	sdWorkAddress = sdStartAddress;
	fat.sdFlag = 1;					// データ記録開始
	
	//printf("sdLogStart(), sdFlag=%d, sdError=%d\n", fat.sdFlag, sdError);
	return 0;
}

// SDカードへのログ記録処理　※5ms毎に呼び出すこと
void sdRecLog( char *log_buf )
{
	int i;
	if( fat.sdFlag == 1 ) {

		//Buffに記録
		if( sdBuffAddress <= 512 - LOG_RECORD_BYTES - 8){
			for(i=0; i<LOG_RECORD_BYTES; i++)
				sdBuff[ sdBuffAddress++ ] = *log_buf++;
		}

		//Buffが一杯になった → SD書込
		if( sdBuffAddress >= 512 - LOG_RECORD_BYTES - 8){
			sdBuff[ sdBuffAddress     ] = -2;
			sdBuff[ sdBuffAddress + 1 ] = sdError_RecLogCnt;
			if(setMicroSDdata() == 12){		//SD書込成功！！
				sdBuffAddress = 0;
				sdWorkAddress += fat.bytesPerSector;
				if( sdWorkAddress >= sdEndAddress ) {
					fat.sdFlag = 0;			//記録処理終了
				}
				sdError_RecLogCnt = 0;
			}
			else{							//SD書込失敗
				sdError_RecLog = 1;
				sdError_RecLogCnt++;
			}
		}
	}
}

int sdLogEnd( void )
{
	unsigned int i;
	volatile unsigned char *p;

	fat.sdFlag = 0;							// データ記録終了

	i = 0;
	while( checkMicroSDProcess() != 11 ){	// 最後のデータ書込待ち
		wait(10);
		if(i++ > 50) break;
	}

	while(sdBuffAddress < 511){				// 残りのバッファを0で埋める
		sdBuff[ sdBuffAddress ] = 0x00;
		sdBuffAddress++;
	}

//	p = sdBuff + sdBuffAddress;				// 最後に1レコード分を0x00で埋める
//	for(i=0; i<LOG_RECORD_BYTES; i++)
//		*p++ = 0x00;

	setMicroSDdata();
	sdWorkAddress += fat.bytesPerSector;

	i = 0;
	while( checkMicroSDProcess() != 11 ){	// 最後のデータ書込待ち
		wait(10);
		if(i++ > 50) break;
	}

	microSDProcessEnd();					// microSDProcess終了処理

	i = 0;
	while( checkMicroSDProcess() != 0 ){	// 終了処理が終わるまで待つ
		wait(10);
		if(i++ > 50) break;
	}


	//
	// 画素データの先頭アドレスをセットする。[Camera]
	//
	fat.adIMG   = sdWorkAddress;
	fat.sizeIMG = fat.adLOG + fat.sizeLOG - fat.adIMG;

	return 0;
}

//======================================================================
// SD Print 関連処理
//   ※この処理を開始する前に必ずログ記録を止めておく必要がある
//   SDカード内の *.LO_ ファイルに文字出力
//   1) sdPrintInit ファイル名を変更　*.LO_ → *.LOG
//   2) sdPrint     文字出力
//   3) sdPrintEnd  末尾に0付加
//======================================================================

// SDカード内のLOGファイルに文字列出力するための初期化関数
int sdPrintInit( void )
{
	int ret;

	fat.sdFlag = 0;			// ログ記録を止めておく（念のため）
	if(!sdPrintEnabled){
//		printf("sdPrintEnabled=%d\n", sdPrintEnabled);
		return 1;
	}
	ret = sdChangeLogExt();	// TXTログファイルを1つ分有効化
	if(ret){
		sdError = 1;
//		printf("sdChangeLogExt() Error!! : %d\n", ret);
	}
	adSDPrintTxtImg = fat.adTXT;		// LOGファイルの先頭アドレス
	sdPrintBuffPtr = 0;
	//printf("adSDPrintTxtImg Address :%8lx\n", adSDPrintTxtImg);
	//printf("                End Ad  :%8lx\n", fat.adTXT + fat.sizeTXT - 1);
	return 0;
}

// SDカード内のLOGファイルに文字列出力
int sdPrint( char *lbuf )
{
	int len, i, ret;
	if(!sdPrintEnabled) return 0;
	len = strlen(lbuf);

	// 1行分をTXTバッファへコピー
	for(i=0; i<len; i++){
		//sdBuffTxt[ sdPrintBuffPtr++ ] = lbuf[i];
		sdBuff[ sdPrintBuffPtr++ ] = lbuf[i];
		if( sdPrintBuffPtr >= fat.bytesPerSector ){	// SDのTxtLogファイルに書き出し
			ret = writeMicroSD( adSDPrintTxtImg, (signed char*)sdBuff );
			adSDPrintTxtImg += fat.bytesPerSector;
			if( ret ){
			//	printf( "\nsdBuffTxt Write Error!! , %d\n", ret );
				break;
			}
			if( adSDPrintTxtImg >= fat.adTXT + fat.sizeTXT ){	// 書き出し先が満杯
				sdPrintEnabled = 0;			// これ以降sdPrintの呼び出し不可
				return 1;
			}
   			sdPrintBuffPtr = 0;
		}
	}
	return 0;
}

// sdPrint 終了処理
void sdPrintEnd( void )
{
	int ret;
	if(!sdPrintEnabled) return;

	while(sdPrintBuffPtr < fat.bytesPerSector){
		sdBuff[ sdPrintBuffPtr++ ] = 0x00;	// 残りのバッファを0で埋める
	}

	ret = writeMicroSD( adSDPrintTxtImg, (signed char*)sdBuff );
	if( ret ){
//		printf( "\nsdBuffTxt Write Error!! , %d\n", ret );
	}
	sdPrintEnabled = 0;			// これ以降sdPrintの呼び出し不可
	sdPrintTXTsize = adSDPrintTxtImg - fat.adTXT;
}

int sdPrintf(char far *format, ...)
{
	va_list argptr;
//	char	*p;
	int		ret = 0;

	char lbuf[80];

	va_start(argptr, format);
	ret = vsprintf( lbuf, format, argptr );
	va_end(argptr);

	if( ret > 0 ){		// vsprintfが正常ならsdPrintへ転送
		sdPrint(lbuf);
	}
	return ret;
}

#if SENS_CAMERA
//======================================================================
// 画素データの記録処理
// 記録場所はバイナリデータの後
//======================================================================

// 画素データを出力するための初期化関数
int sdImgPrintStart( void )
{
	int ret;

	if(!sdError) sdPrintEnabled = 1;
	else         return 1;
//	if(!sdPrintEnabled) return 1;

	fat.sdFlag = 0;			// ログ記録を止めておく（念のため）

	adSDPrintTxtImg	= fat.adIMG;		// IMGデータの先頭アドレス
	sdPrintBuffPtr		= 0;

	//printf("fat.adIMG Address :%8lx\n", adSDPrintTxtImg);
	//printf("          End Ad  :%8lx\n", fat.adIMG + fat.sizeIMG - 1);
	return 0;
}

// 画素データ出力
int sdImgPrint( unsigned char data )
{
	int len, i, ret;

	if(!sdPrintEnabled) return 0;

	//バッファにデータを１バイト分セット
	sdBuff[ sdPrintBuffPtr++ ] = data;
	
	//１セクタ分埋まったら、SDに書き出し
	if( sdPrintBuffPtr >= fat.bytesPerSector ){
		Camera_Command_LogSendStop();		// ESP32にログ送信停止要求
		
		ret = writeMicroSD( adSDPrintTxtImg, (signed char*)sdBuff );
		adSDPrintTxtImg += fat.bytesPerSector;

		// 書き出し先が満杯なら、これ以降の呼び出し不可
		if( adSDPrintTxtImg >= fat.adIMG + fat.sizeIMG ){
			sdPrintEnabled = 0;
			return 1;
		}
		sdPrintBuffPtr = 0;
		Camera_Command_LogSendStart();		// ESP32にログ送信再開要求

	}
	return 0;
}

// sdPrint 終了処理
void sdImgPrintEnd( void )
{
	int i;
	if(!sdPrintEnabled) return;

	// 残りのバッファを-3で埋める
	while(sdPrintBuffPtr < fat.bytesPerSector){
		sdBuff[ sdPrintBuffPtr++ ] = -3;
	}
	writeMicroSD( adSDPrintTxtImg, (signed char*)sdBuff );
	adSDPrintTxtImg += fat.bytesPerSector;

	// 次のセクタも全て-3で埋める
	if( adSDPrintTxtImg < fat.adIMG + fat.sizeIMG - fat.bytesPerSector ){
		for(i=0; i<fat.bytesPerSector; i++){
			sdBuff[ i ] = -3;
		}
		writeMicroSD( adSDPrintTxtImg, (signed char*)sdBuff );
		adSDPrintTxtImg += fat.bytesPerSector;
	}

	sdPrintEnabled = 0;			// これ以降sdPrintの呼び出し不可
}
#endif

void print_SDstate(void)		//SD関連情報をSDに吐き出す。
{
	sdPrintf("fat.adTXT= %lx, sizeTXT= %lx\n", fat.adTXT, fat.sizeTXT);
	sdPrintf("fat.adLOG= %lx, sizeLOG= %lx\n", fat.adLOG, fat.sizeLOG);
	sdPrintf("fat.adIMG= %lx, sizeIMG= %lx\n", fat.adIMG, fat.sizeIMG);

//printf("\n\n");
//printf("fat.adTXT= %lx, sizeTXT= %lx\n", fat.adTXT, fat.sizeTXT);
//printf("fat.adLOG= %lx, sizeLOG= %lx\n", fat.adLOG, fat.sizeLOG);
//printf("fat.adIMG= %lx, sizeIMG= %lx\n", fat.adIMG, fat.sizeIMG);

/*	sprintf(lbuf, "\n"); sdPrint(lbuf);
	sprintf(lbuf, "bytesPerSector    = %x\n",  fat.bytesPerSector); sdPrint(lbuf);
	sprintf(lbuf, "sectorsPerCluster = %x\n",  fat.sectorsPerCluster); sdPrint(lbuf);
	sprintf(lbuf, "reservedSectors   = %x\n",  fat.reservedSectors); sdPrint(lbuf);
	sprintf(lbuf, "numberOfFATs      = %x\n",  fat.numberOfFATs); sdPrint(lbuf);
	sprintf(lbuf, "rootEntries       = %x\n",  fat.rootEntries); sdPrint(lbuf);
	sprintf(lbuf, "sectorsPerFAT     = %x\n",  fat.sectorsPerFAT); sdPrint(lbuf);
	sprintf(lbuf, "hiddenSectors     = %lx\n", fat.hiddenSectors); sdPrint(lbuf);
	sprintf(lbuf, "totalSectors      = %x\n",  fat.totalSectors); sdPrint(lbuf);
	sprintf(lbuf, "bigTotalSectors   = %lx\n", fat.bigTotalSectors); sdPrint(lbuf);
	sprintf(lbuf, "\nadFAT = %lx\nadDIR = %lx\nadDAT = %lx\n", fat.adFAT, fat.adDIR, fat.adDAT); sdPrint(lbuf);
	sprintf(lbuf, "adLOG = %lx\nadTXT = %lx\n", fat.adLOG, fat.adTXT); sdPrint(lbuf);
	sprintf(lbuf, "sizeLOG = %lx\nsizeTXT = %lx\n", fat.sizeLOG, fat.sizeTXT); sdPrint(lbuf);

	sprintf(lbuf, "fat.adLOG_DIR     = %lx\n", fat.adLOG_DIR); sdPrint(lbuf);
	sprintf(lbuf, "fat.adLOG_DIR_n   = %d\n", fat.adLOG_DIR_n); sdPrint(lbuf);

	sprintf(lbuf, "sdStartAddress = %lx\n", sdStartAddress); sdPrint(lbuf);
	sprintf(lbuf, "sdEndAddress   = %lx\n", sdEndAddress); sdPrint(lbuf);
	sprintf(lbuf, "sdWorkAddress  = %lx\n", sdWorkAddress); sdPrint(lbuf);
	sprintf(lbuf, "sdFlag         = %d\n",  fat.sdFlag); sdPrint(lbuf);
	sprintf(lbuf, "sdError        = %d\n",  sdError); sdPrint(lbuf);
	sprintf(lbuf, "sdPrintTXTsize = %d\n",  sdPrintTXTsize); sdPrint(lbuf);
	sprintf(lbuf, "sdPrintEnabled = %d\n",  sdPrintEnabled); sdPrint(lbuf);
	sprintf(lbuf, "adSDPrintTxtImg= %lx\n", adSDPrintTxtImg); sdPrint(lbuf);
	sprintf(lbuf, "sdBuffAddress  = %d\n", sdBuffAddress); sdPrint(lbuf);
	sprintf(lbuf, "fat.logfilename= %s\n", fat.logfilename); sdPrint(lbuf);
*/
}

void sdPrintFat( void )
{
/*	printf("\n");
	printf("bytesPerSector    = %x\n",  fat.bytesPerSector);
	printf("sectorsPerCluster = %x\n",  fat.sectorsPerCluster);
	printf("reservedSectors   = %x\n",  fat.reservedSectors);
	printf("numberOfFATs      = %x\n",  fat.numberOfFATs);
	printf("rootEntries       = %x\n",  fat.rootEntries);
	printf("sectorsPerFAT     = %x\n",  fat.sectorsPerFAT);
	printf("hiddenSectors     = %lx\n", fat.hiddenSectors);
	printf("totalSectors      = %x\n",  fat.totalSectors);
	printf("bigTotalSectors   = %lx\n", fat.bigTotalSectors);
	printf("\nadFAT = %lx\nadDIR = %lx\nadDAT = %lx\n", fat.adFAT, fat.adDIR, fat.adDAT);
	printf("adLOG = %lx\nadTXT = %lx\n", fat.adLOG, fat.adTXT);
	printf("sizeLOG = %lx\nsizeTXT = %lx\n", fat.sizeLOG, fat.sizeTXT);
*/
}

void sdClearBuff(void)
{
	int i;
	for(i=0; i<fat.bytesPerSector; i++) sdBuff[i] = 0;
}

// SDカード内の情報を　アドレス adr から sectors セクタ分表示
void sdReadSector( unsigned long adr, int sectors )
{
/*	int n, i, ret;
	char c;
	sdStartAddress = adr;
	sdEndAddress   = sdStartAddress + (unsigned long)sectors * (unsigned long)fat.bytesPerSector;
	sdWorkAddress  = sdStartAddress;	// 読み込み開始アドレス

	while(1){
		if( sdWorkAddress >= sdEndAddress ) {
		//	printf( "\nend.\n" );
			break;
		}
		ret = readMicroSD( sdWorkAddress , (signed char*)sdBuff );
		if( ret != 0x00 ) {
		//	printf( "\nmicroSD Read Error!! , %d\n", ret );
			break;
		}
		sdBuffAddress = 0; 		// 配列からの読み込み位置を0に

		while(1){
			for(n=0; n<4; n++){
			//	printf("%08lx: ", sdWorkAddress + sdBuffAddress);
				for(i=0; i<16; i++){
			//		printf("%02x ", 0xff & sdBuff[sdBuffAddress + i]);
			//		if(i == 7) printf(" ");
				}
	   			for(i=0; i<16; i++){
					c = 0xff & sdBuff[sdBuffAddress + i];
			//		if(c >= 0x20 && c <= 0x7e) printf("%c", c);
			//		else					   printf(".");
				}
			//	printf("\n");
				sdBuffAddress += 16;
			}

			if( sdBuffAddress >= fat.bytesPerSector ) {
				break;
			}
		}
		sdWorkAddress += fat.bytesPerSector;
	}
*/
}
// End of file =========================================================
