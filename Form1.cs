using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Gma.System.MouseKeyHook;
using NAudio.CoreAudioApi;
using ScreenDimmer.Properties;
using Microsoft.Win32;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace ScreenDimmer
{
    public partial class Form1 : Form
    {
        public int secs;
        Form2 dimmer;
        private IKeyboardMouseEvents _globalHook;

        private NotifyIcon notifyIcon1;

        public Form1()
        {
            InitializeComponent();

            this.KeyPreview = true;
            dimmer = new Form2();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            numericUpDown2.Value = Properties.Settings.Default.DimmingFactor;
            numericUpDown1.Value = Properties.Settings.Default.Seconds;

            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;
            _globalHook.MouseMove += GlobalHook_MouseMove;
            _globalHook.MouseDown += GlobalHook_MouseDown;

            checkBox1.Checked = Properties.Settings.Default.StartWithWindows;

            notifyIcon1 = new NotifyIcon();
            notifyIcon1.Icon = SystemIcons.Application;
            notifyIcon1.Text = "Dim Screen running...";
            notifyIcon1.Visible = false;

            notifyIcon1.MouseDoubleClick += NotifyIcon1_MouseDoubleClick;


            ResetSeconds();

            timer1.Start();
            timer2.Start();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();
        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private bool IsAudioPlaying()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                float volume = device.AudioMeterInformation.MasterPeakValue;

                return volume > 0.01f;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            ResetSeconds();
        }

        private void GlobalHook_MouseMove(object sender, MouseEventArgs e)
        {
            ResetSeconds();
        }

        private void GlobalHook_MouseDown(object sender, MouseEventArgs e)
        {
            ResetSeconds();
        }

        void ResetSeconds() 
        {
            secs = (int)numericUpDown1.Value * 5;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            secs--;
            if (secs == 0) 
            {
                dimmer.Show();
            }
            if (secs > 0) 
            {
                dimmer.Hide();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up and dispose of the hook
            if (_globalHook != null)
            {
                _globalHook.KeyDown -= GlobalHook_KeyDown;
                _globalHook.Dispose();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (IsAudioPlaying()) 
            {
                ResetSeconds();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            MinimizeToFuckingTray();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            dimmer.Opacity = ((double)numericUpDown2.Value) / 100;

            Properties.Settings.Default.DimmingFactor = (int)numericUpDown2.Value;
            Properties.Settings.Default.Save();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Seconds = (int)numericUpDown1.Value;
            Properties.Settings.Default.Save();
        }

        private void SetStartup(bool enable)
        {
            string appName = "ScreenDimmer";
            string exePath = Assembly.GetExecutingAssembly().Location;

            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            if (enable)
            {
                rk.SetValue(appName, $"\"{exePath}\"");
                Properties.Settings.Default.StartWithWindows = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                rk.DeleteValue(appName, false);
                Properties.Settings.Default.StartWithWindows = false;
                Properties.Settings.Default.Save();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SetStartup(checkBox1.Checked);
        }

        private void MinimizeToFuckingTray() 
        {
            this.Hide();
            notifyIcon1.Visible = true;
        }
    }
}
