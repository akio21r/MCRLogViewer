//==========================================================================
// グラフ描画３  （画素データを濃淡で左右の中央にスクロール表示）
//==========================================================================
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.ApplicationServices;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Linq;

namespace MCRLogViewer
{
    partial class frmMain
    {
		//==================================================================
		// Graph3の描画（画素データ）
		//==================================================================
		public void DrawGraph3()
		{
			cur3_show = false;		//カーソルを非表示に

			//ビットマップイメージを解放
			if(pctGraph3.Image != null) pctGraph3.Image.Dispose();

			// pctGraph3のサイズ設定
		//	graph3_vx = (Single)pctGraph3.Width  / (Single)32;				//１画素の幅
		//	graph3_vy = (Single)pctGraph3.Height / (Single)imgLog_Count;	//１画素の高さ
			graph3_vx = 4;
			graph3_vy = 6;
	
		//	pctGraph3.Width = 32 * (int)graph3_vx;
			pctGraph3.Height = imgLog_Count * (int)graph3_vy;
			pnlGraph3.Width = pctGraph3.Width + SCROLLBAR_WIDTH;

			// PictureBoxと同サイズのBitmapオブジェクトを作成
			Bitmap bmp3 = new Bitmap(pctGraph3.Size.Width, pctGraph3.Size.Height);
			pctGraph3.Image = bmp3;
			Graphics g3 = Graphics.FromImage(pctGraph3.Image);
			
			int n, i;
			
			int x0 = pctGraph3.Width / 2;									//中心線
			
			SolidBrush[] brsh = new SolidBrush[16];
			for(i=0; i<16; i++){
				brsh[i] = new SolidBrush(Color.FromArgb(i*17, i*17, i*17));
			}

			g3.DrawLine(Pens.Gray,  x0, 0, x0, pctGraph.Height);

			for(n=0; n<imgLog_Count-1; n++){
				// 画素
				for(i = 0; i<32; i++){
					g3.FillRectangle(brsh[imgLog[n].data[i]], i*graph3_vx, n*graph3_vy, graph3_vx, graph3_vy);
				}

				// 中央値
			//	g3.DrawRectangle(Pens.Red, imgLog[n].Center * vx + vx*3/8, n*vy + vy*3/8 , vx/4, vy/4);
			//	g3.DrawRectangle(Pens.Red, imgLog[n].Center * vx + vx/2, n*vy, 1, vy);
				g3.DrawLine(Pens.Red, imgLog[n].Center * graph3_vx + graph3_vx/2, n*graph3_vy,
					imgLog[n].Center * graph3_vx + graph3_vx/2, (n+1)*graph3_vy);


				// sens を追加
				byte s = imgLog[n].Sens;
				s <<= 1;
				for(i=0; i<7; i++){
					Brush br;
					if((s & 0x80) == 0)
						br = Brushes.Black;
					else{
						switch(i){
							case 3:				//Center
								br = Brushes.Red;
								break;
							case 2: case 4:		//anL, anR
								br = Brushes.Cyan;
								break;
							default:
								br = Brushes.White;
								break;
						}
					}
					s <<= 1;

					g3.FillRectangle(br, i*(graph3_vx*2) + 32*graph3_vx + 12, n*graph3_vy, graph3_vx, graph3_vy-1);
					
				}
			}

			//スクロール
			cur_x = (int)((Single)(lstView.SelectedIndex + 1) * graph_v);
			pnlGraph.AutoScrollPosition = new Point(cur_x - pnlGraph.Width / 2, 0);

//			pctGraph3.Refresh();		// PictureBoxを更新（再描画させる）
			
		//	draw_cursol();
			
			for(i=0; i<16; i++){
				brsh[i].Dispose();
			}
			g3.Dispose();
		}
	}
}
