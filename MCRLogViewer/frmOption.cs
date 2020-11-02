using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MCRLogViewer
{
	public partial class frmOption : Form
	{
		public frmOption()
		{
			InitializeComponent();
		}

		private void lblLine_Click(object sender, EventArgs e)
		{
			Label lbl = (Label)sender;
			if(colorDialog1.ShowDialog() == DialogResult.OK){
				lbl.ForeColor = colorDialog1.Color;
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.Close();
		}

        private void btnAllSelect_Click(object sender, EventArgs e)
        {
            chkA.Checked = true;
            chkB.Checked = true;
            chkC.Checked = true;
            chkD.Checked = true;
            chkE.Checked = true;
            chkF.Checked = true;
            chkG.Checked = true;
            chkH.Checked = true;
            chkI.Checked = true;
            chkJ.Checked = true;
            chkK.Checked = true;
            chkL.Checked = true;
        }

        private void btnAllClear_Click(object sender, EventArgs e)
        {
            chkA.Checked = false;
            chkB.Checked = false;
            chkC.Checked = false;
            chkD.Checked = false;
            chkE.Checked = false;
            chkF.Checked = false;
            chkG.Checked = false;
            chkH.Checked = false;
            chkI.Checked = false;
            chkJ.Checked = false;
            chkK.Checked = false;
            chkL.Checked = false;
        }
	}
}
