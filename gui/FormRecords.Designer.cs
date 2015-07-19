namespace gui
{
    partial class FormRecords
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnStartRecord = new System.Windows.Forms.Button();
            this.btnPauseRecord = new System.Windows.Forms.Button();
            this.btnStopRecord = new System.Windows.Forms.Button();
            this.btnOnRecord = new System.Windows.Forms.Button();
            this.buttonDel = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ColStatus = new System.Windows.Forms.DataGridViewImageColumn();
            this.ColName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColStartTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColEndTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.AddRecToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.delRecToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OnRecToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PauseRecToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StartRecToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StopRecToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnStartRecord);
            this.panel1.Controls.Add(this.btnPauseRecord);
            this.panel1.Controls.Add(this.btnStopRecord);
            this.panel1.Controls.Add(this.btnOnRecord);
            this.panel1.Controls.Add(this.buttonDel);
            this.panel1.Controls.Add(this.buttonAdd);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(579, 31);
            this.panel1.TabIndex = 0;
            // 
            // btnStartRecord
            // 
            this.btnStartRecord.FlatAppearance.BorderSize = 0;
            this.btnStartRecord.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.btnStartRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartRecord.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStartRecord.Location = new System.Drawing.Point(453, 0);
            this.btnStartRecord.Name = "btnStartRecord";
            this.btnStartRecord.Size = new System.Drawing.Size(120, 32);
            this.btnStartRecord.TabIndex = 5;
            this.btnStartRecord.Text = "Запуск";
            this.btnStartRecord.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnStartRecord.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnStartRecord.UseVisualStyleBackColor = true;
            this.btnStartRecord.Click += new System.EventHandler(this.StartRecToolStripMenuItem_Click);
            // 
            // btnPauseRecord
            // 
            this.btnPauseRecord.FlatAppearance.BorderSize = 0;
            this.btnPauseRecord.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.btnPauseRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPauseRecord.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPauseRecord.Location = new System.Drawing.Point(329, 0);
            this.btnPauseRecord.Name = "btnPauseRecord";
            this.btnPauseRecord.Size = new System.Drawing.Size(120, 32);
            this.btnPauseRecord.TabIndex = 4;
            this.btnPauseRecord.Text = "Пауза";
            this.btnPauseRecord.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnPauseRecord.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPauseRecord.UseVisualStyleBackColor = true;
            this.btnPauseRecord.Click += new System.EventHandler(this.PauseRecToolStripMenuItem_Click);
            // 
            // btnStopRecord
            // 
            this.btnStopRecord.FlatAppearance.BorderSize = 0;
            this.btnStopRecord.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.btnStopRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopRecord.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnStopRecord.Location = new System.Drawing.Point(205, 0);
            this.btnStopRecord.Name = "btnStopRecord";
            this.btnStopRecord.Size = new System.Drawing.Size(120, 32);
            this.btnStopRecord.TabIndex = 3;
            this.btnStopRecord.Text = "Остановить";
            this.btnStopRecord.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnStopRecord.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnStopRecord.UseVisualStyleBackColor = true;
            this.btnStopRecord.Click += new System.EventHandler(this.StopRecToolStripMenuItem_Click);
            // 
            // btnOnRecord
            // 
            this.btnOnRecord.FlatAppearance.BorderSize = 0;
            this.btnOnRecord.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.btnOnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOnRecord.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOnRecord.Location = new System.Drawing.Point(82, 0);
            this.btnOnRecord.Name = "btnOnRecord";
            this.btnOnRecord.Size = new System.Drawing.Size(120, 32);
            this.btnOnRecord.TabIndex = 2;
            this.btnOnRecord.Text = "Включить";
            this.btnOnRecord.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnOnRecord.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnOnRecord.UseVisualStyleBackColor = true;
            this.btnOnRecord.Click += new System.EventHandler(this.OnRecToolStripMenuItem_Click);
            // 
            // buttonDel
            // 
            this.buttonDel.FlatAppearance.BorderSize = 0;
            this.buttonDel.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.buttonDel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDel.Location = new System.Drawing.Point(44, 0);
            this.buttonDel.Name = "buttonDel";
            this.buttonDel.Size = new System.Drawing.Size(32, 32);
            this.buttonDel.TabIndex = 1;
            this.buttonDel.UseVisualStyleBackColor = true;
            this.buttonDel.Click += new System.EventHandler(this.buttonDel_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonAdd.FlatAppearance.BorderSize = 0;
            this.buttonAdd.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonAdd.ForeColor = System.Drawing.SystemColors.ControlText;
            this.buttonAdd.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.buttonAdd.Location = new System.Drawing.Point(3, 0);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(32, 32);
            this.buttonAdd.TabIndex = 0;
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeight = 21;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColStatus,
            this.ColName,
            this.ColStartTime,
            this.ColEndTime});
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 31);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 15;
            this.dataGridView1.RowTemplate.Height = 20;
            this.dataGridView1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.ShowCellErrors = false;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.ShowRowErrors = false;
            this.dataGridView1.Size = new System.Drawing.Size(579, 230);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
            this.dataGridView1.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_RowEnter);
            // 
            // ColStatus
            // 
            this.ColStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColStatus.HeaderText = "Status";
            this.ColStatus.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.ColStatus.Name = "ColStatus";
            this.ColStatus.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColStatus.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ColStatus.Width = 32;
            // 
            // ColName
            // 
            this.ColName.HeaderText = "NameRecord";
            this.ColName.Name = "ColName";
            this.ColName.Width = 235;
            // 
            // ColStartTime
            // 
            dataGridViewCellStyle3.NullValue = null;
            this.ColStartTime.DefaultCellStyle = dataGridViewCellStyle3;
            this.ColStartTime.HeaderText = "StartTime";
            this.ColStartTime.Name = "ColStartTime";
            this.ColStartTime.Width = 150;
            // 
            // ColEndTime
            // 
            dataGridViewCellStyle4.NullValue = null;
            this.ColEndTime.DefaultCellStyle = dataGridViewCellStyle4;
            this.ColEndTime.HeaderText = "EndTime";
            this.ColEndTime.Name = "ColEndTime";
            this.ColEndTime.Width = 150;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AddRecToolStripMenuItem,
            this.delRecToolStripMenuItem,
            this.OnRecToolStripMenuItem,
            this.PauseRecToolStripMenuItem,
            this.StartRecToolStripMenuItem,
            this.StopRecToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(154, 136);
            this.contextMenuStrip1.Opened += new System.EventHandler(this.contextMenuStrip1_Opened);
            // 
            // AddRecToolStripMenuItem
            // 
            this.AddRecToolStripMenuItem.Name = "AddRecToolStripMenuItem";
            this.AddRecToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.AddRecToolStripMenuItem.Text = "Добавить";
            this.AddRecToolStripMenuItem.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // delRecToolStripMenuItem
            // 
            this.delRecToolStripMenuItem.Name = "delRecToolStripMenuItem";
            this.delRecToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.delRecToolStripMenuItem.Text = "Удалить";
            this.delRecToolStripMenuItem.Click += new System.EventHandler(this.buttonDel_Click);
            // 
            // OnRecToolStripMenuItem
            // 
            this.OnRecToolStripMenuItem.Name = "OnRecToolStripMenuItem";
            this.OnRecToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.OnRecToolStripMenuItem.Text = "Включить";
            this.OnRecToolStripMenuItem.Click += new System.EventHandler(this.OnRecToolStripMenuItem_Click);
            // 
            // PauseRecToolStripMenuItem
            // 
            this.PauseRecToolStripMenuItem.Name = "PauseRecToolStripMenuItem";
            this.PauseRecToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.PauseRecToolStripMenuItem.Text = "Пауза";
            this.PauseRecToolStripMenuItem.Click += new System.EventHandler(this.PauseRecToolStripMenuItem_Click);
            // 
            // StartRecToolStripMenuItem
            // 
            this.StartRecToolStripMenuItem.Name = "StartRecToolStripMenuItem";
            this.StartRecToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.StartRecToolStripMenuItem.Text = "Начать запись";
            this.StartRecToolStripMenuItem.Click += new System.EventHandler(this.StartRecToolStripMenuItem_Click);
            // 
            // StopRecToolStripMenuItem
            // 
            this.StopRecToolStripMenuItem.Name = "StopRecToolStripMenuItem";
            this.StopRecToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.StopRecToolStripMenuItem.Text = "Остановить";
            this.StopRecToolStripMenuItem.Click += new System.EventHandler(this.StopRecToolStripMenuItem_Click);
            // 
            // FormRecords
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(579, 261);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel1);
            this.MinimumSize = new System.Drawing.Size(595, 300);
            this.Name = "FormRecords";
            this.Text = "FormRecords";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormRecords_FormClosed);
            this.Load += new System.EventHandler(this.FormRecords_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonDel;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem AddRecToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem delRecToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem OnRecToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PauseRecToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StartRecToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StopRecToolStripMenuItem;
        private System.Windows.Forms.Button btnOnRecord;
        private System.Windows.Forms.Button btnStartRecord;
        private System.Windows.Forms.Button btnPauseRecord;
        private System.Windows.Forms.Button btnStopRecord;
        private System.Windows.Forms.DataGridViewImageColumn ColStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColStartTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColEndTime;
    }
}