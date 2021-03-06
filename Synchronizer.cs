﻿using ComputerMonitorClient.RemoteClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerMonitorClient
{
    public class Synchronizer
    {
        private SynchronizerData newData = new SynchronizerData();
        private SynchronizerData oldData = new SynchronizerData();
        private bool synchInProgress = false;

        private BackgroundWorker backgroundSynchronizer;

        public Synchronizer()
        {
            LoadFromSettings();

            if (newData.Date == default(DateTime))
            {
                oldData.Synchronized = true;
            }
            else if (newData.Date != DateTime.Today.Date)
            {
                oldData = newData.CopyWithoutSynch();
                oldData.Synchronized = false;

                newData.ResetData();

                SaveInSettings();
            }

            StartSynchronizing();
        }

        public SynchronizerData TodayData()
        {
            return newData.CopyWithoutSynch();
        }

        public void SaveTodayDate(double download, double upload)
        {
            if (newData.Date == default(DateTime) || newData.Date == DateTime.Today.Date)
            {
                newData.Date = DateTime.Today.Date;
                newData.Download = download;
                newData.Upload = upload;
            }
            else
            {
                oldData = newData.CopyWithoutSynch();
                oldData.Synchronized = false;

                newData.ResetData();
            }
            SaveInSettings();
            StartSynchronizing();
        }

        public SynchronizerData LoadTotalData()
        {
            string token = Settings.Token;
            int deviceId = Settings.DeviceId;

            double download = newData.Download;
            double upload = newData.Upload;

            List<Usage> usageList = Client.LoadUsage(token, deviceId);
            if (usageList != null)
            {
                download += usageList.Sum(x => x.download);
                upload += usageList.Sum(x => x.upload);
            }

            return new SynchronizerData() { Download = download, Upload = upload };
        }

        private void LoadFromSettings()
        {
            newData.LoadAsNewData();
            oldData.LoadAsOldData();
        }

        private void SaveInSettings()
        {
            newData.SaveAsNewData();
            oldData.SaveAsOldData();
        }

        public void StartSynchronizing()
        {
            if (!oldData.Synchronized && !synchInProgress)
            {
                backgroundSynchronizer = new BackgroundWorker();
                backgroundSynchronizer.DoWork += new DoWorkEventHandler(SynchronizeBackground);
                backgroundSynchronizer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(SynchronizeComplete);
                backgroundSynchronizer.RunWorkerAsync();
            }
        }

        private void SynchronizeBackground(object sender, DoWorkEventArgs e)
        {
            string token = Settings.Token;
            int deviceId = Settings.DeviceId;
            bool error = false;

            synchInProgress = true;
            do
            {
                try
                {
                    Status status;
                    status = Client.AddUsage(token, deviceId, oldData.Download, oldData.Upload, oldData.Date);
                    oldData.Synchronized = status.status;
                    error = false;
                }
                catch (Exception)
                {
                    error = true;
                    System.Threading.Thread.Sleep(5000);
                }

            } while (error);
            synchInProgress = false;
        }

        private void SynchronizeComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            SaveInSettings();
            if (!oldData.Synchronized)
            {
                // TODO Token or deviceId is invalid
                throw new NotImplementedException("Token or deviceId invalid");
            }
        }
    }

    public class SynchronizerData
    {
        public double Download { get; set; }
        public double Upload { get; set; }
        public DateTime Date { get; set; }
        public bool Synchronized { get; set; }

        public SynchronizerData CopyWithoutSynch()
        {
            return new SynchronizerData()
            {
                Download = Download,
                Upload = Upload,
                Date = new DateTime(Date.Ticks)
            };
        }
        public void ResetData()
        {
            Download = 0d;
            Upload = 0d;
            Date = DateTime.Today.Date;
        }
        public void SaveAsNewData()
        {
            Settings.NewDownload = Download;
            Settings.NewUpload = Upload;
            Settings.NewDate = Date.Date;
            Settings.SaveSettings();
        }
        public void SaveAsOldData()
        {
            Settings.OldDownload = Download;
            Settings.OldUpload = Upload;
            Settings.OldDate = Date.Date;
            Settings.Synchronized = Synchronized;
            Settings.SaveSettings();
        }
        public void LoadAsNewData()
        {
            Download = Settings.NewDownload;
            Upload = Settings.NewUpload;
            Date = Settings.NewDate;
        }

        public void LoadAsOldData()
        {
            Download = Settings.OldDownload;
            Upload = Settings.OldUpload;
            Date = Settings.OldDate ?? DateTime.Now.Date;
            Synchronized = Settings.Synchronized;
        }
    }
}
