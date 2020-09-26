//==========================================================================
// グラフ描画１  （メイングラフ）
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
		// Graphの描画
		//==================================================================
		public void DrawGraph()
		{
			cur_show = false;		//カーソルを非表示に

			//ビットマップイメージを解放
			if(pctGraph.Image != null) pctGraph.Image.Dispose();

			// PictureBoxと同サイズのBitmapオブジェクトを作成
			Bitmap bmp = new Bitmap(pctGraph.Size.Width, pctGraph.Size.Height);
			pctGraph.Image = bmp;
			Graphics g = Graphics.FromImage(pctGraph.Image);
			
			int n, i;
			Single x, x1;
			Pen pen_err_background = Pens.MidnightBlue;
			Pen pen_backline = Pens.DarkSlateGray;

			y0 = pctGraph.Height / 2;											//水平線

			x1 = x = 0;
			graph_v = (Single)pctGraph.Width / (Single)frmMain.log_count;		//xの増分

			for(i=y0; i<pctGraph.Height; i+=40){
				g.DrawLine(pen_backline, 0, i, pctGraph.Width, i);
			}
			for(i=y0; i>0; i-=40){
				g.DrawLine(pen_backline, 0, i, pctGraph.Width, i);
			}

			g.DrawLine(Pens.Gray,  0, y0, pctGraph.Width, y0);


			for(n=0; n<graph_points; n++){
				gp[n].y = gp[n].y1 = 0;
			}

			gp[ 0].pen = new Pen(frmOption1.lblA.ForeColor, (float)frmOption1.widthA.Value);
			gp[ 1].pen = new Pen(frmOption1.lblB.ForeColor, (float)frmOption1.widthB.Value);
			gp[ 2].pen = new Pen(frmOption1.lblC.ForeColor, (float)frmOption1.widthC.Value);
			gp[ 3].pen = new Pen(frmOption1.lblD.ForeColor, (float)frmOption1.widthD.Value);
			gp[ 4].pen = new Pen(frmOption1.lblE.ForeColor, (float)frmOption1.widthE.Value);
			gp[ 5].pen = new Pen(frmOption1.lblF.ForeColor, (float)frmOption1.widthF.Value);
			gp[ 6].pen = new Pen(frmOption1.lblG.ForeColor, (float)frmOption1.widthG.Value);
			gp[ 7].pen = new Pen(frmOption1.lblH.ForeColor, (float)frmOption1.widthH.Value);
			gp[ 8].pen = new Pen(frmOption1.lblI.ForeColor, (float)frmOption1.widthI.Value);
			gp[ 9].pen = new Pen(frmOption1.lblJ.ForeColor, (float)frmOption1.widthJ.Value);
			gp[10].pen = new Pen(frmOption1.lblK.ForeColor, (float)frmOption1.widthK.Value);
			gp[11].pen = new Pen(frmOption1.lblL.ForeColor, (float)frmOption1.widthL.Value);

			gp[ 0].enabled = frmOption1.chkA.Checked;
			gp[ 1].enabled = frmOption1.chkB.Checked;
			gp[ 2].enabled = frmOption1.chkC.Checked;
			gp[ 3].enabled = frmOption1.chkD.Checked;
			gp[ 4].enabled = frmOption1.chkE.Checked;
			gp[ 5].enabled = frmOption1.chkF.Checked;
			gp[ 6].enabled = frmOption1.chkG.Checked;
			gp[ 7].enabled = frmOption1.chkH.Checked;
			gp[ 8].enabled = frmOption1.chkI.Checked;
			gp[ 9].enabled = frmOption1.chkJ.Checked;
			gp[10].enabled = frmOption1.chkK.Checked;
			gp[11].enabled = frmOption1.chkL.Checked;

			gp[ 0].scale = (Single)frmOption1.nudA.Value;
			gp[ 1].scale = (Single)frmOption1.nudB.Value;
			gp[ 2].scale = (Single)frmOption1.nudC.Value;
			gp[ 3].scale = (Single)frmOption1.nudD.Value;
			gp[ 4].scale = (Single)frmOption1.nudE.Value;
			gp[ 5].scale = (Single)frmOption1.nudF.Value;
			gp[ 6].scale = (Single)frmOption1.nudG.Value;
			gp[ 7].scale = (Single)frmOption1.nudH.Value;
			gp[ 8].scale = (Single)frmOption1.nudI.Value;
			gp[ 9].scale = (Single)frmOption1.nudJ.Value;
			gp[10].scale = (Single)frmOption1.nudK.Value;
			gp[11].scale = (Single)frmOption1.nudL.Value;

			for(n=0; n<log_count; n++){
				gp[ 0].y = -log[n].angle_t;
				gp[ 1].y = -log[n].angle;
				gp[ 3].y = -log[n].vt;
				gp[ 4].y = -log[n].v;

				if(LOG_Version <= 3){
					gp[ 2].y = -log[n].power;
					gp[ 5].y = -log[n].batt;
					gp[ 6].y = -log[n].gyro;
					gp[ 7].y = 0;
					gp[ 8].y = 0;
					gp[ 9].y = 0;
					gp[10].y = 0;
					gp[11].y = 0;
				}
				else if(LOG_Version >= 4){
					gp[ 2].y = -log[n].sv_pow;
					gp[ 5].y = -log[n].fl;
					gp[ 6].y = -log[n].fr;
					gp[ 7].y = -log[n].rl;
					gp[ 8].y = -log[n].rr;
					gp[ 9].y = -log[n].trip;
					gp[11].y = -log[n].gyro;

					if(LOG_Version >= 11 || LOG_Version == 9)
						gp[10].y = -log[n].center;
					else if(LOG_Version >= 7 && LOG_Version <= 8)
						gp[10].y = -log[n].gyroEx;
					else
						gp[10].y = -log[n].batt;
				}

				//スケール
				for(i=0; i<12; i++){
					gp[i].y = gp[i].y * gp[i].scale * (Single)y0 / 1000;
				}
	
				x += graph_v;

				
				if(log[n].mode == 0){		//ログ記録エラーの部分は背景を替えてグラフ描画はしない。
					int ix;

					for(ix = (int)x1; ix < x; ix++){
						g.DrawLine(pen_err_background, ix, 0, ix, (int)pctGraph.Height);
					}
				}
				else{						//通常描画
					if(n % 20 == 0){		//縦線
						g.DrawLine(pen_backline, x1, 0, x1, pctGraph.Height);
					}
				
					for(i=11; i>=0; i--){
						if(gp[i].enabled == true){
							if(n>0) if(log[n-1].mode == 0) gp[i].y1 = gp[i].y;

							g.DrawLine(gp[i].pen, x, gp[i].y + y0, x1, gp[i].y1 + y0);
							gp[i].y1 = gp[i].y;
						}
					}
				}
				x1 = x;
			}

			//スクロール
			cur_x = (int)((Single)(lstView.SelectedIndex + 1) * graph_v);
			pnlGraph.AutoScrollPosition = new Point(cur_x - pnlGraph.Width / 2, 0);


			pctGraph.Refresh();		// PictureBoxを更新（再描画させる）
			
		//	draw_cursol();
			
			for(i=0; i<graph_points; i++){
				gp[i].pen.Dispose();
			}
			g.Dispose();
		}

		//==================================================================
		//現在位置のカーソルを消去
		//==================================================================
		private void erase_cursol()
		{
			Point p1, p2, ps, pe;
			Point pgx = pnlGraph.PointToScreen(new Point(0, 0));

			//--------------------------------------------------------------
			//graph
			p1 = new Point((int)cur_x1, 0);
			p2 = new Point((int)cur_x1, pctGraph.Height);
			ps = pctGraph.PointToScreen(p1);
			pe = pctGraph.PointToScreen(p2);
			if(ps.X > pgx.X && ps.X < pgx.X + pnlGraph.Width){
				ControlPaint.DrawReversibleLine(ps, pe, Color.Black);
			}

		}
	
		private void erase_cursol3()
		{
			Point p1, p2, ps, pe;
			Point pgx3 = pnlGraph3.PointToScreen(new Point(0, 0));
			//--------------------------------------------------------------
			//graph3
			if(LOG_Version == 10){
				p1 = new Point(0, (int)cur3_y1);
				p2 = new Point(pctGraph3.Width, (int)cur3_y1);
				ps = pctGraph3.PointToScreen(p1);
				pe = pctGraph3.PointToScreen(p2);
				if(ps.Y > pgx3.Y && ps.Y < pgx3.Y + pnlGraph3.Height){
					ControlPaint.DrawReversibleLine(ps, pe, Color.Black);
				}
			}	
		}

		//==================================================================
		//新しい位置にカーソルを表示
		//==================================================================
		private void draw_cursol()
		{
			Point p1, p2, ps, pe;
			int n;
			Point pgx = pnlGraph.PointToScreen(new Point(0, 0));
			Point pgx3 = pnlGraph3.PointToScreen(new Point(0, 0));

			//--------------------------------------------------------------
			//graph
			if(cur_show){		//カーソルが表示されていたら現在のカーソルを消去
				erase_cursol();
			}
			else{
				cur_show = true;
			}
		
			//新しい場所の位置を計算
			n = lstView.SelectedIndex + 1;
			cur_x = (int)((Single)n * graph_v);

			//新しい場所にカーソル表示
			p1 = new Point((int)cur_x, 0);
			p2 = new Point((int)cur_x, pctGraph.Height);
			ps = pctGraph.PointToScreen(p1);
			pe = pctGraph.PointToScreen(p2);
			if(ps.X > pgx.X && ps.X < pgx.X + pnlGraph.Width){
				ControlPaint.DrawReversibleLine(ps, pe, Color.Black);
			}

			//新しい場所をcur_x1に記録
			cur_n1 = n;
			cur_x1 = cur_x;

			//--------------------------------------------------------------
			//graph3
			if(LOG_Version == 10){
				if(cur3_show){		//カーソルが表示されていたら現在のカーソルを消去
					erase_cursol3();
				}
				else{
					cur3_show = true;
				}
				//新しい場所の位置を計算
				cur3_y = (int)((Single)(n-1) * graph3_vy);

				//新しい場所にカーソル表示
				p1 = new Point(0, (int)cur3_y);
				p2 = new Point(pctGraph3.Width, (int)cur3_y);
				ps = pctGraph3.PointToScreen(p1);
				pe = pctGraph3.PointToScreen(p2);
				if(ps.Y > pgx3.Y && ps.Y < pgx3.Y + pnlGraph3.Height){
					ControlPaint.DrawReversibleLine(ps, pe, Color.Black);
				}
				//新しい場所をcur3_y1に記録
				cur3_n1 = n;
				cur3_y1 = cur3_y;

			}
		}

		//==================================================================
		//画面上のグラフカーソル描画
		//==================================================================
		private void pctGraph_Paint(object sender, PaintEventArgs e)
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

		//==================================================================
        //グラフのクリックでlstViewのインデックス変更
		//==================================================================
		private void pctGraph_MouseMove(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left){
				if(lstView.Items.Count > 0){
					int x = (int)(e.X / graph_v);
					if(x < 0)
						x = 0;
					else if(x >= lstView.Items.Count)
						x = lstView.Items.Count - 1;
					lstView.SelectedIndex = x;
					lstView.Focus();
					draw_cursol();
				}
			}
			else if(e.Button == MouseButtons.Right){
				Point pnt2 = new Point(e.X, e.Y);
				pnt2 = pctGraph.PointToScreen(pnt2);
				int x = pnt2.X - scrPoint2.X;
				int y = pnt2.Y - scrPoint2.Y;
				pnlGraph.AutoScrollPosition = new Point(-scrPoint1.X + x * -1, -scrPoint1.Y + y * -1);
			}
		}

		private void pctGraph_MouseDown(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left){
				if(lstView.Items.Count > 0){
					int x = (int)(e.X / graph_v);
					if(x < 0)
						x = 0;
					else if(x >= lstView.Items.Count)
						x = lstView.Items.Count - 1;
					lstView.SelectedIndex = x;
					lstView.Focus();
				}
			}
			else if(e.Button == MouseButtons.Right){
				scrPoint1 = pnlGraph.AutoScrollPosition;
				scrPoint2 = new Point(e.X, e.Y);
				scrPoint2 = pctGraph.PointToScreen(scrPoint2);

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

		private void pnlGraph_Scroll(object sender, ScrollEventArgs e)
		{
			if(cur_show){
				erase_cursol();
				cur_show = false;
			}
		}

	}
}
