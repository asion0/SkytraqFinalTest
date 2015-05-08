namespace SkytraqFinalTestServer
{
    partial class LoginForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.workNo = new System.Windows.Forms.TextBox();
            this.finishBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(16, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(178, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "輸入工單號碼：";
            // 
            // workNo
            // 
            this.workNo.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.workNo.Location = new System.Drawing.Point(16, 39);
            this.workNo.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.workNo.MaxLength = 20;
            this.workNo.Name = "workNo";
            this.workNo.Size = new System.Drawing.Size(814, 36);
            this.workNo.TabIndex = 1;
            // 
            // finishBtn
            // 
            this.finishBtn.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.finishBtn.Location = new System.Drawing.Point(670, 87);
            this.finishBtn.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.finishBtn.Name = "finishBtn";
            this.finishBtn.Size = new System.Drawing.Size(160, 45);
            this.finishBtn.TabIndex = 2;
            this.finishBtn.Text = "完成";
            this.finishBtn.UseVisualStyleBackColor = true;
            this.finishBtn.Click += new System.EventHandler(this.finishBtn_Click);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(846, 145);
            this.Controls.Add(this.finishBtn);
            this.Controls.Add(this.workNo);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "工單號碼";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button finishBtn;
        public System.Windows.Forms.TextBox workNo;
    }
}