namespace SkytraqFinalTestServer
{
    partial class ServerForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改這個方法的內容。
        ///
        /// </summary>
        private void InitializeComponent()
        {
            this.consoleMsg = new System.Windows.Forms.ListBox();
            this.start = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ip = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.port = new System.Windows.Forms.TextBox();
            this.notify00 = new System.Windows.Forms.TextBox();
            this.noTest = new System.Windows.Forms.CheckBox();
            this.crcLabel = new System.Windows.Forms.Label();
            this.crcValue = new System.Windows.Forms.Label();
            this.modeLabel = new System.Windows.Forms.Label();
            this.workingMode = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // consoleMsg
            // 
            this.consoleMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.consoleMsg.FormattingEnabled = true;
            this.consoleMsg.ItemHeight = 12;
            this.consoleMsg.Location = new System.Drawing.Point(12, 138);
            this.consoleMsg.Name = "consoleMsg";
            this.consoleMsg.Size = new System.Drawing.Size(672, 352);
            this.consoleMsg.TabIndex = 1;
            // 
            // start
            // 
            this.start.Location = new System.Drawing.Point(577, 16);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(107, 96);
            this.start.TabIndex = 2;
            this.start.Text = "Start";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 29);
            this.label1.TabIndex = 3;
            this.label1.Text = "Server IP :";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // ip
            // 
            this.ip.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ip.Location = new System.Drawing.Point(160, 9);
            this.ip.Name = "ip";
            this.ip.Size = new System.Drawing.Size(193, 29);
            this.ip.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(359, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 29);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port :";
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(445, 14);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(92, 22);
            this.port.TabIndex = 4;
            // 
            // notify00
            // 
            this.notify00.Location = new System.Drawing.Point(584, 490);
            this.notify00.Name = "notify00";
            this.notify00.Size = new System.Drawing.Size(100, 22);
            this.notify00.TabIndex = 7;
            this.notify00.Visible = false;
            this.notify00.TextChanged += new System.EventHandler(this.notify00_TextChanged);
            // 
            // noTest
            // 
            this.noTest.AutoSize = true;
            this.noTest.Location = new System.Drawing.Point(12, 496);
            this.noTest.Name = "noTest";
            this.noTest.Size = new System.Drawing.Size(235, 16);
            this.noTest.TabIndex = 9;
            this.noTest.Text = "Don\'t test.(It\'ll return all NG after 75 seconds)";
            this.noTest.UseVisualStyleBackColor = true;
            this.noTest.Visible = false;
            this.noTest.CheckedChanged += new System.EventHandler(this.noTest_CheckedChanged);
            // 
            // crcLabel
            // 
            this.crcLabel.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.crcLabel.ForeColor = System.Drawing.Color.Black;
            this.crcLabel.Location = new System.Drawing.Point(12, 44);
            this.crcLabel.Name = "crcLabel";
            this.crcLabel.Size = new System.Drawing.Size(87, 33);
            this.crcLabel.TabIndex = 10;
            this.crcLabel.Text = "CRC :";
            // 
            // crcValue
            // 
            this.crcValue.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.crcValue.ForeColor = System.Drawing.Color.Black;
            this.crcValue.Location = new System.Drawing.Point(92, 45);
            this.crcValue.Name = "crcValue";
            this.crcValue.Size = new System.Drawing.Size(178, 33);
            this.crcValue.TabIndex = 10;
            // 
            // modeLabel
            // 
            this.modeLabel.Font = new System.Drawing.Font("Verdana", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.modeLabel.ForeColor = System.Drawing.Color.Black;
            this.modeLabel.Location = new System.Drawing.Point(12, 79);
            this.modeLabel.Name = "modeLabel";
            this.modeLabel.Size = new System.Drawing.Size(98, 33);
            this.modeLabel.TabIndex = 10;
            this.modeLabel.Text = "Mode :";
            // 
            // workingMode
            // 
            this.workingMode.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.workingMode.ForeColor = System.Drawing.Color.Black;
            this.workingMode.Location = new System.Drawing.Point(110, 82);
            this.workingMode.Name = "workingMode";
            this.workingMode.Size = new System.Drawing.Size(455, 33);
            this.workingMode.TabIndex = 10;
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(696, 506);
            this.Controls.Add(this.workingMode);
            this.Controls.Add(this.crcValue);
            this.Controls.Add(this.modeLabel);
            this.Controls.Add(this.crcLabel);
            this.Controls.Add(this.noTest);
            this.Controls.Add(this.notify00);
            this.Controls.Add(this.port);
            this.Controls.Add(this.ip);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.start);
            this.Controls.Add(this.consoleMsg);
            this.Name = "ServerForm";
            this.Text = "Skytraq Final Test Server";
            this.Load += new System.EventHandler(this.ServerForm_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ServerForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox consoleMsg;
        private System.Windows.Forms.Button start;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ip;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.TextBox notify00;
        private System.Windows.Forms.CheckBox noTest;
        private System.Windows.Forms.Label crcLabel;
        private System.Windows.Forms.Label crcValue;
        private System.Windows.Forms.Label modeLabel;
        private System.Windows.Forms.Label workingMode;

    }
}

