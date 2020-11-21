//==========================================================================
// グラフ描画３  （画素データを濃淡で左右の中央にスクロール表示）
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
		//==================================================================
		// Graph3の描画（画素データ）
		//==================================================================
		public void DrawGraph3()
		{
			cur3_show = false;		//カーソルを非表示に

			//ビットマップイメージを解放
			if(pctGraph3.Image != null) pctGraph3.Image.Dispose();

			// pctGraph3のサイズ設定
			graph3_vx = 4;
			graph3_vy = 6;
	
			pctGraph3.Height = imgLog_Count * (int)graph3_vy;
			pnlGraph3.Width = pctGraph3.Width + SCROLLBAR_WIDTH;

			// PictureBoxと同サイズのBitmapオブジェクトを作成
			Bitmap bmp3 = new Bitmap(pctGraph3.Size.Width, pctGraph3.Size.Height);
			pctGraph3.Image = bmp3;
			Graphics g3 = Graphics.FromImage(pctGraph3.Image);
			
			int n, i;
			
			int x0 = pctGraph3.Width / 2;									//中心線
			

			g3.DrawLine(Pens.Gray,  x0, 0, x0, pctGraph.Height);

			for(n=0; n<imgLog_Count-1; n++){
				// 画素
				for(i = 0; i<32; i++){
					g3.FillRectangle(brsh[imgLog[n].data[GASO_HW * vPos + i]], i*graph3_vx, n*graph3_vy, graph3_vx, graph3_vy);
				}

				// 中央値
				float center_x;
				if(enableCenter2){
					center_x = imgLog[n].Center * graph3_vx + graph3_vx/2;
					g3.FillRectangle(Brushes.Red, center_x, n*graph3_vy, 2, graph3_vy);
					center_x = imgLog[n].Center2 * graph3_vx + graph3_vx/2;
					g3.DrawLine(Pens.Magenta, center_x, n*graph3_vy, center_x, (n+1)*graph3_vy);
				}
				else{
					center_x = imgLog[n].Center * graph3_vx + graph3_vx/2;
					g3.DrawLine(Pens.Red, center_x, n*graph3_vy, center_x, (n+1)*graph3_vy);
				}

				// ハーフライン
				if(hlPos > 0){
					float x_hlPos;
					x_hlPos = (imgLog[n].Center - hlPos) * graph3_vx + graph3_vx/2;
					g3.DrawLine(Pens.Teal, x_hlPos, n*graph3_vy, x_hlPos, (n+1)*graph3_vy);
					x_hlPos = (imgLog[n].Center + hlPos) * graph3_vx + graph3_vx/2;
					g3.DrawLine(Pens.Teal, x_hlPos, n*graph3_vy, x_hlPos, (n+1)*graph3_vy);
				}

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

			g3.Dispose();
		}

		private void pctGraph3_MouseDown(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left){
				if(imgLog_Count > 0){
					int n = (int)(e.Y / graph3_vy);
					if(n < 0)
						n = 0;
					else if(n >= imgLog_Count)
						n = imgLog_Count - 1;

					//棒グラフ・２次元画像の表示
					DrawGraph2(n);

					//lstViewのカーソル位置変更
					if(LOG_Version >= 51){
						lstView.SelectedIndex = n;
						lstView.Focus();
					}

					//カーソルライン表示
					Point p1 = new Point(0, e.Y);
					Point p2 = new Point(pctGraph3.Width, e.Y);
					Point ps = pctGraph3.PointToScreen(p1);
					Point pe = pctGraph3.PointToScreen(p2);
					ControlPaint.DrawReversibleLine(ps, pe, Color.Black);
					cur3b_y1 = e.Y;
					cur3b_show = true;
				}
			}
			if(e.Button == MouseButtons.Right){
				scrPoint1 = pnlGraph3.AutoScrollPosition;
				scrPoint2 = new Point(e.X, e.Y);
				scrPoint2 = pctGraph3.PointToScreen(scrPoint2);

				if(cur_show){
					erase_cursol();
					cur_show = false;
				}
				if(cur3_show){
					erase_cursol3();
					cur3_show = false;
				}
			}
		}

		private void pctGraph3_MouseMove(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left){
				if(imgLog_Count > 0){
					int n = (int)(e.Y / graph3_vy);
					if(n < 0)
						n = 0;
					else if(n >= imgLog_Count)
						n = imgLog_Count - 1;
					
					//棒グラフ・２次元画像の表示
					DrawGraph2(n);

					//lstViewのカーソル位置変更
					if(LOG_Version >= 51){
						lstView.SelectedIndex = n;
						lstView.Focus();
					}

					//カーソルライン消去
					if(cur3b_show){
						cur3b_show = false;
						Point p1, p2, ps, pe;
						p1 = new Point(0, cur3b_y1);
						p2 = new Point(pctGraph3.Width, cur3b_y1);
						ps = pctGraph3.PointToScreen(p1);
						pe = pctGraph3.PointToScreen(p2);
						ControlPaint.DrawReversibleLine(ps, pe, Color.Black);
					}
				}
			}
			if(e.Button == MouseButtons.Right){
				Point pnt = new Point(e.X, e.Y);
				pnt = pctGraph3.PointToScreen(pnt);
				int x = pnt.X - scrPoint2.X;
				int y = pnt.Y - scrPoint2.Y;
				pnlGraph3.AutoScrollPosition = new Point(-scrPoint1.X + x * -1, -scrPoint1.Y + y * -1);

				int n = -pnlGraph3.AutoScrollPosition.Y / (int)graph3_vy;

				//棒グラフ・２次元画像の表示
				DrawGraph2(n);

				//lstViewのカーソル位置変更
				if(LOG_Version >= 51){
					lstView.SelectedIndex = n;
					lstView.Focus();
				}
			}
		}

		private void pctGraph3_MouseUp(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left){
				if(cur3b_show){
					cur3b_show = false;
					Point p1, p2, ps, pe;
					p1 = new Point(0, cur3b_y1);
					p2 = new Point(pctGraph3.Width, cur3b_y1);
					ps = pctGraph3.PointToScreen(p1);
					pe = pctGraph3.PointToScreen(p2);
					ControlPaint.DrawReversibleLine(ps, pe, Color.Black);
				}
			}
		}

		private void pctGraph3_Paint(object sender, PaintEventArgs e)
		{
			if(cur_show){
				erase_cursol();
				cur_show = false;
			}
			if(cur3_show){
				erase_cursol3();
				cur3_show = false;
			}
		}

		private void pnlGraph3_Scroll(object sender, ScrollEventArgs e)
		{
			if(cur3_show){
				erase_cursol3();
				cur3_show = false;
			}

			//棒グラフ・２次元画像の表示
			int n = -pnlGraph3.AutoScrollPosition.Y / (int)graph3_vy;
			DrawGraph2(n);
			
			//lstViewのカーソル位置変更
			if(LOG_Version >= 51){
				lstView.SelectedIndex = n;
				lstView.Focus();
			}

		}

	}
}
