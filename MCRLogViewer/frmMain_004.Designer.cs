namespace MCRLogViewer
{
    partial class frmMain
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.lstView = new System.Windows.Forms.ListBox();
			this.txtHead = new System.Windows.Forms.TextBox();
			this.lblHead1 = new System.Windows.Forms.Label();
			this.menu = new System.Windows.Forms.MenuStrip();
			this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.menuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.menuFileSave = new System.Windows.Forms.ToolStripMenuItem();
			this.menuFileSaveTXT = new System.Windows.Forms.ToolStripMenuItem();
			this.menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
			this.txtPath = new System.Windows.Forms.TextBox();
			this.pnlGraph = new System.Windows.Forms.Panel();
			this.pctGraph = new System.Windows.Forms.PictureBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnX4 = new System.Windows.Forms.Button();
			this.btnX1 = new System.Windows.Forms.Button();
			this.btnX2 = new System.Windows.Forms.Button();
			this.btnGraphOption = new System.Windows.Forms.Button();
			this.btnW4 = new System.Windows.Forms.Button();
			this.btnW2 = new System.Windows.Forms.Button();
			this.btnOpen = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.lblHead2 = new System.Windows.Forms.Label();
			this.btnToubai = new System.Windows.Forms.Button();
			this.btnW8 = new System.Windows.Forms.Button();
			this.btnX8 = new System.Windows.Forms.Button();
			this.menu.SuspendLayout();
			this.pnlGraph.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pctGraph)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// lstView
			// 
			this.lstView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lstView.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.lstView.FormattingEnabled = true;
			this.lstView.HorizontalScrollbar = true;
			this.lstView.ItemHeight = 12;
			this.lstView.Location = new System.Drawing.Point(3, 32);
			this.lstView.Name = "lstView";
			this.lstView.Size = new System.Drawing.Size(551, 328);
			this.lstView.TabIndex = 5;
			this.lstView.SelectedIndexChanged += new System.EventHandler(this.lstView_SelectedIndexChanged);
			// 
			// txtHead
			// 
			this.txtHead.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.txtHead.Location = new System.Drawing.Point(3, 28);
			this.txtHead.Multiline = true;
			this.txtHead.Name = "txtHead";
			this.txtHead.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtHead.Size = new System.Drawing.Size(551, 111);
			this.txtHead.TabIndex = 6;
			// 
			// lblHead1
			// 
			this.lblHead1.AutoSize = true;
			this.lblHead1.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.lblHead1.Location = new System.Drawing.Point(0, 18);
			this.lblHead1.Name = "lblHead1";
			this.lblHead1.Size = new System.Drawing.Size(557, 12);
			this.lblHead1.TabIndex = 8;
			this.lblHead1.Text = "  time mode  sens    pos  hnd ang  sv    vt  v   fl  fr  rl  rr      x  slc   Bat" +
    "t  Gyro    ";
			// 
			// menu
			// 
			this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile});
			this.menu.Location = new System.Drawing.Point(0, 0);
			this.menu.Name = "menu";
			this.menu.Size = new System.Drawing.Size(1147, 26);
			this.menu.TabIndex = 10;
			this.menu.Text = "menuStrip1";
			// 
			// menuFile
			// 
			this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileOpen,
            this.menuFileSave,
            this.menuFileSaveTXT,
            this.menuFileExit});
			this.menuFile.Name = "menuFile";
			this.menuFile.Size = new System.Drawing.Size(89, 22);
			this.menuFile.Text = "ファイル (&F)";
			// 
			// menuFileOpen
			// 
			this.menuFileOpen.Name = "menuFileOpen";
			this.menuFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Space)));
			this.menuFileOpen.Size = new System.Drawing.Size(197, 22);
			this.menuFileOpen.Text = "開く (&O)";
			this.menuFileOpen.Click += new System.EventHandler(this.FileOpen_Click);
			// 
			// menuFileSave
			// 
			this.menuFileSave.Enabled = false;
			this.menuFileSave.Name = "menuFileSave";
			this.menuFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.menuFileSave.Size = new System.Drawing.Size(197, 22);
			this.menuFileSave.Text = "圧縮保存 (&S)";
			this.menuFileSave.Click += new System.EventHandler(this.menuFileSave_Click);
			// 
			// menuFileSaveTXT
			// 
			this.menuFileSaveTXT.Enabled = false;
			this.menuFileSaveTXT.Name = "menuFileSaveTXT";
			this.menuFileSaveTXT.Size = new System.Drawing.Size(197, 22);
			this.menuFileSaveTXT.Text = "TXT形式で保存 (&S)";
			this.menuFileSaveTXT.Click += new System.EventHandler(this.menuFileSaveTXT_Click);
			// 
			// menuFileExit
			// 
			this.menuFileExit.Name = "menuFileExit";
			this.menuFileExit.Size = new System.Drawing.Size(197, 22);
			this.menuFileExit.Text = "終了 (&X)";
			this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
			// 
			// txtPath
			// 
			this.txtPath.Location = new System.Drawing.Point(3, 3);
			this.txtPath.Name = "txtPath";
			this.txtPath.Size = new System.Drawing.Size(551, 19);
			this.txtPath.TabIndex = 11;
			this.txtPath.Text = "c:\\mcr\\log\\000.LOG";
			// 
			// pnlGraph
			// 
			this.pnlGraph.AutoScroll = true;
			this.pnlGraph.Controls.Add(this.pctGraph);
			this.pnlGraph.Cursor = System.Windows.Forms.Cursors.Default;
			this.pnlGraph.Location = new System.Drawing.Point(3, 60);
			this.pnlGraph.Name = "pnlGraph";
			this.pnlGraph.Size = new System.Drawing.Size(455, 326);
			this.pnlGraph.TabIndex = 12;
			this.pnlGraph.Scroll += new System.Windows.Forms.ScrollEventHandler(this.pnlGraph_Scroll);
			// 
			// pctGraph
			// 
			this.pctGraph.BackColor = System.Drawing.Color.Navy;
			this.pctGraph.Location = new System.Drawing.Point(0, 0);
			this.pctGraph.Name = "pctGraph";
			this.pctGraph.Size = new System.Drawing.Size(252, 211);
			this.pctGraph.TabIndex = 0;
			this.pctGraph.TabStop = false;
			this.pctGraph.Paint += new System.Windows.Forms.PaintEventHandler(this.pctGraph_Paint);
			this.pctGraph.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pctGraph_MouseDown);
			this.pctGraph.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pctGraph_MouseMove);
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(68, 3);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(69, 52);
			this.btnOK.TabIndex = 13;
			this.btnOK.Text = "グラフ更新";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnX4
			// 
			this.btnX4.Enabled = false;
			this.btnX4.Location = new System.Drawing.Point(237, 3);
			this.btnX4.Name = "btnX4";
			this.btnX4.Size = new System.Drawing.Size(41, 23);
			this.btnX4.TabIndex = 16;
			this.btnX4.Text = "等x4";
			this.btnX4.UseVisualStyleBackColor = true;
			this.btnX4.Click += new System.EventHandler(this.btnX4_Click);
			// 
			// btnX1
			// 
			this.btnX1.Location = new System.Drawing.Point(143, 31);
			this.btnX1.Name = "btnX1";
			this.btnX1.Size = new System.Drawing.Size(41, 24);
			this.btnX1.TabIndex = 15;
			this.btnX1.Text = "x1";
			this.btnX1.UseVisualStyleBackColor = true;
			this.btnX1.Click += new System.EventHandler(this.btnX1_Click);
			// 
			// btnX2
			// 
			this.btnX2.Enabled = false;
			this.btnX2.Location = new System.Drawing.Point(190, 3);
			this.btnX2.Name = "btnX2";
			this.btnX2.Size = new System.Drawing.Size(41, 23);
			this.btnX2.TabIndex = 14;
			this.btnX2.Text = "等x2";
			this.btnX2.UseVisualStyleBackColor = true;
			this.btnX2.Click += new System.EventHandler(this.btnX2_Click);
			// 
			// btnGraphOption
			// 
			this.btnGraphOption.Location = new System.Drawing.Point(375, 3);
			this.btnGraphOption.Name = "btnGraphOption";
			this.btnGraphOption.Size = new System.Drawing.Size(83, 51);
			this.btnGraphOption.TabIndex = 18;
			this.btnGraphOption.Text = "オプション";
			this.btnGraphOption.UseVisualStyleBackColor = true;
			this.btnGraphOption.Click += new System.EventHandler(this.btnGraphOption_Click);
			// 
			// btnW4
			// 
			this.btnW4.Location = new System.Drawing.Point(237, 31);
			this.btnW4.Name = "btnW4";
			this.btnW4.Size = new System.Drawing.Size(41, 23);
			this.btnW4.TabIndex = 20;
			this.btnW4.Text = "横x4";
			this.btnW4.UseVisualStyleBackColor = true;
			this.btnW4.Click += new System.EventHandler(this.btnW4_Click);
			// 
			// btnW2
			// 
			this.btnW2.Location = new System.Drawing.Point(190, 31);
			this.btnW2.Name = "btnW2";
			this.btnW2.Size = new System.Drawing.Size(41, 23);
			this.btnW2.TabIndex = 19;
			this.btnW2.Text = "横x2";
			this.btnW2.UseVisualStyleBackColor = true;
			this.btnW2.Click += new System.EventHandler(this.btnW2_Click);
			// 
			// btnOpen
			// 
			this.btnOpen.Location = new System.Drawing.Point(3, 3);
			this.btnOpen.Name = "btnOpen";
			this.btnOpen.Size = new System.Drawing.Size(59, 51);
			this.btnOpen.TabIndex = 21;
			this.btnOpen.Text = "開く";
			this.btnOpen.UseVisualStyleBackColor = true;
			this.btnOpen.Click += new System.EventHandler(this.FileOpen_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 26);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.AutoScroll = true;
			this.splitContainer1.Panel2.Controls.Add(this.btnToubai);
			this.splitContainer1.Panel2.Controls.Add(this.btnOpen);
			this.splitContainer1.Panel2.Controls.Add(this.btnOK);
			this.splitContainer1.Panel2.Controls.Add(this.pnlGraph);
			this.splitContainer1.Panel2.Controls.Add(this.btnW8);
			this.splitContainer1.Panel2.Controls.Add(this.btnW4);
			this.splitContainer1.Panel2.Controls.Add(this.btnGraphOption);
			this.splitContainer1.Panel2.Controls.Add(this.btnX1);
			this.splitContainer1.Panel2.Controls.Add(this.btnW2);
			this.splitContainer1.Panel2.Controls.Add(this.btnX2);
			this.splitContainer1.Panel2.Controls.Add(this.btnX8);
			this.splitContainer1.Panel2.Controls.Add(this.btnX4);
			this.splitContainer1.Panel2.Resize += new System.EventHandler(this.splitContainer1_Panel2_Resize);
			this.splitContainer1.Size = new System.Drawing.Size(1147, 716);
			this.splitContainer1.SplitterDistance = 572;
			this.splitContainer1.TabIndex = 22;
			// 
			// splitContainer2
			// 
			this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.txtPath);
			this.splitContainer2.Panel1.Controls.Add(this.txtHead);
			this.splitContainer2.Panel1.Resize += new System.EventHandler(this.splitContainer2_Panel1_Resize);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.lblHead2);
			this.splitContainer2.Panel2.Controls.Add(this.lstView);
			this.splitContainer2.Panel2.Controls.Add(this.lblHead1);
			this.splitContainer2.Size = new System.Drawing.Size(572, 716);
			this.splitContainer2.SplitterDistance = 146;
			this.splitContainer2.TabIndex = 12;
			this.splitContainer2.SizeChanged += new System.EventHandler(this.splitContainer2_SizeChanged);
			// 
			// lblHead2
			// 
			this.lblHead2.AutoSize = true;
			this.lblHead2.BackColor = System.Drawing.Color.SlateBlue;
			this.lblHead2.Font = new System.Drawing.Font("ＭＳ ゴシック", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.lblHead2.ForeColor = System.Drawing.Color.White;
			this.lblHead2.Location = new System.Drawing.Point(0, 0);
			this.lblHead2.Name = "lblHead2";
			this.lblHead2.Size = new System.Drawing.Size(557, 12);
			this.lblHead2.TabIndex = 12;
			this.lblHead2.Text = "                           A   B    C     D   E   F   G   H   I      J         K " +
    "    L      ";
			// 
			// btnToubai
			// 
			this.btnToubai.Enabled = false;
			this.btnToubai.Location = new System.Drawing.Point(143, 3);
			this.btnToubai.Name = "btnToubai";
			this.btnToubai.Size = new System.Drawing.Size(41, 23);
			this.btnToubai.TabIndex = 22;
			this.btnToubai.Text = "等倍";
			this.btnToubai.UseVisualStyleBackColor = true;
			this.btnToubai.Click += new System.EventHandler(this.btnToubai_Click);
			// 
			// btnW8
			// 
			this.btnW8.Location = new System.Drawing.Point(284, 31);
			this.btnW8.Name = "btnW8";
			this.btnW8.Size = new System.Drawing.Size(41, 23);
			this.btnW8.TabIndex = 20;
			this.btnW8.Text = "横x8";
			this.btnW8.UseVisualStyleBackColor = true;
			this.btnW8.Click += new System.EventHandler(this.btnW8_Click);
			// 
			// btnX8
			// 
			this.btnX8.Enabled = false;
			this.btnX8.Location = new System.Drawing.Point(284, 3);
			this.btnX8.Name = "btnX8";
			this.btnX8.Size = new System.Drawing.Size(41, 23);
			this.btnX8.TabIndex = 16;
			this.btnX8.Text = "等x8";
			this.btnX8.UseVisualStyleBackColor = true;
			this.btnX8.Click += new System.EventHandler(this.btnX8_Click);
			// 
			// frmMain
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1147, 742);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.menu);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menu;
			this.Name = "frmMain";
			this.Text = "MCR LOG Viewer  ver.3.5";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmMain_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmMain_DragEnter);
			this.Resize += new System.EventHandler(this.frmMain_Resize);
			this.menu.ResumeLayout(false);
			this.menu.PerformLayout();
			this.pnlGraph.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pctGraph)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel1.PerformLayout();
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.Panel2.PerformLayout();
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.Label lblHead1;

        public System.Windows.Forms.TextBox txtHead;
		public System.Windows.Forms.ListBox lstView;
		private System.Windows.Forms.MenuStrip menu;
		private System.Windows.Forms.ToolStripMenuItem menuFile;
		private System.Windows.Forms.ToolStripMenuItem menuFileOpen;
		private System.Windows.Forms.ToolStripMenuItem menuFileSaveTXT;
		private System.Windows.Forms.ToolStripMenuItem menuFileExit;
		private System.Windows.Forms.TextBox txtPath;
		private System.Windows.Forms.Panel pnlGraph;
		public System.Windows.Forms.PictureBox pctGraph;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnX4;
		private System.Windows.Forms.Button btnX1;
		private System.Windows.Forms.Button btnX2;
		private System.Windows.Forms.Button btnGraphOption;
		private System.Windows.Forms.Button btnW4;
		private System.Windows.Forms.Button btnW2;
		private System.Windows.Forms.Button btnOpen;
		private System.Windows.Forms.ToolStripMenuItem menuFileSave;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.Label lblHead2;
		private System.Windows.Forms.Button btnW8;
		private System.Windows.Forms.Button btnX8;
		private System.Windows.Forms.Button btnToubai;
    }
}

