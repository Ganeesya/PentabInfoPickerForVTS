using System;
using System.Collections.Generic;
using System.Windows.Forms;
using VTS.Models;

namespace VTSPentabPlugin
{
    public partial class Form1 : Form
    {
        private PentabInfoPicker _pentab = new PentabInfoPicker();

        private VTSSender _sender = new VTSSender();


        public Form1()
        {
            InitializeComponent();
            var first = new DeviceData("Wacom:Cintq13HD", 0x56A, 0x0A, 0xff00);
            comboBox1.Items.Add(first);
            comboBox1.SelectedItem = first;
            _pentab.Init(first.VenderId, first.Usage, first.UsagePage);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var currentPentabInfo = _pentab.GetState();

            onTabletBox.Text = currentPentabInfo.isOnTab.ToString();
            positionXRewBox.Text = currentPentabInfo.pointX.ToString();
            positionYRewBox.Text = currentPentabInfo.pointY.ToString();
            tiltXRewBox.Text = currentPentabInfo.tiltX.ToString();
            tiltYRewBox.Text = currentPentabInfo.tiltY.ToString();
            presserRewBox.Text = currentPentabInfo.presser.ToString();

            if (currentPentabInfo.pointX > int.Parse(positionXMaxBox.Text))
            {
                positionXMaxBox.Text = currentPentabInfo.pointX.ToString();
            }

            if (currentPentabInfo.pointY > int.Parse(positionYMaxBox.Text))
            {
                positionYMaxBox.Text = currentPentabInfo.pointY.ToString();
            }

            if (currentPentabInfo.tiltX > int.Parse(tiltXMaxBox.Text))
            {
                tiltXMaxBox.Text = currentPentabInfo.tiltX.ToString();
            }

            if (currentPentabInfo.tiltY > int.Parse(tiltYMaxBox.Text))
            {
                tiltYMaxBox.Text = currentPentabInfo.tiltY.ToString();
            }

            if (currentPentabInfo.presser > int.Parse(presserMaxBox.Text))
            {
                presserMaxBox.Text = currentPentabInfo.presser.ToString();
            }

            _sender._onTable.UpdateValue(currentPentabInfo.isOnTab ?1 : 0, 1);
            _sender._posX.UpdateValue(currentPentabInfo.pointX, int.Parse(positionXMaxBox.Text));
            _sender._posY.UpdateValue(currentPentabInfo.pointY, int.Parse(positionYMaxBox.Text));
            _sender._tiltX.UpdateValue(currentPentabInfo.tiltX, int.Parse(tiltXMaxBox.Text));
            _sender._tiltY.UpdateValue(currentPentabInfo.tiltY, int.Parse(tiltYMaxBox.Text));
            _sender._pre.UpdateValue(currentPentabInfo.presser, int.Parse(presserMaxBox.Text));

            _sender.SendCustomInput();

            this.Update();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _pentab.Close();
            var select = (DeviceData)comboBox1.SelectedItem;
            _pentab.Init(select.VenderId, select.Usage, select.UsagePage);
        }

        private void fpsBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var fps = int.Parse(TestFps.Text);
                timer1.Interval = 1000 / fps;
            }
            finally{}
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _pentab.Close();
            _sender.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void positionXMaxBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int ret;
            e.Cancel = !int.TryParse((sender as TextBox).Text,out ret);
        }

        private void positionYMaxBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int ret;
            e.Cancel = !int.TryParse((sender as TextBox).Text, out ret);
        }

        private void tiltXMaxBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int ret;
            e.Cancel = !int.TryParse((sender as TextBox).Text, out ret);
        }

        private void tiltYMaxBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int ret;
            e.Cancel = !int.TryParse((sender as TextBox).Text, out ret);
        }

        private void presserMaxBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int ret;
            e.Cancel = !int.TryParse((sender as TextBox).Text, out ret);
        }

        private void TestFps_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int ret;
            e.Cancel = !int.TryParse((sender as TextBox).Text, out ret);
        }
    }

    public class InjectionInfo
    {
        public string name;
        public string explanation;

        public float min = 0f;
        public float max = 1f;
        public float defalutValue = 0f;

        public float value = 0f;

        public float sendWeight = 0.8f;

        public InjectionInfo(string _name, string _explanation)
        {
            name = _name;
            explanation = _explanation;
        }

        public void UpdateValue(int rew, int maxIn)
        {
            value = (float) rew / (float) maxIn;
        }

        public VTSCustomParameter ConvertCustomParameter()
        {
            VTSCustomParameter ret = new VTSCustomParameter();

            ret.explanation = explanation;
            ret.defaultValue = defalutValue;
            ret.max = max;
            ret.min = min;
            ret.parameterName = name;

            return ret;
        }

        public VTSParameterInjectionValue ConvertInjectionValue()
        {
            VTSParameterInjectionValue ret = new VTSParameterInjectionValue();

            ret.id = name;
            ret.value = value;
            ret.weight = sendWeight;

            return ret;
        }
    }
}
