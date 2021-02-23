//==============================================================================
//SDカードへのログ記録
//==============================================================================
#pragma once

#define	LOG_VERSION			52						//ログのバージョン		51
#define LOG_RECORD_BYTES	14						//1レコードのバイト数	12
#define	TXT_SECTORSIZE		4						//TXT領域のセクタ数
#define	LOG_RECORD_BYTES_A	(LOG_RECORD_BYTES + GASO_HW * GASO_VW / 2)
#define	LOG_GASO_BYTES		(2 + 16*24)				//画素データ 2 + 16*24

//	#define	LOG_NUM			(unsigned long)3600		//約1分間（16.67ms毎の記録）
	#define	LOG_NUM			(unsigned long)7200		//約2分間（16.67ms毎の記録）
//	#define	LOG_NUM			(unsigned long)10798	//約3分間（16.67ms毎の記録）
//	#define	LOG_NUM			(unsigned long)17996	//約5分間（16.67ms毎の記録）

//----------------------------------------------------------------------
//ログ記録関連
//----------------------------------------------------------------------
struct SDLOG_T{								//時系列記録ログ
	unsigned char	mode;					//モードmode
	unsigned char	sens;					//センサの状態
	unsigned char	sens2;					//遠方センサの状態
	unsigned int	cnt0;					//時間
	signed char		handle;					//ステア角
	signed char		powL, powR;				//出力
	unsigned char	halfLine;				//ハーフラインの検出状態

	signed char		center;					//Cente値  -16～0～16
	unsigned char	hl_cIndex;				//halfLine | centerIndex;
	signed char		centerIndex2;			//centerIndex2

	unsigned char	ex1,ex2;				//ext

	unsigned char	gaso[LOG_GASO_BYTES];	//画素データ 2 + 16*24
};

//==============================================================================
//ログ記録関連クラス
//==============================================================================
class SDLOG{
private:
	struct SDLOG_T	*data;				//ログデータ本体へのポインタ
	unsigned int	log_p;				//ログデータのindex

	void			writeLogData();		//SD Card へのログ出力
	int				sdPrintf(const char *fmt, ...);	//SDCardへのprintf

public:
	bool			sdEnabled;			//SD Card 使用可
	bool			recEnabled;			//ログ記録許可

					SDLOG(void);		//コンストラクタ
	void			Init();				//SD Card init
	void			Rec();				//ログ記録
	void			recStart();			//バイナリログ記録スタート
	void			recEnd();			//バイナリログ記録終了
	void			print_data();		//SD Card への設定値等出力
};
extern SDLOG sdLog;

/*------------------------------------------------------------------------------
** ログ記録関連クラスの使い方 **

（１）他のファイル群お同じ場所に下記ファイルを追加
	210_log.h , 211_log.cpp

（２）プロジェクトに下記ファイルを追加
	211_log.cpp

（３）最初に sdLog.Init(); を実行

（４）スタート直後に sdLog.recStart(); を実行し、ログ記録処理を開始する。

（５）intTimer() 関数内で sdLog.Rec(); を実行し、定期的にログデータを記録する。

（６）動作停止後に sdLog.print_data(); を実行し、内部データを SD カードに書きだす。

------------------------------------------------------------------------------*/
