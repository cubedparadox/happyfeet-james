using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Happyfeet
{
    class KinectController
    {
        private Dictionary<string, KinectSensor> kinectSensors;
        private Skeleton[] skeletonData;

        #region Event definitions
        public event KinectInitializingHandler Initializing;
        public event KinectNotPoweredHandler NotPowered;
        public event KinectReadyHandler Ready;
        public event KinectDisconnectedHandler Disconnected;
        public event KinectErrorHandler Error;
        public event KinectStreamEnabled StreamEnabled;
        public event KinectStreamDisabled StreamDisabled;
        public event KinectSkeletonTracked SkeletonTracked;
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

        protected virtual void OnStreamEnabled(KinectStatusArgs e)
        {
            if (StreamEnabled != null)
                StreamEnabled(this, e);
        }

        protected virtual void OnStreamDisabled(KinectStatusArgs e)
        {
            if (StreamDisabled != null)
                StreamDisabled(this, e);
        }

        protected virtual void OnSkeletonTracked(KinectSkeletonTrackedArgs e)
        {
            if (SkeletonTracked != null)
                SkeletonTracked(this, e);
        }
        #endregion

        public KinectController()
        {
            this.kinectSensors = new Dictionary<string, KinectSensor>();
        }

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
                HandleStream(sensor);
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
            HandleStream(sensor);
        }

        private void SkeletonReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((this.skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);

                    foreach (Skeleton skeleton in this.skeletonData)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            OnSkeletonTracked(new KinectSkeletonTrackedArgs(skeleton));
                        }
                    }
                }
            }
        }

        private void UpdateListeners(KinectSensor sensor)
        {
            if (!kinectSensors.ContainsValue(sensor))
                kinectSensors.Add(sensor.DeviceConnectionId, sensor);
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

        private void HandleStream(KinectSensor sensor)
        {
            if (sensor.Status == KinectStatus.Connected)
            {
                sensor.SkeletonFrameReady += this.SkeletonReady;
                sensor.SkeletonStream.Enable(new TransformSmoothParameters()
                                            {
                                                Smoothing = 0.5f,
                                                Correction = 0.5f,
                                                Prediction = 0.5f,
                                                JitterRadius = 0.05f,
                                                MaxDeviationRadius = 0.04f
                                            });
                try
                {
                    sensor.Start();
                }
                catch (IOException)
                {

                }
                OnStreamEnabled(new KinectStatusArgs(sensor.DeviceConnectionId));
            }
            else
            {
                OnStreamDisabled(new KinectStatusArgs(sensor.DeviceConnectionId));
                sensor.Stop();
            }
        }
    }

    #region Delegate definitions
    public delegate void KinectInitializingHandler(object sender, KinectStatusArgs e);
    public delegate void KinectNotPoweredHandler(object sender, KinectStatusArgs e);
    public delegate void KinectReadyHandler(object sender, KinectStatusArgs e);
    public delegate void KinectDisconnectedHandler(object sender, KinectStatusArgs e);
    public delegate void KinectErrorHandler(object sender, KinectErrorArgs e);
    public delegate void KinectStreamEnabled(object sender, KinectStatusArgs e);
    public delegate void KinectStreamDisabled(object sender, KinectStatusArgs e);
    public delegate void KinectSkeletonTracked(object sender, KinectSkeletonTrackedArgs e);
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

    public class KinectSkeletonTrackedArgs : EventArgs
    {
        public KinectSkeletonTrackedArgs(Skeleton skeleton)
        {
            this.skeleton = skeleton;
        }

        public Skeleton skeleton;
    }
    #endregion
}
