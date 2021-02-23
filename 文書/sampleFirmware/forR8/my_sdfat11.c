//======================================================================
// SD�J�[�h��FAT�A�N�Z�X
// 2009 �����H�ƍ��Z
//======================================================================
//20110529 ���O�̃��R�[�h�T�C�Y���ς�
//20131220 TXT�̈�̃Z�N�^�T�C�Y���ς� ver8
#include	"./_setup.h"			// �}�V���ŗL�̊e��p�����[�^


//======================================================================
// �C���N���[�h
//======================================================================
//#include	<no_float.h>				// stdio�̊ȗ��� �ŏ��ɒu��
#include	<stdio.h>
#include	<stdarg.h>
#include	"lib_microsd.h" 			// microSD����p
#include	"my_sdfat.h"
#include	<string.h>
#include	"my_sci.h"
#include	"205_camera.h"
//#include	"lib_printf.h"

//======================================================================
// �V���{����`
//======================================================================
//#define		TXT_SECTORSIZE		4		//TXT�̈�̃Z�N�^�T�C�Y

//======================================================================
// �O���[�o���ϐ��̐錾
//======================================================================
static int 				sdBuffAddress; 			// �ꎞ�L�^�o�b�t�@�����A�h���X
int 					sdError = 1;			// �ʏ� 0�A�e��G���[������ 1
static unsigned long	sdStartAddress;			// �L�^�J�n�A�h���X
static unsigned long	sdEndAddress;			// �L�^�I���A�h���X
static unsigned long	sdWorkAddress; 			// ��Ɨp�A�h���X
unsigned char			sdError_RecLogCnt=0;	// RecLog�ł̃G���[������
unsigned char			sdError_RecLog=0;		// RecLog�ł̃G���[�����̗L��

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

	unsigned long	adLOG, sizeLOG;		//�o�C�i�����O�̃A�h���X�ƃT�C�Y
	unsigned long	adTXT, sizeTXT;		//�e�L�X�g���O�̃A�h���X�ƃT�C�Y
	unsigned long	adIMG, sizeIMG;		//��f���O�̃A�h���X�ƃT�C�Y [Camera]

	unsigned long	adLOG_DIR;			//�e�L�X�g���O�����\�t�@�C����
										//  ����f�B���N�g���G���g���Z�N
										//  �^�̃A�h���X
	unsigned int	adLOG_DIR_n;		//��L�̃I�t�Z�b�g�i'_'�̏ꏊ�j
	char			logfilename[9];		//TXT�t�@�C����8������
	char 			sdFlag;				// 1:�f�[�^�L�^ 0:�L�^���Ȃ�
} fat;

