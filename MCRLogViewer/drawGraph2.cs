//==========================================================================
// グラフ描画２  （画素データを棒グラフで画面上部に表示）
//==========================================================================
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.ApplicationServices;

namespace MCRLogViewer
{
    partial class frmMain
    {
		Bitmap bmp2;
		Graphics g2;
		float graph_vx;
		float graph_vy;

		//==================================================================
		// 画素データのグラフ表示
		//==================================================================
		public void DrawGraph2(int sel){
			int i;
			int x0, y0;
			int[] Camera = new int[33];
			x0 = pctGraph2.Width / 2;	//中央線
			y0 = pctGraph2.Height / 2;	//水平線

			//----------------------------------------------------------------------
			//g2への描画
			//----------------------------------------------------------------------

			//ビットマップイメージを解放
			if(pctGraph2.Image != null) pctGraph2.Image.Dispose();

			// PictureBoxと同サイズのBitmapオブジェクトを作成
			bmp2 = new Bitmap(pctGraph2.Size.Width, pctGraph2.Size.Height);
			pctGraph2.Image = bmp2;
			g2 = Graphics.FromImage(pctGraph2.Image);


			if(LOG_Version < 51){
				//######################################################################
				//棒グラフ
				//######################################################################
				int scaleX = pctGraph2.Width / Camera_N;
				int scaleY = pctGraph2.Height / 16;
	
				g2.FillRectangle(Brushes.Black, 0, 0, pctGraph2.Width, pctGraph2.Height);
	
				//軸描画
				g2.DrawLine(Pens.Gray,          16*scaleX, 0, 16*scaleX, pctGraph2.Height);
				g2.DrawLine(Pens.DarkSlateGray,  8*scaleX, 0,  8*scaleX, pctGraph2.Height);
				g2.DrawLine(Pens.DarkSlateGray, 24*scaleX, 0, 24*scaleX, pctGraph2.Height);

				g2.DrawLine(Pens.Gray,          0,  8*scaleY, pctGraph2.Width,  8*scaleY);
				g2.DrawLine(Pens.DarkSlateGray, 0,  4*scaleY, pctGraph2.Width,  4*scaleY);
				g2.DrawLine(Pens.DarkSlateGray, 0, 12*scaleY, pctGraph2.Width, 12*scaleY);
	
				//Cameraの画像を描画
				for(i=0; i<Camera_N; i++){
					int y = imgLog[sel].data[i] * scaleY;
					g2.FillRectangle(Brushes.Green, i*scaleX, pctGraph2.Height-y, scaleX, y);
					g2.DrawRectangle(Pens.White, i*scaleX, pctGraph2.Height-y, scaleX, y);
				//	g2.DrawRectangle(Pens.Cyan, i*scale, pctGraph2.Height-y, i*scale+scale, pctGraph2.Height);
				}

				// 中央値
				int center_x = imgLog[sel].Center * scaleX;
				g2.DrawLine(Pens.Red, center_x, 0, center_x, pctGraph2.Height);

				// ハーフライン
				if(hlPos > 0){
					int x_hlPos;
					x_hlPos = center_x - hlPos * scaleX;
					g2.DrawLine(Pens.Teal, x_hlPos, 0, x_hlPos, pctGraph2.Height);
					x_hlPos = center_x + hlPos * scaleX;
					g2.DrawLine(Pens.Teal, x_hlPos, 0, x_hlPos, pctGraph2.Height);
				}
			}
			else{
				//######################################################################
				//２Ｄ画像
				//######################################################################
				graph_vx = (float)pctGraph2.Width / (float)GASO_HW;
				graph_vy = (float)pctGraph2.Height / (float)GASO_VW;
				for(int v=0; v<GASO_VW; v++){
					for(int h=0; h<GASO_HW; h++){
						g2.FillRectangle(brsh[imgLog[sel].data[GASO_HW * v + h]], h*graph_vx, v*graph_vy, graph_vx, graph_vy);
					}
				}

				//連続中央線の描画（ビュアー内部で計算した値）
				DrawCenterLines(sel);

				g2.DrawLine(Pens.Red, 0, vPos*graph_vy+graph_vy/2, pctGraph.Width-3, vPos*graph_vy+graph_vy/2);
				g2.DrawLine(Pens.Magenta, 0, vPos2*graph_vy+graph_vy/2, pctGraph.Width-3, vPos2*graph_vy+graph_vy/2);
			//	g2.DrawRectangle(Pens.Red, 0, vPos*graph_vy, pctGraph.Width-3, graph_vy);
			//	g2.DrawRectangle(Pens.Magenta, 0, vPos2*graph_vy, pctGraph.Width-3, graph_vy);

				//センター値
				g2.FillRectangle(Brushes.Magenta, imgLog[sel].Center*graph_vx, vPos*graph_vy, graph_vx, graph_vy);
				g2.DrawRectangle(Pens.Red, imgLog[sel].Center*graph_vx, vPos*graph_vy, graph_vx, graph_vy);
				if(enableCenter2){
					g2.FillRectangle(Brushes.Magenta, imgLog[sel].Center2*graph_vx, vPos2*graph_vy, graph_vx, graph_vy);
					g2.DrawRectangle(Pens.Red, imgLog[sel].Center2*graph_vx, vPos2*graph_vy, graph_vx, graph_vy);
				}

			}
		}
		private void pctGraph2_Click(object sender, EventArgs e)
		{
			CenterIndex[GASO_VW-1] = 16;
			DrawCenterLines(lstView.SelectedIndex);
		}
	}
}

