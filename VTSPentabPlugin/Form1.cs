using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VTSPentabPlugin
{
    public partial class Form1 : Form
    {
        private PentabInfoPicker _pentab = new PentabInfoPicker();
        public Form1()
        {
            InitializeComponent();
            var first = new DeviceData("Wacom:Cintq13", 0x56A, 0x0A, 0xff00);
            comboBox1.Items.Add(first);
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
        }
    }
}
