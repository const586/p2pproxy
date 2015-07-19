using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TTVApi;
using P2pProxy;

namespace gui
{
    public partial class FormRecords : Form
    {
        private readonly Timer _rectm;
        private readonly P2pProxyApp _app;
        public FormRecords(P2pProxyApp app)
        {
            InitializeComponent();
            this._app = app;
            Icon = Properties.Resources.recsystem;
            buttonAdd.BackgroundImage = Properties.Resources.addrec.ToBitmap();
            buttonDel.BackgroundImage = Properties.Resources.remrec.ToBitmap();
            btnOnRecord.Image = Properties.Resources.waitrec;
            btnPauseRecord.Image = Properties.Resources.pause;
            btnStartRecord.Image = Properties.Resources.record;
            btnStopRecord.Image = Properties.Resources.finished;
            AddRecToolStripMenuItem.Image = Properties.Resources.addrec.ToBitmap();
            delRecToolStripMenuItem.Image = Properties.Resources.remrec.ToBitmap();
            OnRecToolStripMenuItem.Image = Properties.Resources.waitrec;
            StopRecToolStripMenuItem.Image = Properties.Resources.finished;
            PauseRecToolStripMenuItem.Image = Properties.Resources.pause;
            StartRecToolStripMenuItem.Image = Properties.Resources.record;

            btnOnRecord.Text = "Включить";
            btnPauseRecord.Text = "Пауза";
            btnStartRecord.Text = "Запись";
            btnStopRecord.Text = "Остановить";
            AddRecToolStripMenuItem.Text = "Добавить";
            delRecToolStripMenuItem.Text = "Удалить";
            OnRecToolStripMenuItem.Text = "Включить";
            StopRecToolStripMenuItem.Text = "Остановить";
            PauseRecToolStripMenuItem.Text = "Пауза";
            StartRecToolStripMenuItem.Text = "Запись";

            ColStatus.HeaderText = "";
            ColName.HeaderText = "Имя";
            ColStartTime.HeaderText = "Начало";
            ColEndTime.HeaderText = "Конец";

            _rectm = new Timer { Interval = 1273, Enabled = true };
            _rectm.Tick += RectmOnTick;
            _rectm.Start();
        }

        private void RectmOnTick(object sender, EventArgs e)
        {
            List<DataGridViewRow> delrows;
            List<Records> addrecs;

            lock (dataGridView1.Rows)
            {
                delrows = dataGridView1.Rows.Cast<DataGridViewRow>().Where(row => !_app.Device.Records.GetRecords().Contains(row.Tag)).ToList();
                addrecs = _app.Device.Records.GetRecords().Where(rec => dataGridView1.Rows.Cast<DataGridViewRow>().All(row => row.Tag != rec)).ToList();
            }

            foreach (var row in delrows)
            {
                DelRow(row);
            }
            
            foreach (var rec in addrecs)
            {
                AddRec(rec);
            }
            UpdateRecords();
        }

