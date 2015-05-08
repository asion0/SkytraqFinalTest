using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SkytraqFinalTestServer
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
        private bool CheckWorkNo()
        {
            String s = workNo.Text;
            //A511-10201020001
            if (s.Length != 16 || s[4] != '-')
            {
                return false;
            }
            return true;
        }

        private void finishBtn_Click(object sender, EventArgs e)
        {
            if (CheckWorkNo())
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("請輸入正確的工單號碼！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
