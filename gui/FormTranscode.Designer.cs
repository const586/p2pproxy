namespace gui
{
    partial class FormTranscode
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.textHeight = new System.Windows.Forms.TextBox();
            this.textWidth = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textFps = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.textVideoBitrate = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboVideoCodec = new System.Windows.Forms.ComboBox();
            this.comboMux = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textChannels = new System.Windows.Forms.NumericUpDown();
            this.comboRate = new System.Windows.Forms.ComboBox();
            this.textAudioBitrate = new System.Windows.Forms.NumericUpDown();
            this.comboAudioCodec = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.textName = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textFps)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textVideoBitrate)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textChannels)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textAudioBitrate)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(4, 30);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(196, 219);
            this.tabControl1.TabIndex = 12;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.textHeight);
            this.tabPage1.Controls.Add(this.textWidth);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.textFps);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.textVideoBitrate);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.comboVideoCodec);
            this.tabPage1.Controls.Add(this.comboMux);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(188, 193);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Видео";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // textHeight
            // 
            this.textHeight.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textHeight.Location = new System.Drawing.Point(146, 159);
            this.textHeight.Name = "textHeight";
            this.textHeight.Size = new System.Drawing.Size(30, 13);
            this.textHeight.TabIndex = 23;
            this.textHeight.Text = "0";
            // 
            // textWidth
            // 
            this.textWidth.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textWidth.Location = new System.Drawing.Point(99, 159);
            this.textWidth.Name = "textWidth";
            this.textWidth.Size = new System.Drawing.Size(30, 13);
            this.textWidth.TabIndex = 21;
            this.textWidth.Text = "0";
            this.textWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.SystemColors.Window;
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label6.Location = new System.Drawing.Point(96, 155);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 20);
            this.label6.TabIndex = 22;
            this.label6.Text = "X";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 162);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "Разрешение";
            // 
            // textFps
            // 
            this.textFps.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textFps.Location = new System.Drawing.Point(99, 126);
            this.textFps.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            131072});
            this.textFps.Name = "textFps";
            this.textFps.Size = new System.Drawing.Size(77, 20);
            this.textFps.TabIndex = 19;
            this.textFps.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 133);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(88, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Частота кадров";
            // 
            // textVideoBitrate
            // 
            this.textVideoBitrate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textVideoBitrate.Location = new System.Drawing.Point(99, 104);
            this.textVideoBitrate.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.textVideoBitrate.Name = "textVideoBitrate";
            this.textVideoBitrate.Size = new System.Drawing.Size(77, 20);
            this.textVideoBitrate.TabIndex = 17;
            this.textVideoBitrate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 111);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Битрейт";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Видео Кодек";
            // 
            // comboVideoCodec
            // 
            this.comboVideoCodec.FormattingEnabled = true;
            this.comboVideoCodec.Items.AddRange(new object[] {
            "",
            "mp1v",
            "mp2v",
            "mp4v",
            "DIV1",
            "DIV2",
            "DIV3",
            "H263",
            "H264",
            "VP80",
            "WMV1",
            "WMV2",
            "MJPG",
            "theo",
            "drac"});
            this.comboVideoCodec.Location = new System.Drawing.Point(12, 65);
            this.comboVideoCodec.Name = "comboVideoCodec";
            this.comboVideoCodec.Size = new System.Drawing.Size(168, 21);
            this.comboVideoCodec.TabIndex = 14;
            // 
            // comboMux
            // 
            this.comboMux.FormattingEnabled = true;
            this.comboMux.Items.AddRange(new object[] {
            "",
            "ts",
            "webm",
            "ogg",
            "ffmpeg{mux=flv}",
            "ps",
            "mpjpeg",
            "flv",
            "mpeg1",
            "mkv",
            "raw",
            "avi",
            "asf"});
            this.comboMux.Location = new System.Drawing.Point(12, 24);
            this.comboMux.Name = "comboMux";
            this.comboMux.Size = new System.Drawing.Size(168, 21);
            this.comboMux.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Инкапсуляция";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textChannels);
            this.tabPage2.Controls.Add(this.comboRate);
            this.tabPage2.Controls.Add(this.textAudioBitrate);
            this.tabPage2.Controls.Add(this.comboAudioCodec);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.label9);
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(188, 193);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Аудио";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textChannels
            // 
            this.textChannels.Location = new System.Drawing.Point(63, 58);
            this.textChannels.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.textChannels.Name = "textChannels";
            this.textChannels.Size = new System.Drawing.Size(120, 20);
            this.textChannels.TabIndex = 7;
            // 
            // comboRate
            // 
            this.comboRate.FormattingEnabled = true;
            this.comboRate.Items.AddRange(new object[] {
            "",
            "8000",
            "11025",
            "22050",
            "44100",
            "48000"});
            this.comboRate.Location = new System.Drawing.Point(62, 84);
            this.comboRate.Name = "comboRate";
            this.comboRate.Size = new System.Drawing.Size(121, 21);
            this.comboRate.TabIndex = 6;
            // 
            // textAudioBitrate
            // 
            this.textAudioBitrate.Location = new System.Drawing.Point(63, 31);
            this.textAudioBitrate.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.textAudioBitrate.Name = "textAudioBitrate";
            this.textAudioBitrate.Size = new System.Drawing.Size(120, 20);
            this.textAudioBitrate.TabIndex = 5;
            // 
            // comboAudioCodec
            // 
            this.comboAudioCodec.FormattingEnabled = true;
            this.comboAudioCodec.Items.AddRange(new object[] {
            "",
            "mpga",
            "mp3",
            "mp4a",
            "a52",
            "vorb",
            "flac",
            "spx",
            "s16l",
            "wma2"});
            this.comboAudioCodec.Location = new System.Drawing.Point(62, 4);
            this.comboAudioCodec.Name = "comboAudioCodec";
            this.comboAudioCodec.Size = new System.Drawing.Size(121, 21);
            this.comboAudioCodec.TabIndex = 4;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 87);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(49, 13);
            this.label10.TabIndex = 3;
            this.label10.Text = "Частота";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 60);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(46, 13);
            this.label9.TabIndex = 2;
            this.label9.Text = "Каналы";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 33);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(48, 13);
            this.label8.TabIndex = 1;
            this.label8.Text = "Битрейт";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Кодек";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(4, 255);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 13;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(125, 255);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(5, 7);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(29, 13);
            this.label11.TabIndex = 15;
            this.label11.Text = "Имя";
            // 
            // textName
            // 
            this.textName.Location = new System.Drawing.Point(40, 4);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(156, 20);
            this.textName.TabIndex = 16;
            // 
            // FormTranscode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(202, 282);
            this.Controls.Add(this.textName);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormTranscode";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormTranscode";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textFps)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textVideoBitrate)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textChannels)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textAudioBitrate)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox textHeight;
        private System.Windows.Forms.TextBox textWidth;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown textFps;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown textVideoBitrate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboVideoCodec;
        private System.Windows.Forms.ComboBox comboMux;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.NumericUpDown textChannels;
        private System.Windows.Forms.ComboBox comboRate;
        private System.Windows.Forms.NumericUpDown textAudioBitrate;
        private System.Windows.Forms.ComboBox comboAudioCodec;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textName;

    }
}