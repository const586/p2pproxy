using System;
using System.Globalization;
using System.Windows.Forms;
using P2pProxy.Broadcasting.VLC;

namespace gui
{
    public partial class FormTranscode : Form
    {

        private Transcode _trans;
        public FormTranscode(Transcode trans)
        {
            InitializeComponent();
            _trans = trans;

            textName.Text = trans.Name;
            textAudioBitrate.Text = trans.AudioBitrate.ToString();
            textChannels.Text = trans.AudioChannels.ToString();
            textFps.Text = trans.VideoFps.ToString(new NumberFormatInfo { CurrencyDecimalSeparator = "."});
            textHeight.Text = trans.VideoHeight.ToString();
            textVideoBitrate.Text = trans.VideoBitrate.ToString();
            textWidth.Text = trans.VideoWidth.ToString();
            comboAudioCodec.SelectedItem = string.IsNullOrEmpty(trans.AudioCodec) ? "" : trans.AudioCodec;
            comboMux.SelectedItem = string.IsNullOrEmpty(trans.Incapsulate) ? "ts" : trans.Incapsulate;
            comboRate.SelectedText = trans.AudioRate > 0 ? trans.AudioRate.ToString() : "";
            comboVideoCodec.SelectedItem = string.IsNullOrEmpty(trans.VideoCodec) ? "" : trans.VideoCodec;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _trans.Name = textName.Text;
            _trans.AudioCodec = (string)comboAudioCodec.SelectedItem;
            _trans.Incapsulate = (string) comboMux.SelectedItem;
            _trans.VideoCodec = (string) comboVideoCodec.SelectedItem;
            _trans.Incapsulate = (string)comboMux.SelectedItem;
            ushort.TryParse(textAudioBitrate.Text, out _trans.AudioBitrate);
            ushort.TryParse(textChannels.Text, out _trans.AudioChannels);
            float.TryParse(textFps.Text, NumberStyles.Float, new NumberFormatInfo{ CurrencyDecimalSeparator = "."}, out _trans.VideoFps);
            ushort.TryParse(textHeight.Text, out _trans.VideoHeight);
            ushort.TryParse(textVideoBitrate.Text, out _trans.VideoBitrate);
            ushort.TryParse(textWidth.Text, out _trans.VideoWidth);
            uint.TryParse((string) comboMux.SelectedItem, out _trans.AudioRate);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
