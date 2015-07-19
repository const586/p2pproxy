namespace gui
{
    partial class FormOption
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnDelTranscode = new System.Windows.Forms.Button();
            this.btnTransAdd = new System.Windows.Forms.Button();
            this.btnTransEdit = new System.Windows.Forms.Button();
            this.comboTranscode = new System.Windows.Forms.ComboBox();
            this.lblVlcTranscode = new System.Windows.Forms.Label();
            this.txtVlcRtspPort = new System.Windows.Forms.TextBox();
            this.txtExtVlcPath = new System.Windows.Forms.TextBox();
            this.txtVlcMux = new System.Windows.Forms.TextBox();
            this.txtVlcCache = new System.Windows.Forms.TextBox();
            this.txtVlcPort = new System.Windows.Forms.TextBox();
            this.lblVlcRtspPort = new System.Windows.Forms.Label();
            this.btnChooseVlcPath = new System.Windows.Forms.Button();
            this.chkExtVlc = new System.Windows.Forms.CheckBox();
            this.lblVlcMux = new System.Windows.Forms.Label();
            this.lblVlcCache = new System.Windows.Forms.Label();
            this.lblVlcPort = new System.Windows.Forms.Label();
            this.tabPageWeb = new System.Windows.Forms.TabPage();
            this.textBoxHttpPort = new System.Windows.Forms.TextBox();
            this.labelWebPort = new System.Windows.Forms.Label();
            this.tabPageSystem = new System.Windows.Forms.TabPage();
            this.chkAutoUpdate = new System.Windows.Forms.CheckBox();
            this.txtAsTimeOut = new System.Windows.Forms.TextBox();
            this.textRecordPath = new System.Windows.Forms.TextBox();
            this.as_timeout = new System.Windows.Forms.Label();
            this.btnSetRecordPath = new System.Windows.Forms.Button();
            this.record_path = new System.Windows.Forms.Label();
            this.tabPageAuth = new System.Windows.Forms.TabPage();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.textBoxLogin = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.labelLogin = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.chckDlnaEnabled = new System.Windows.Forms.CheckBox();
            this.txtLabelDlna = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.cbProfile = new System.Windows.Forms.ComboBox();
            this.txtUdn = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtUpnpPort = new System.Windows.Forms.NumericUpDown();
            this.txtMaxAge = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.chkUseVLC = new System.Windows.Forms.CheckBox();
            this.tabPage1.SuspendLayout();
            this.tabPageWeb.SuspendLayout();
            this.tabPageSystem.SuspendLayout();
            this.tabPageAuth.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtUpnpPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaxAge)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(198, 233);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "button2";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(12, 233);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "button1";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.chkUseVLC);
            this.tabPage1.Controls.Add(this.btnDelTranscode);
            this.tabPage1.Controls.Add(this.btnTransAdd);
            this.tabPage1.Controls.Add(this.btnTransEdit);
            this.tabPage1.Controls.Add(this.comboTranscode);
            this.tabPage1.Controls.Add(this.lblVlcTranscode);
            this.tabPage1.Controls.Add(this.txtVlcRtspPort);
            this.tabPage1.Controls.Add(this.txtExtVlcPath);
            this.tabPage1.Controls.Add(this.txtVlcMux);
            this.tabPage1.Controls.Add(this.txtVlcCache);
            this.tabPage1.Controls.Add(this.txtVlcPort);
            this.tabPage1.Controls.Add(this.lblVlcRtspPort);
            this.tabPage1.Controls.Add(this.btnChooseVlcPath);
            this.tabPage1.Controls.Add(this.chkExtVlc);
            this.tabPage1.Controls.Add(this.lblVlcMux);
            this.tabPage1.Controls.Add(this.lblVlcCache);
            this.tabPage1.Controls.Add(this.lblVlcPort);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(278, 205);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Text = "VLC";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnDelTranscode
            // 
            this.btnDelTranscode.BackgroundImage = global::gui.Properties.Resources.minus;
            this.btnDelTranscode.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelTranscode.Location = new System.Drawing.Point(251, 136);
            this.btnDelTranscode.Name = "btnDelTranscode";
            this.btnDelTranscode.Size = new System.Drawing.Size(24, 24);
            this.btnDelTranscode.TabIndex = 15;
            this.btnDelTranscode.UseVisualStyleBackColor = true;
            this.btnDelTranscode.Click += new System.EventHandler(this.btnDelTranscode_Click);
            // 
            // btnTransAdd
            // 
            this.btnTransAdd.BackgroundImage = global::gui.Properties.Resources.plus;
            this.btnTransAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnTransAdd.Location = new System.Drawing.Point(227, 136);
            this.btnTransAdd.Name = "btnTransAdd";
            this.btnTransAdd.Size = new System.Drawing.Size(24, 24);
            this.btnTransAdd.TabIndex = 14;
            this.btnTransAdd.UseVisualStyleBackColor = true;
            this.btnTransAdd.Click += new System.EventHandler(this.btnTransAdd_Click);
            // 
            // btnTransEdit
            // 
            this.btnTransEdit.BackgroundImage = global::gui.Properties.Resources.pencil;
            this.btnTransEdit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnTransEdit.Location = new System.Drawing.Point(203, 136);
            this.btnTransEdit.Name = "btnTransEdit";
            this.btnTransEdit.Size = new System.Drawing.Size(24, 24);
            this.btnTransEdit.TabIndex = 13;
            this.btnTransEdit.UseVisualStyleBackColor = true;
            this.btnTransEdit.Click += new System.EventHandler(this.btnTransEdit_Click);
            // 
            // comboTranscode
            // 
            this.comboTranscode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTranscode.FormattingEnabled = true;
            this.comboTranscode.Location = new System.Drawing.Point(105, 138);
            this.comboTranscode.Name = "comboTranscode";
            this.comboTranscode.Size = new System.Drawing.Size(97, 21);
            this.comboTranscode.TabIndex = 12;
            // 
            // lblVlcTranscode
            // 
            this.lblVlcTranscode.AutoSize = true;
            this.lblVlcTranscode.Location = new System.Drawing.Point(3, 140);
            this.lblVlcTranscode.Name = "lblVlcTranscode";
            this.lblVlcTranscode.Size = new System.Drawing.Size(104, 13);
            this.lblVlcTranscode.TabIndex = 11;
            this.lblVlcTranscode.Text = "Транскодирование";
            // 
            // txtVlcRtspPort
            // 
            this.txtVlcRtspPort.Location = new System.Drawing.Point(97, 59);
            this.txtVlcRtspPort.MaxLength = 5;
            this.txtVlcRtspPort.Name = "txtVlcRtspPort";
            this.txtVlcRtspPort.Size = new System.Drawing.Size(70, 20);
            this.txtVlcRtspPort.TabIndex = 10;
            // 
            // txtExtVlcPath
            // 
            this.txtExtVlcPath.Location = new System.Drawing.Point(11, 179);
            this.txtExtVlcPath.Name = "txtExtVlcPath";
            this.txtExtVlcPath.Size = new System.Drawing.Size(216, 20);
            this.txtExtVlcPath.TabIndex = 7;
            this.txtExtVlcPath.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // txtVlcMux
            // 
            this.txtVlcMux.Location = new System.Drawing.Point(97, 111);
            this.txtVlcMux.MaxLength = 10;
            this.txtVlcMux.Name = "txtVlcMux";
            this.txtVlcMux.Size = new System.Drawing.Size(70, 20);
            this.txtVlcMux.TabIndex = 5;
            this.txtVlcMux.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.txtVlcMux.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // txtVlcCache
            // 
            this.txtVlcCache.Location = new System.Drawing.Point(97, 85);
            this.txtVlcCache.MaxLength = 5;
            this.txtVlcCache.Name = "txtVlcCache";
            this.txtVlcCache.Size = new System.Drawing.Size(70, 20);
            this.txtVlcCache.TabIndex = 3;
            this.txtVlcCache.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.txtVlcCache.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // txtVlcPort
            // 
            this.txtVlcPort.Location = new System.Drawing.Point(97, 34);
            this.txtVlcPort.MaxLength = 5;
            this.txtVlcPort.Name = "txtVlcPort";
            this.txtVlcPort.Size = new System.Drawing.Size(70, 20);
            this.txtVlcPort.TabIndex = 1;
            this.txtVlcPort.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.txtVlcPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // lblVlcRtspPort
            // 
            this.lblVlcRtspPort.AutoSize = true;
            this.lblVlcRtspPort.Location = new System.Drawing.Point(8, 62);
            this.lblVlcRtspPort.Name = "lblVlcRtspPort";
            this.lblVlcRtspPort.Size = new System.Drawing.Size(44, 13);
            this.lblVlcRtspPort.TabIndex = 9;
            this.lblVlcRtspPort.Text = "vlc_rtsp";
            // 
            // btnChooseVlcPath
            // 
            this.btnChooseVlcPath.Location = new System.Drawing.Point(228, 178);
            this.btnChooseVlcPath.Name = "btnChooseVlcPath";
            this.btnChooseVlcPath.Size = new System.Drawing.Size(24, 22);
            this.btnChooseVlcPath.TabIndex = 8;
            this.btnChooseVlcPath.Text = "...";
            this.btnChooseVlcPath.UseVisualStyleBackColor = true;
            this.btnChooseVlcPath.Click += new System.EventHandler(this.btnChooseVlcPath_Click);
            // 
            // chkExtVlc
            // 
            this.chkExtVlc.AutoSize = true;
            this.chkExtVlc.Location = new System.Drawing.Point(11, 156);
            this.chkExtVlc.Name = "chkExtVlc";
            this.chkExtVlc.Size = new System.Drawing.Size(60, 17);
            this.chkExtVlc.TabIndex = 6;
            this.chkExtVlc.Text = "ext_vlc";
            this.chkExtVlc.UseVisualStyleBackColor = true;
            this.chkExtVlc.CheckedChanged += new System.EventHandler(this.chkExtVlc_CheckedChanged);
            // 
            // lblVlcMux
            // 
            this.lblVlcMux.AutoSize = true;
            this.lblVlcMux.Location = new System.Drawing.Point(8, 114);
            this.lblVlcMux.Name = "lblVlcMux";
            this.lblVlcMux.Size = new System.Drawing.Size(83, 13);
            this.lblVlcMux.TabIndex = 4;
            this.lblVlcMux.Text = "vlc_start_cache";
            this.lblVlcMux.Click += new System.EventHandler(this.label2_Click);
            // 
            // lblVlcCache
            // 
            this.lblVlcCache.AutoSize = true;
            this.lblVlcCache.Location = new System.Drawing.Point(8, 88);
            this.lblVlcCache.Name = "lblVlcCache";
            this.lblVlcCache.Size = new System.Drawing.Size(57, 13);
            this.lblVlcCache.TabIndex = 2;
            this.lblVlcCache.Text = "vlc_cache";
            // 
            // lblVlcPort
            // 
            this.lblVlcPort.AutoSize = true;
            this.lblVlcPort.Location = new System.Drawing.Point(8, 37);
            this.lblVlcPort.Name = "lblVlcPort";
            this.lblVlcPort.Size = new System.Drawing.Size(45, 13);
            this.lblVlcPort.TabIndex = 0;
            this.lblVlcPort.Text = "vlc_port";
            // 
            // tabPageWeb
            // 
            this.tabPageWeb.Controls.Add(this.textBoxHttpPort);
            this.tabPageWeb.Controls.Add(this.labelWebPort);
            this.tabPageWeb.Location = new System.Drawing.Point(4, 22);
            this.tabPageWeb.Name = "tabPageWeb";
            this.tabPageWeb.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWeb.Size = new System.Drawing.Size(278, 205);
            this.tabPageWeb.TabIndex = 2;
            this.tabPageWeb.Text = "Web";
            this.tabPageWeb.UseVisualStyleBackColor = true;
            // 
            // textBoxHttpPort
            // 
            this.textBoxHttpPort.Location = new System.Drawing.Point(76, 6);
            this.textBoxHttpPort.MaxLength = 6;
            this.textBoxHttpPort.Name = "textBoxHttpPort";
            this.textBoxHttpPort.Size = new System.Drawing.Size(51, 20);
            this.textBoxHttpPort.TabIndex = 3;
            this.textBoxHttpPort.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBoxHttpPort.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // labelWebPort
            // 
            this.labelWebPort.AutoSize = true;
            this.labelWebPort.Location = new System.Drawing.Point(8, 9);
            this.labelWebPort.Name = "labelWebPort";
            this.labelWebPort.Size = new System.Drawing.Size(51, 13);
            this.labelWebPort.TabIndex = 2;
            this.labelWebPort.Text = "web_port";
            // 
            // tabPageSystem
            // 
            this.tabPageSystem.Controls.Add(this.chkAutoUpdate);
            this.tabPageSystem.Controls.Add(this.txtAsTimeOut);
            this.tabPageSystem.Controls.Add(this.textRecordPath);
            this.tabPageSystem.Controls.Add(this.as_timeout);
            this.tabPageSystem.Controls.Add(this.btnSetRecordPath);
            this.tabPageSystem.Controls.Add(this.record_path);
            this.tabPageSystem.Location = new System.Drawing.Point(4, 22);
            this.tabPageSystem.Name = "tabPageSystem";
            this.tabPageSystem.Size = new System.Drawing.Size(278, 205);
            this.tabPageSystem.TabIndex = 1;
            this.tabPageSystem.Text = "System";
            this.tabPageSystem.UseVisualStyleBackColor = true;
            // 
            // chkAutoUpdate
            // 
            this.chkAutoUpdate.AutoSize = true;
            this.chkAutoUpdate.Location = new System.Drawing.Point(7, 77);
            this.chkAutoUpdate.Name = "chkAutoUpdate";
            this.chkAutoUpdate.Size = new System.Drawing.Size(159, 17);
            this.chkAutoUpdate.TabIndex = 11;
            this.chkAutoUpdate.Text = "обновлять автоматически";
            this.chkAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // txtAsTimeOut
            // 
            this.txtAsTimeOut.Location = new System.Drawing.Point(157, 51);
            this.txtAsTimeOut.MaxLength = 5;
            this.txtAsTimeOut.Name = "txtAsTimeOut";
            this.txtAsTimeOut.Size = new System.Drawing.Size(70, 20);
            this.txtAsTimeOut.TabIndex = 10;
            this.txtAsTimeOut.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.txtAsTimeOut.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // textRecordPath
            // 
            this.textRecordPath.Location = new System.Drawing.Point(6, 25);
            this.textRecordPath.Name = "textRecordPath";
            this.textRecordPath.Size = new System.Drawing.Size(221, 20);
            this.textRecordPath.TabIndex = 4;
            // 
            // as_timeout
            // 
            this.as_timeout.AutoSize = true;
            this.as_timeout.Location = new System.Drawing.Point(3, 54);
            this.as_timeout.Name = "as_timeout";
            this.as_timeout.Size = new System.Drawing.Size(58, 13);
            this.as_timeout.TabIndex = 9;
            this.as_timeout.Text = "as_timeout";
            // 
            // btnSetRecordPath
            // 
            this.btnSetRecordPath.Location = new System.Drawing.Point(228, 24);
            this.btnSetRecordPath.Name = "btnSetRecordPath";
            this.btnSetRecordPath.Size = new System.Drawing.Size(24, 22);
            this.btnSetRecordPath.TabIndex = 5;
            this.btnSetRecordPath.Text = "...";
            this.btnSetRecordPath.UseVisualStyleBackColor = true;
            this.btnSetRecordPath.Click += new System.EventHandler(this.btnSetRecordPath_Click);
            // 
            // record_path
            // 
            this.record_path.AutoSize = true;
            this.record_path.Location = new System.Drawing.Point(4, 8);
            this.record_path.Name = "record_path";
            this.record_path.Size = new System.Drawing.Size(64, 13);
            this.record_path.TabIndex = 3;
            this.record_path.Text = "record_path";
            // 
            // tabPageAuth
            // 
            this.tabPageAuth.Controls.Add(this.textBoxPassword);
            this.tabPageAuth.Controls.Add(this.textBoxLogin);
            this.tabPageAuth.Controls.Add(this.labelPassword);
            this.tabPageAuth.Controls.Add(this.labelLogin);
            this.tabPageAuth.Location = new System.Drawing.Point(4, 22);
            this.tabPageAuth.Name = "tabPageAuth";
            this.tabPageAuth.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAuth.Size = new System.Drawing.Size(278, 205);
            this.tabPageAuth.TabIndex = 0;
            this.tabPageAuth.Text = "Auth";
            this.tabPageAuth.UseVisualStyleBackColor = true;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(63, 29);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(108, 20);
            this.textBoxPassword.TabIndex = 3;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxPassword_TextChanged);
            // 
            // textBoxLogin
            // 
            this.textBoxLogin.Location = new System.Drawing.Point(63, 3);
            this.textBoxLogin.Name = "textBoxLogin";
            this.textBoxLogin.Size = new System.Drawing.Size(108, 20);
            this.textBoxLogin.TabIndex = 2;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(8, 32);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(35, 13);
            this.labelPassword.TabIndex = 1;
            this.labelPassword.Text = "label1";
            // 
            // labelLogin
            // 
            this.labelLogin.AutoSize = true;
            this.labelLogin.Location = new System.Drawing.Point(7, 7);
            this.labelLogin.Name = "labelLogin";
            this.labelLogin.Size = new System.Drawing.Size(35, 13);
            this.labelLogin.TabIndex = 0;
            this.labelLogin.Text = "label1";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPageAuth);
            this.tabControl1.Controls.Add(this.tabPageSystem);
            this.tabControl1.Controls.Add(this.tabPageWeb);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(286, 231);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.chckDlnaEnabled);
            this.tabPage2.Controls.Add(this.txtLabelDlna);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.cbProfile);
            this.tabPage2.Controls.Add(this.txtUdn);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.txtUpnpPort);
            this.tabPage2.Controls.Add(this.txtMaxAge);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(278, 205);
            this.tabPage2.TabIndex = 4;
            this.tabPage2.Text = "DLNA";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // chckDlnaEnabled
            // 
            this.chckDlnaEnabled.AutoSize = true;
            this.chckDlnaEnabled.Location = new System.Drawing.Point(11, 138);
            this.chckDlnaEnabled.Name = "chckDlnaEnabled";
            this.chckDlnaEnabled.Size = new System.Drawing.Size(131, 17);
            this.chckDlnaEnabled.TabIndex = 10;
            this.chckDlnaEnabled.Text = "Использовать DLNA";
            this.chckDlnaEnabled.UseVisualStyleBackColor = true;
            // 
            // txtLabelDlna
            // 
            this.txtLabelDlna.Location = new System.Drawing.Point(68, 4);
            this.txtLabelDlna.Name = "txtLabelDlna";
            this.txtLabelDlna.Size = new System.Drawing.Size(201, 20);
            this.txtLabelDlna.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Подпись";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 111);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Профиль";
            // 
            // cbProfile
            // 
            this.cbProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProfile.Items.AddRange(new object[] {
            "По умолчанию"});
            this.cbProfile.Location = new System.Drawing.Point(92, 108);
            this.cbProfile.Name = "cbProfile";
            this.cbProfile.Size = new System.Drawing.Size(177, 21);
            this.cbProfile.TabIndex = 6;
            // 
            // txtUdn
            // 
            this.txtUdn.Location = new System.Drawing.Point(41, 82);
            this.txtUdn.Name = "txtUdn";
            this.txtUdn.Size = new System.Drawing.Size(228, 20);
            this.txtUdn.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Udn";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Порт";
            // 
            // txtUpnpPort
            // 
            this.txtUpnpPort.Location = new System.Drawing.Point(194, 56);
            this.txtUpnpPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.txtUpnpPort.Name = "txtUpnpPort";
            this.txtUpnpPort.Size = new System.Drawing.Size(75, 20);
            this.txtUpnpPort.TabIndex = 2;
            this.txtUpnpPort.Value = new decimal(new int[] {
            1900,
            0,
            0,
            0});
            // 
            // txtMaxAge
            // 
            this.txtMaxAge.Location = new System.Drawing.Point(194, 29);
            this.txtMaxAge.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.txtMaxAge.Name = "txtMaxAge";
            this.txtMaxAge.Size = new System.Drawing.Size(75, 20);
            this.txtMaxAge.TabIndex = 1;
            this.txtMaxAge.Value = new decimal(new int[] {
            127,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(144, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Интервал уведомлений (c.)";
            // 
            // chkUseVLC
            // 
            this.chkUseVLC.AutoSize = true;
            this.chkUseVLC.Location = new System.Drawing.Point(11, 7);
            this.chkUseVLC.Name = "chkUseVLC";
            this.chkUseVLC.Size = new System.Drawing.Size(122, 17);
            this.chkUseVLC.TabIndex = 16;
            this.chkUseVLC.Text = "Использовать VLC";
            this.chkUseVLC.UseVisualStyleBackColor = true;
            this.chkUseVLC.CheckedChanged += new System.EventHandler(this.chkUseVLC_CheckedChanged);
            // 
            // FormOption
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(285, 261);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormOption";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormOption";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormOption_FormClosed);
            this.Load += new System.EventHandler(this.FormOption_Load);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPageWeb.ResumeLayout(false);
            this.tabPageWeb.PerformLayout();
            this.tabPageSystem.ResumeLayout(false);
            this.tabPageSystem.PerformLayout();
            this.tabPageAuth.ResumeLayout(false);
            this.tabPageAuth.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtUpnpPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaxAge)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btnDelTranscode;
        private System.Windows.Forms.Button btnTransAdd;
        private System.Windows.Forms.Button btnTransEdit;
        private System.Windows.Forms.ComboBox comboTranscode;
        private System.Windows.Forms.Label lblVlcTranscode;
        private System.Windows.Forms.TextBox txtVlcRtspPort;
        private System.Windows.Forms.TextBox txtExtVlcPath;
        private System.Windows.Forms.TextBox txtVlcMux;
        private System.Windows.Forms.TextBox txtVlcCache;
        private System.Windows.Forms.TextBox txtVlcPort;
        private System.Windows.Forms.Label lblVlcRtspPort;
        private System.Windows.Forms.Button btnChooseVlcPath;
        private System.Windows.Forms.CheckBox chkExtVlc;
        private System.Windows.Forms.Label lblVlcMux;
        private System.Windows.Forms.Label lblVlcCache;
        private System.Windows.Forms.Label lblVlcPort;
        private System.Windows.Forms.TabPage tabPageWeb;
        private System.Windows.Forms.TextBox textBoxHttpPort;
        private System.Windows.Forms.Label labelWebPort;
        private System.Windows.Forms.TabPage tabPageSystem;
        private System.Windows.Forms.TextBox textRecordPath;
        private System.Windows.Forms.Button btnSetRecordPath;
        private System.Windows.Forms.Label record_path;
        private System.Windows.Forms.TabPage tabPageAuth;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.TextBox textBoxLogin;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.Label labelLogin;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TextBox txtAsTimeOut;
        private System.Windows.Forms.Label as_timeout;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.NumericUpDown txtMaxAge;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbProfile;
        private System.Windows.Forms.TextBox txtUdn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown txtUpnpPort;
        private System.Windows.Forms.TextBox txtLabelDlna;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkAutoUpdate;
        private System.Windows.Forms.CheckBox chckDlnaEnabled;
        private System.Windows.Forms.CheckBox chkUseVLC;
    }
}