//======================================================================
// SD�J�[�h�����������A�e������擾
// �@�Ԓl�F0=����@0�ȊO=�G���[
//======================================================================
int sdInitFAT( void )
{
	int i, ret;

	for(i=0; i<9; i++)
		fat.logfilename[i] = '\0';

	//==================================================================
	// microSD������
	//==================================================================
	ret = initMicroSD();
	if(ret){
	//	printf( "\nmicroSD Init Error!!, %d\n", ret );
		return 1;
	}

	//==================================================================
	// MBR,BPB�̓Ǎ�
	//==================================================================
	sdWorkAddress = 0;
	ret = readMicroSD( sdWorkAddress , (signed char*)sdBuff );
	if(ret){
	//	printf( "\nmicroSD Read Error!! (Ad0), %d\n", ret );
		return 2;
	}
	if(sdBuff[0] != (char)0xeb){		//BPB�̓Ǎ�
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
	// BPB���̏��̓Ǎ�
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
	//FAT,�f�B���N�g���G���g��,���[�U�f�[�^�̈�̃A�h���X�v�Z
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
	//�f�B���N�g���G���g���̓Ǎ�
	//==================================================================
	sdWorkAddress = fat.adDIR;
	ret = readMicroSD( sdWorkAddress , (signed char*)sdBuff );
	if(ret){
	//	printf( "\nmicroSD Read Error!! (DIR), %d\n", ret );
		return 4;
	}
	sdBuffAddress = 0;
	while(sdBuff_uchar(sdBuffAddress) != 0x00){
  		if(sdBuff_uchar(sdBuffAddress + 0x0a) == '_'){	//���O�����t�@�C��
			fat.adTXT = fat.adDAT + (unsigned long)fat.bytesPerSector
				* ((unsigned long)(sdBuff_uint(sdBuffAddress + 0x1a) - 2)
				* (unsigned long)fat.sectorsPerCluster);
			fat.sizeTXT = TXT_SECTORSIZE * fat.bytesPerSector;

			fat.adLOG = fat.adTXT + fat.sizeTXT;
			fat.sizeLOG = sdBuff_ulong(sdBuffAddress + 0x1c) - fat.sizeTXT;

			fat.adLOG_DIR = sdWorkAddress;		//'LO_'->'LOG'�ϊ��p
			fat.adLOG_DIR_n = sdBuffAddress + 0x0a;

			for(i=0; i<8; i++)					//LOG�t�@�C�������Z�b�g
				fat.logfilename[i] = sdBuff[sdBuffAddress + i];
			break;	//while()���[�v�𔲂���
		}
		sdBuffAddress += 0x20;
		if(sdBuffAddress > fat.bytesPerSector - 0x20){
			sdWorkAddress += fat.bytesPerSector;
			//�f�B���N�g���G���g������
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
// TXT�t�@�C�����i8�������j���擾
// �@�Ԓl�F������ւ̃|�C���^
//======================================================================
far char* getTXTfilename( void )
{
	return fat.logfilename;
}

//======================================================================
// �������񂾃��O�t�@�C���̊g���q��"�`.LOG"�ɕύX����
// ������.LO_ -> ������.LOG
// �@�Ԓl�F0=����@0�ȊO=�G���[
//======================================================================
int sdChangeLogExt( void )
{
	int ret, n=0;
	if(!fat.adLOG_DIR) return 1;
	do{
		for(ret=0; ret<32760; ret++);
		ret = readMicroSD( fat.adLOG_DIR , (signed char*)sdBuff );
		if(++n > 10){	// 10��ȏ�G���[�ŌJ��Ԃ����甲����
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
// �������ݍς̃��O�t�@�C���̊g���q��"�`.LO_"�ɖ߂�
// ������.LOG -> ������.LO_
// �@�Ԓl�F0=����@0�ȊO=�G���[
//==============================================================
int sdEraseLogExt( void )
{
	sdWorkAddress = fat.adDIR;
	if( readMicroSD(sdWorkAddress, (signed char*)sdBuff) ) return 1;
	sdBuffAddress = 0;
	while(sdBuff_uchar(sdBuffAddress) != 0x00 && sdBuff_uchar(sdBuffAddress) != '_'){
  		if(sdBuff_uchar(sdBuffAddress + 0x0a) == 'G'){			//���O�t�@�C��
			sdBuff[sdBuffAddress + 0x0a] = '_';					//LOG -> LO_
		}
		sdBuffAddress += 0x20;
		if(sdBuffAddress > fat.bytesPerSector - 0x20){			//�o�b�t�@����
			if( writeMicroSD(sdWorkAddress, (signed char*)sdBuff) ) return 2;

			sdWorkAddress += fat.bytesPerSector;
			if(sdWorkAddress >= fat.adDIR + fat.rootEntries * 0x20)
				break;	//�f�B���N�g���G���g�������ɒB������I��

			if( readMicroSD(sdWorkAddress, (signed char*)sdBuff) ) return 3;
			sdBuffAddress = 0;
		}
	}
	if( writeMicroSD(sdWorkAddress, (signed char*)sdBuff) ) return 4;
	return 0;
}

//======================================================================
// ���O�L�^����
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
	fat.sdFlag = 1;					// �f�[�^�L�^�J�n
	
	//printf("sdLogStart(), sdFlag=%d, sdError=%d\n", fat.sdFlag, sdError);
	return 0;
}

// SD�J�[�h�ւ̃��O�L�^�����@��5ms���ɌĂяo������
void sdRecLog( char *log_buf )
{
	int i;
	if( fat.sdFlag == 1 ) {

		//Buff�ɋL�^
		if( sdBuffAddress <= 512 - LOG_RECORD_BYTES - 8){
			for(i=0; i<LOG_RECORD_BYTES; i++)
				sdBuff[ sdBuffAddress++ ] = *log_buf++;
		}

		//Buff����t�ɂȂ��� �� SD����
		if( sdBuffAddress >= 512 - LOG_RECORD_BYTES - 8){
			sdBuff[ sdBuffAddress     ] = -2;
			sdBuff[ sdBuffAddress + 1 ] = sdError_RecLogCnt;
			if(setMicroSDdata() == 12){		//SD���������I�I
				sdBuffAddress = 0;
				sdWorkAddress += fat.bytesPerSector;
				if( sdWorkAddress >= sdEndAddress ) {
					fat.sdFlag = 0;			//�L�^�����I��
				}
				sdError_RecLogCnt = 0;
			}
			else{							//SD�������s
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

	fat.sdFlag = 0;							// �f�[�^�L�^�I��

	i = 0;
	while( checkMicroSDProcess() != 11 ){	// �Ō�̃f�[�^�����҂�
		wait(10);
		if(i++ > 50) break;
	}

	while(sdBuffAddress < 511){				// �c��̃o�b�t�@��0�Ŗ��߂�
		sdBuff[ sdBuffAddress ] = 0x00;
		sdBuffAddress++;
	}

//	p = sdBuff + sdBuffAddress;				// �Ō��1���R�[�h����0x00�Ŗ��߂�
//	for(i=0; i<LOG_RECORD_BYTES; i++)
//		*p++ = 0x00;

	setMicroSDdata();
	sdWorkAddress += fat.bytesPerSector;

	i = 0;
	while( checkMicroSDProcess() != 11 ){	// �Ō�̃f�[�^�����҂�
		wait(10);
		if(i++ > 50) break;
	}

	microSDProcessEnd();					// microSDProcess�I������

	i = 0;
	while( checkMicroSDProcess() != 0 ){	// �I���������I���܂ő҂�
		wait(10);
		if(i++ > 50) break;
	}


	//
	// ��f�f�[�^�̐擪�A�h���X���Z�b�g����B[Camera]
	//
	fat.adIMG   = sdWorkAddress;
	fat.sizeIMG = fat.adLOG + fat.sizeLOG - fat.adIMG;

	return 0;
}

//======================================================================
// SD Print �֘A����
//   �����̏������J�n����O�ɕK�����O�L�^���~�߂Ă����K�v������
//   SD�J�[�h���� *.LO_ �t�@�C���ɕ����o��
//   1) sdPrintInit �t�@�C������ύX�@*.LO_ �� *.LOG
//   2) sdPrint     �����o��
//   3) sdPrintEnd  ������0�t��
//======================================================================

// SD�J�[�h����LOG�t�@�C���ɕ�����o�͂��邽�߂̏������֐�
int sdPrintInit( void )
{
	int ret;

	fat.sdFlag = 0;			// ���O�L�^���~�߂Ă����i�O�̂��߁j
	if(!sdPrintEnabled){
//		printf("sdPrintEnabled=%d\n", sdPrintEnabled);
		return 1;
	}
	ret = sdChangeLogExt();	// TXT���O�t�@�C����1���L����
	if(ret){
		sdError = 1;
//		printf("sdChangeLogExt() Error!! : %d\n", ret);
	}
	adSDPrintTxtImg = fat.adTXT;		// LOG�t�@�C���̐擪�A�h���X
	sdPrintBuffPtr = 0;
	//printf("adSDPrintTxtImg Address :%8lx\n", adSDPrintTxtImg);
	//printf("                End Ad  :%8lx\n", fat.adTXT + fat.sizeTXT - 1);
	return 0;
}

// SD�J�[�h����LOG�t�@�C���ɕ�����o��
int sdPrint( char *lbuf )
{
	int len, i, ret;
	if(!sdPrintEnabled) return 0;
	len = strlen(lbuf);

	// 1�s����TXT�o�b�t�@�փR�s�[
	for(i=0; i<len; i++){
		//sdBuffTxt[ sdPrintBuffPtr++ ] = lbuf[i];
		sdBuff[ sdPrintBuffPtr++ ] = lbuf[i];
		if( sdPrintBuffPtr >= fat.bytesPerSector ){	// SD��TxtLog�t�@�C���ɏ����o��
			ret = writeMicroSD( adSDPrintTxtImg, (signed char*)sdBuff );
			adSDPrintTxtImg += fat.bytesPerSector;
			if( ret ){
			//	printf( "\nsdBuffTxt Write Error!! , %d\n", ret );
				break;
			}
			if( adSDPrintTxtImg >= fat.adTXT + fat.sizeTXT ){	// �����o���悪���t
				sdPrintEnabled = 0;			// ����ȍ~sdPrint�̌Ăяo���s��
				return 1;
			}
   			sdPrintBuffPtr = 0;
		}
	}
	return 0;
}

// sdPrint �I������
void sdPrintEnd( void )
{
	int ret;
	if(!sdPrintEnabled) return;

	while(sdPrintBuffPtr < fat.bytesPerSector){
		sdBuff[ sdPrintBuffPtr++ ] = 0x00;	// �c��̃o�b�t�@��0�Ŗ��߂�
	}

	ret = writeMicroSD( adSDPrintTxtImg, (signed char*)sdBuff );
	if( ret ){
//		printf( "\nsdBuffTxt Write Error!! , %d\n", ret );
	}
	sdPrintEnabled = 0;			// ����ȍ~sdPrint�̌Ăяo���s��
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

	if( ret > 0 ){		// vsprintf������Ȃ�sdPrint�֓]��
		sdPrint(lbuf);
	}
	return ret;
}

#if SENS_CAMERA
//======================================================================
// ��f�f�[�^�̋L�^����
// �L�^�ꏊ�̓o�C�i���f�[�^�̌�
//======================================================================

// ��f�f�[�^���o�͂��邽�߂̏������֐�
int sdImgPrintStart( void )
{
	int ret;

	if(!sdError) sdPrintEnabled = 1;
	else         return 1;
//	if(!sdPrintEnabled) return 1;

	fat.sdFlag = 0;			// ���O�L�^���~�߂Ă����i�O�̂��߁j

	adSDPrintTxtImg	= fat.adIMG;		// IMG�f�[�^�̐擪�A�h���X
	sdPrintBuffPtr		= 0;

	//printf("fat.adIMG Address :%8lx\n", adSDPrintTxtImg);
	//printf("          End Ad  :%8lx\n", fat.adIMG + fat.sizeIMG - 1);
	return 0;
}

// ��f�f�[�^�o��
int sdImgPrint( unsigned char data )
{
	int len, i, ret;

	if(!sdPrintEnabled) return 0;

	//�o�b�t�@�Ƀf�[�^���P�o�C�g���Z�b�g
	sdBuff[ sdPrintBuffPtr++ ] = data;
	
	//�P�Z�N�^�����܂�����ASD�ɏ����o��
	if( sdPrintBuffPtr >= fat.bytesPerSector ){
		Camera_Command_LogSendStop();		// ESP32�Ƀ��O���M��~�v��
		
		ret = writeMicroSD( adSDPrintTxtImg, (signed char*)sdBuff );
		adSDPrintTxtImg += fat.bytesPerSector;

		// �����o���悪���t�Ȃ�A����ȍ~�̌Ăяo���s��
		if( adSDPrintTxtImg >= fat.adIMG + fat.sizeIMG ){
			sdPrintEnabled = 0;
			return 1;
		}
		sdPrintBuffPtr = 0;
		Camera_Command_LogSendStart();		// ESP32�Ƀ��O���M�ĊJ�v��

	}
	return 0;
}

// sdPrint �I������
void sdImgPrintEnd( void )
{
	int i;
	if(!sdPrintEnabled) return;

	// �c��̃o�b�t�@��-3�Ŗ��߂�
	while(sdPrintBuffPtr < fat.bytesPerSector){
		sdBuff[ sdPrintBuffPtr++ ] = -3;
	}
	writeMicroSD( adSDPrintTxtImg, (signed char*)sdBuff );
	adSDPrintTxtImg += fat.bytesPerSector;

	// ���̃Z�N�^���S��-3�Ŗ��߂�
	if( adSDPrintTxtImg < fat.adIMG + fat.sizeIMG - fat.bytesPerSector ){
		for(i=0; i<fat.bytesPerSector; i++){
			sdBuff[ i ] = -3;
		}
		writeMicroSD( adSDPrintTxtImg, (signed char*)sdBuff );
		adSDPrintTxtImg += fat.bytesPerSector;
	}

	sdPrintEnabled = 0;			// ����ȍ~sdPrint�̌Ăяo���s��
}
#endif

void print_SDstate(void)		//SD�֘A����SD�ɓf���o���B
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

// SD�J�[�h���̏����@�A�h���X adr ���� sectors �Z�N�^���\��
void sdReadSector( unsigned long adr, int sectors )
{
/*	int n, i, ret;
	char c;
	sdStartAddress = adr;
	sdEndAddress   = sdStartAddress + (unsigned long)sectors * (unsigned long)fat.bytesPerSector;
	sdWorkAddress  = sdStartAddress;	// �ǂݍ��݊J�n�A�h���X

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
		sdBuffAddress = 0; 		// �z�񂩂�̓ǂݍ��݈ʒu��0��

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