        private void UpdateRecords()
        {
            lock (dataGridView1.Rows)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    var rec = row.Tag as Records;
                    if (rec == null)
                        return;
                    switch (rec.Status)
                    {
                        case RecordStatus.Finished:
                            row.Cells[0].Value = Properties.Resources.finished;
                            break;
                        case RecordStatus.Pause:
                            row.Cells[0].Value = Properties.Resources.pause;
                            break;
                        case RecordStatus.Wait:
                            row.Cells[0].Value = Properties.Resources.waitrec;
                            break;
                        case RecordStatus.Start:
                            row.Cells[0].Value = Properties.Resources.record;
                            break;
                        case RecordStatus.Starting:
                            row.Cells[0].Value = Properties.Resources.starting;
                            break;
                        case RecordStatus.Error:
                            row.Cells[0].Value = Properties.Resources.error;
                            break;
                        default:
                            row.Cells[0].Value = Properties.Resources.init;
                            break;
                    }
                    row.Cells[1].Value = rec.Name;
                    row.Cells[2].Value = rec.Start.ToString();
                    row.Cells[3].Value = rec.End.ToString();
                    if (dataGridView1.CurrentRow != null)
                        dataGridView1_RowEnter(null, new DataGridViewCellEventArgs(dataGridView1.CurrentCell.ColumnIndex, dataGridView1.CurrentCell.RowIndex));
                }
            }
        }

        private void AddRec(Records rec)
        {
            var dgvr = new DataGridViewRow {Tag = rec};
            dgvr.Cells.Add(new DataGridViewImageCell());
            switch (rec.Status)
            {
                case RecordStatus.Finished:
                    dgvr.Cells[0].Value = Properties.Resources.finished;
                    break;
                case RecordStatus.Pause:
                    dgvr.Cells[0].Value = Properties.Resources.pause;
                    break;
                case RecordStatus.Wait:
                    dgvr.Cells[0].Value = Properties.Resources.waitrec;
                    break;
                case RecordStatus.Start:
                    dgvr.Cells[0].Value = Properties.Resources.record;
                    break;
                case RecordStatus.Starting:
                    dgvr.Cells[0].Value = Properties.Resources.starting;
                    break;
                default:
                    dgvr.Cells[0].Value = Properties.Resources.init;
                    break;
            }
            dgvr.Cells.Add(new DataGridViewTextBoxCell());
            dgvr.Cells[1].Value = rec.Name;
            dgvr.Cells.Add(new DataGridViewTextBoxCell());
            dgvr.Cells[2].Value = rec.Start.ToString();
            dgvr.Cells.Add(new DataGridViewTextBoxCell());
            dataGridView1.Columns[2].DefaultCellStyle.Format = "G";
            dgvr.Cells[3].Value = rec.End.ToString();
            dataGridView1.Columns[3].DefaultCellStyle.Format = "G";
            lock (dataGridView1.Rows)
            {
                dataGridView1.Rows.Add(dgvr);
            }
        }

        private void DelRow(DataGridViewRow row)
        {
            lock (dataGridView1.Rows)
            {
                dataGridView1.Rows.Remove(row);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {

            var frm = new FormChannels(_app);
            frm.Choosed += FrmOnChoosed;
            frm.Show();
        }

        private void FrmOnChoosed(Telecast epg, Channel ch)
        {
            Records rec = new Records("http://127.0.0.1:" + _app.Device.Web.Port, _app.Device.Records.RecordsPath)
            {
                Name = string.Format("{0}", epg.name),
                Id = Guid.NewGuid().ToString(),
                Start = epg.StartTime,
                End = epg.EndTime,
                Status = RecordStatus.Init,
                TorrentId = ch.id
            };
            _app.Device.Records.Add(rec);
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                var record = dataGridView1.CurrentRow.Tag as Records;
                if (record != null)
                    _app.Device.Records.Del(record.Id);
                DelRow(dataGridView1.CurrentRow);
            }
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            lock (dataGridView1.Rows)
            {
                var rec = (Records)dataGridView1.Rows[e.RowIndex].Tag;
                if (rec == null)
                    return;
                btnOnRecord.Enabled = rec.Status == RecordStatus.Wait || rec.Status == RecordStatus.Init;
                btnOnRecord.Image = rec.Status == RecordStatus.Init
                                        ? Properties.Resources.waitrec
                                        : Properties.Resources.init;
                btnOnRecord.Text = rec.Status == RecordStatus.Init ? "Включить" : "Выключить";
                OnRecToolStripMenuItem.Enabled = rec.Status == RecordStatus.Wait || rec.Status == RecordStatus.Init;
                OnRecToolStripMenuItem.Image = rec.Status == RecordStatus.Init ? Properties.Resources.waitrec : Properties.Resources.init;
                OnRecToolStripMenuItem.Text = rec.Status == RecordStatus.Init ? "Включить" : "Выключить";
                OnRecToolStripMenuItem.Visible = btnOnRecord.Enabled;

                btnPauseRecord.Enabled = (rec.Status != RecordStatus.Init && rec.Status != RecordStatus.Wait && rec.Status != RecordStatus.Finished && rec.Status != RecordStatus.Pause);
                PauseRecToolStripMenuItem.Visible = btnPauseRecord.Enabled;

                btnStartRecord.Enabled = (rec.Status != RecordStatus.Init && rec.Status != RecordStatus.Finished && rec.Status != RecordStatus.Start);
                StartRecToolStripMenuItem.Visible = btnStartRecord.Enabled;

                btnStopRecord.Enabled = (rec.Status != RecordStatus.Init && rec.Status != RecordStatus.Wait && rec.Status != RecordStatus.Finished);
                StopRecToolStripMenuItem.Visible = btnStopRecord.Enabled;
            }
        }

        private void FormRecords_Load(object sender, EventArgs e)
        {
            RectmOnTick(sender, e);
        }

        private void contextMenuStrip1_Opened(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                OnRecToolStripMenuItem.Enabled = false;
                StopRecToolStripMenuItem.Enabled = false;
                PauseRecToolStripMenuItem.Enabled = false;
                StartRecToolStripMenuItem.Enabled = false;
            }
        }

        private void OnRecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                var rec = (dataGridView1.CurrentRow.Tag as Records);
                if (rec == null)
                    return;
                rec.Status = rec.Status == RecordStatus.Init ? RecordStatus.Wait : RecordStatus.Init;
                dataGridView1.CurrentRow.Cells[0].Value = rec.Status == RecordStatus.Init ? Properties.Resources.init : Properties.Resources.waitrec;
                btnOnRecord.Image = rec.Status == RecordStatus.Init ? Properties.Resources.waitrec : Properties.Resources.init;
                btnOnRecord.Text = rec.Status == RecordStatus.Init ? "Включить" : "Выключить";
                OnRecToolStripMenuItem.Image = rec.Status == RecordStatus.Init ? Properties.Resources.waitrec : Properties.Resources.init;
                OnRecToolStripMenuItem.Text = rec.Status == RecordStatus.Init ? "Включить" : "Выключить";
                dataGridView1_RowEnter(sender, new DataGridViewCellEventArgs(dataGridView1.CurrentCell.ColumnIndex, dataGridView1.CurrentCell.RowIndex));
            }
        }

        private void PauseRecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                var rec = (Records)(dataGridView1.CurrentRow.Tag);
                rec.Pause();
            }
        }

        private void StartRecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;
            var rec = (dataGridView1.CurrentRow.Tag as Records);
            if (rec == null)
                return;
            rec.Status = RecordStatus.Starting;
            dataGridView1_RowEnter(sender, new DataGridViewCellEventArgs(dataGridView1.CurrentCell.ColumnIndex, dataGridView1.CurrentCell.RowIndex));
            rec.Record();
        }

        private void StopRecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                var rec = (dataGridView1.CurrentRow.Tag as Records);
                if (rec == null)
                    return;
                rec.Stop();
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                var rec = (Records)dataGridView1.CurrentRow.Tag;
                if (rec != null)
                {
                    DateTime startTime;
                    DateTime endTime;
                    var ress = DateTime.TryParse((string)dataGridView1.CurrentRow.Cells[2].Value, out startTime);
                    var rese = DateTime.TryParse((string) dataGridView1.CurrentRow.Cells[3].Value, out endTime);
                    if (!ress || !rese || startTime > endTime)
                    {
                        MessageBox.Show("Неверные данные", "Менеджер записей", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        
                        startTime = DateTime.Now;
                        endTime = startTime.AddTicks(-1);
                        dataGridView1.CurrentRow.Cells[2].Value = startTime.ToString();
                        dataGridView1.CurrentRow.Cells[3].Value = endTime.ToString();
                        rec.Status = RecordStatus.Init;
                        dataGridView1.CurrentRow.Cells[0].Value = Properties.Resources.init;
                        dataGridView1_RowEnter(sender, new DataGridViewCellEventArgs(dataGridView1.CurrentCell.ColumnIndex, dataGridView1.CurrentCell.RowIndex));
                    }
                    rec.Name = (string)dataGridView1.CurrentRow.Cells[1].Value;
                    rec.Start = startTime;
                    rec.End = endTime;
                }
            }
        }

        private void FormRecords_FormClosed(object sender, FormClosedEventArgs e)
        {
            _rectm.Stop();
            _rectm.Dispose();
        }
    }
}
