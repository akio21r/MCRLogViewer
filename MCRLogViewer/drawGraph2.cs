//==========================================================================
// グラフ描画２  （画素データを棒グラフで画面上部に表示）
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
			Bitmap bmp2 = new Bitmap(pctGraph2.Size.Width, pctGraph2.Size.Height);
			pctGraph2.Image = bmp2;
			Graphics g2 = Graphics.FromImage(pctGraph2.Image);


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
					g2.DrawLine(Pens.DarkMagenta, x_hlPos, 0, x_hlPos, pctGraph2.Height);
					x_hlPos = center_x + hlPos * scaleX;
					g2.DrawLine(Pens.DarkMagenta, x_hlPos, 0, x_hlPos, pctGraph2.Height);
				}
			}
			else{
				//######################################################################
				//２Ｄ画像
				//######################################################################
				float graph_vx = (float)pctGraph2.Width / (float)GASO_HW;
				float graph_vy = (float)pctGraph2.Height / (float)GASO_VW;
				for(int v=0; v<GASO_VW; v++){
					for(int h=0; h<GASO_HW; h++){
						g2.FillRectangle(brsh[imgLog[sel].data[GASO_HW * v + h]], h*graph_vx, v*graph_vy, graph_vx, graph_vy);
					}
				}

				//中央線
				g2.DrawLine(Pens.Gray,  pctGraph.Width / 2, 0, pctGraph.Width / 2, pctGraph.Height);
				g2.DrawRectangle(Pens.Red, 0, vPos*graph_vy, pctGraph.Width-3, graph_vy);
				g2.DrawRectangle(Pens.Magenta, 0, vPos2*graph_vy, pctGraph.Width-3, graph_vy);

			}
		}
	}
}