namespace MCRLogViewer
{
    partial class frmMain
    {
		const int GASO_N	=	32;
		const int GL_START	=	0;		//開始画素		5
		const int GL_END	=	31;		//終了画素		27

		struct YamaTani{					//diff[]の山と谷に関する情報
			public int vertex;				//山・谷における頂点
			public int index;				//頂点のIndex
			public int start, end;			//開始Index、終了Index
		}
		YamaTani[] yama = new YamaTani[GASO_N];
		YamaTani[] tani = new YamaTani[GASO_N];

		//==================================================================
		// ２Ｄ画素グラフに中央線を表示
		//==================================================================
		public void DrawCenterLines(int sel){
			byte[]		Sens		= new byte[GASO_VW];
			byte[]		data		= new byte[GASO_HW];
			int[]		diff		= new int[GASO_N];		//差分データ
			int			max, min;			//最大値、最小値
			int			line_left;			//頂上の中央位置算出用
			int			yama_n, tani_n;		//山・谷の数
			int			line_n;
			int[]		lineIdx = new int[16];	//ラインの数, Index
			int			v, h;

			//各種パラメータ
			int			CenterDiff = 4;		//6  4  8

			CenterIndex[vPos] = imgLog[sel].Center;
			for(v=vPos-1; v>=0; v--){
				//##########################################################################
				//画素の1行スキャン
				//32x24画素のうち上から vpos の位置の横32画素分をスキャンし，data[]にセット
				//##########################################################################
				for(h=0; h<GASO_HW; h++){
					data[h] = imgLog[sel].data[GASO_HW * v + h];
				}

				//##########################################################################
				//中央線の検出
				//##########################################################################

				//--------------------------------------------------------------------------
				//各画素の差分をとり、diff[]へ格納する。
				//	diff[0]=d[1]-d[0], diff[30]=d[31]-d[30]
				for(int i=0; i<GASO_N-1; i++){
					diff[i] = (int)data[i+1] - (int)data[i];
				}

				//--------------------------------------------------------------------------
				//閾値を超える山と谷を全て記録する。 ※diffは[0-30]
				yama_n = tani_n = 0;
				for(int i=0; i<GASO_N; i++){
					yama[i].start = yama[i].end = 0;
					tani[i].start = tani[i].end = 0;
				}

				if(diff[0] > thMax){						//左端の山開始点記録
					yama[yama_n].start = 0;
				}
				for(int i=GL_START+1; i<GL_END; i++){		//iは0-31
					//山の開始点と終了地点、数を記録する
					if(diff[i] > thMax){
						if(diff[i-1] <= thMax){
							yama[yama_n].start = i;
						}
					}
					else{
						if(diff[i-1] > thMax){
							yama[yama_n].end = i;
							yama_n++;
						}
					}

					//谷の開始点と終了地点、数を記録する
					if(diff[i] < thMin){
						if(diff[i-1] >= thMin){
							tani[tani_n].start = i;
						}
					}
					else{
						if(diff[i-1] < thMin){
							tani[tani_n].end = i;
							tani_n++;
						}
					}
				}
				//右端の谷終了点記録
				if(tani[tani_n].start != 0 && tani[tani_n].end == 0){
					tani[tani_n].end = GL_END-1;
					tani_n++;
				}

				//それぞれの山の頂点を記録する。
				for(int j=0; j<yama_n; j++){
					yama[j].vertex = diff[yama[j].start];
					yama[j].index  = yama[j].start;
					for(int i=yama[j].start + 1; i<=yama[j].end; i++){
						if(diff[i] > yama[j].vertex){
							yama[j].vertex = diff[i];
							yama[j].index  = i;
						}
					}
				}

				//それぞれの谷の頂点を記録する。
				for(int j=0; j<tani_n; j++){
					tani[j].vertex = diff[tani[j].start];
					tani[j].index  = tani[j].start;
					for(int i=tani[j].start + 1; i<=tani[j].end; i++){
						if(diff[i] < tani[j].vertex){
							tani[j].vertex = diff[i];
							tani[j].index  = i;
						}
					}
				}

				//山の右側に谷があったら、その中間を線とみなす。
				line_n = 0;
				for(int i=0, j=0; i<yama_n && j<tani_n; i++){

					//山の左側の谷を読み飛ばす
					while(tani[j].index <= yama[i].index){
						j++;
						if(j >= tani_n){		//もうこれ以上谷がない
							goto line_comp_detect;
						}
					}
			
					//谷の左側に後続の山が続いている場合
					if(i < yama_n - 1){
						if(yama[i+1].index < tani[j].index){
							continue;
						}
					}

					//左側の山と右側の谷の間隔が開き過ぎていたら、線以外と判断し、次の山へ
				//	if( tani[j].index - yama[i].index > 16){		//12
				//		continue;
				//	}

					//山のすぐ右側に谷がある場合、そこに線があると認定 lineIdx[]にIndex追加
				//	lineIdx[line_n++] = (yama[i].index + tani[j].index) / 2;	//線のIndexを記録
					max = (int)data[yama[i].index];
					line_left = yama[i].index;
					for(int k=yama[i].index+1; k<=tani[j].index; k++){
						if((int)data[k] > max){
							max = (int)data[k];
							lineIdx[line_n] = line_left = k;
						}
						else if((int)data[k] == max){
							lineIdx[line_n] = (k + line_left) / 2;
						}
					}
					g2.FillRectangle(Brushes.Red, lineIdx[line_n]*graph_vx + graph_vx/2 - 1, v*graph_vy + graph_vy/2 - 1, 2, 2);
					line_n++;
				}
				line_comp_detect:
		
				//もし、ラインを一つも検出できなかった場合
				if(line_n == 0){
				//	lineIdx[0] = -99;				//
					lineIdx[0] = CenterIndex[v+1];	//一つ前の値をlineIdx[0]とする
				//	CenterIndex[v] = -99;
				}

				//中央線を検出する
				//lineIdx[] の中で、一つ下の中央線に一番近いものを次の中央線とする
				int df, df_index;
				df = lineIdx[0] - CenterIndex[v+1];
				if(df < 0) df = -df;
				df_index = lineIdx[0];
				min = df;
				for(int i=1; i<line_n; i++){
					df = lineIdx[i] - CenterIndex[v+1];
					if(df < 0) df = -df;
					if(df < min){
						min = df;
						df_index = lineIdx[i];
					}
				}

				//前回との差が大きいものは却下する
				df = df_index - CenterIndex[v+1];
				if(df > -CenterDiff && df < CenterDiff)
					CenterIndex[v] = (byte)df_index;		//次の中央線のIndexを更新する
				else
				//	CenterIndex[v] = -99;
					CenterIndex[v] = CenterIndex[v+1];		//下と同じ

			//	g2.FillRectangle(Brushes.LimeGreen, CenterIndex[v]*graph_vx, v*graph_vy, graph_vx, graph_vy);
				g2.DrawRectangle(Pens.LimeGreen, CenterIndex[v]*graph_vx, v*graph_vy, graph_vx, graph_vy);
				g2.DrawRectangle(Pens.Green, CenterIndex[v]*graph_vx+1, v*graph_vy+1, graph_vx-2, graph_vy-2);

				//ハーフライン読み取り位置
				if(v >= vPos - HL_VW3){
					g2.DrawRectangle(Pens.Teal, (CenterIndex[v] - hlPos)*graph_vx, v*graph_vy, graph_vx, graph_vy);
					g2.DrawRectangle(Pens.Teal, (CenterIndex[v] + hlPos)*graph_vx, v*graph_vy, graph_vx, graph_vy);
				}

			}
		}
	}
}
