﻿using ComputerMonitorClient.RemoteClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComputerMonitorClient
{
    /// <summary>
    /// Interaktionslogik für DevicePage.xaml
    /// </summary>
    public partial class DevicePage : Page, IStartSwitch
    {

        private IStartSwitch context;

        public DevicePage()
        {
            InitializeComponent();
        }

        public DevicePage(IStartSwitch context) : this()
        {
            this.context = context;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDeviceList();
            textDevice.Text = System.Environment.MachineName;
        }

        private void LoadDeviceList()
        {
            string token = Settings.Token;
            List<Device> devices = Client.LoadDevices(token);

            comboDevice.DisplayMemberPath = "name";
            comboDevice.ItemsSource = devices;
            textDevice.Text = "";
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            string token = Settings.Token.ToString();
            Status status = Client.AddDevice(token, textDevice.Text);
            if (status.status)
            {
                LoadDeviceList();
                MessageBox.Show(status.message, "Created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(status.message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnUse_Click(object sender, RoutedEventArgs e)
        {
            if(comboDevice.SelectedItem != null)
            {
                Settings.DeviceId = (comboDevice.SelectedItem as Device).id;
                Settings.DeviceName = (comboDevice.SelectedItem as Device).name;
                Settings.SaveSettings();
                this.SwitchToMainWindow();
            }
            else
            {
                MessageBox.Show("No Device selected!","Warning",MessageBoxButton.OK,MessageBoxImage.Warning);
            }

        }

        public void SwitchToMainWindow()
        {
            context.SwitchToMainWindow();
        }
    }
}
