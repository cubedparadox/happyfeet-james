using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Happyfeet
{
    class KinectController
    {
        #region Event definitions
        public event KinectInitializingHandler Initializing;
        public event KinectNotPoweredHandler NotPowered;
        public event KinectReadyHandler Ready;
        public event KinectDisconnectedHandler Disconnected;
        public event KinectErrorHandler Error;
        #endregion

        #region Event invokers
        protected virtual void OnInitializing(KinectStatusArgs e)
        {
            if (Initializing != null)
                Initializing(this, e);
        }

        protected virtual void OnNotPowered(KinectStatusArgs e)
        {
            if (NotPowered != null)
                NotPowered(this, e);
        }

        protected virtual void OnReady(KinectStatusArgs e)
        {
            if (Ready != null)
                Ready(this, e);
        }

        protected virtual void OnDisconnected(KinectStatusArgs e)
        {
            if (Disconnected != null)
                Disconnected(this, e);
        }
        
        protected virtual void OnError(KinectErrorArgs e)
        {
            if (Error != null)
                Error(this, e);
        }
        #endregion

        ~KinectController()
        {
            this.KinectStop();
        }

        public void KinectStart()
        {
            KinectSensor.KinectSensors.StatusChanged += this.StatusChanged;

            foreach (KinectSensor sensor in KinectSensor.KinectSensors)
            {
                UpdateListeners(sensor);
            }
        }

        public void KinectStop()
        {
            KinectSensor.KinectSensors.StatusChanged -= this.StatusChanged;
        }

        private void StatusChanged(object sender, StatusChangedEventArgs e)
        {
            KinectSensor sensor = e.Sensor;
            UpdateListeners(sensor);
        }

        private void UpdateListeners(KinectSensor sensor)
        {
            switch (sensor.Status)
            {
                case KinectStatus.Initializing:
                    OnInitializing(new KinectStatusArgs(sensor.DeviceConnectionId));
                    break;
                case KinectStatus.NotPowered:
                    OnNotPowered(new KinectStatusArgs(sensor.DeviceConnectionId));
                    break;
                case KinectStatus.Connected:
                    OnReady(new KinectStatusArgs(sensor.DeviceConnectionId));
                    break;
                case KinectStatus.Disconnected:
                    OnDisconnected(new KinectStatusArgs(sensor.DeviceConnectionId));
                    break;
                default:
                    OnError(new KinectErrorArgs(sensor.DeviceConnectionId, sensor.Status));
                    break;
            }
        }
    }

    #region Delegate definitions
    public delegate void KinectInitializingHandler(object sender, KinectStatusArgs e);
    public delegate void KinectNotPoweredHandler(object sender, KinectStatusArgs e);
    public delegate void KinectReadyHandler(object sender, KinectStatusArgs e);
    public delegate void KinectDisconnectedHandler(object sender, KinectStatusArgs e);
    public delegate void KinectErrorHandler(object sender, KinectErrorArgs e);
    #endregion

    #region Event argument definitions
    public class KinectStatusArgs : EventArgs
    {
        public KinectStatusArgs(string kinectID)
        {
            this.kinectID = kinectID;
        }

        public string kinectID;
    }

    public class KinectErrorArgs : EventArgs
    {
        public KinectErrorArgs(string kinectID, KinectStatus status)
        {
            this.kinectID = kinectID;
            this.status = status;
        }

        public string kinectID;
        public KinectStatus status;
    }
    #endregion
}
