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
        private List<int> trackedSkeletons;

        #region Event definitions
        public event KinectInitializingHandler Initializing;
        public event KinectNotPoweredHandler NotPowered;
        public event KinectReadyHandler Ready;
        public event KinectDisconnectedHandler Disconnected;
        public event KinectErrorHandler Error;
        public event KinectStreamEnabled StreamEnabled;
        public event KinectStreamDisabled StreamDisabled;
        public event KinectSkeletonTracked SkeletonTracked;
        public event KinectLeftFootTracked LeftFootTracked;
        public event KinectRightFootTracked RightFootTracked;
        public event KinectLeftAnkleTracked LeftAnkleTracked;
        public event KinectRightAnkleTracked RightAnkleTracked;
        public event KinectLeftKneeTracked LeftKneeTracked;
        public event KinectRightKneeTracked RightKneeTracked;
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

        protected virtual void OnLeftFootTracked(KinectJointTrackedArgs e)
        {
            if (LeftFootTracked != null)
                LeftFootTracked(this, e);
        }

        protected virtual void OnRightFootTracked(KinectJointTrackedArgs e)
        {
            if (RightFootTracked != null)
                RightFootTracked(this, e);
        }

        protected virtual void OnLeftAnkleTracked(KinectJointTrackedArgs e)
        {
            if (LeftAnkleTracked != null)
                LeftAnkleTracked(this, e);
        }

        protected virtual void OnRightAnkleTracked(KinectJointTrackedArgs e)
        {
            if (RightAnkleTracked != null)
                RightAnkleTracked(this, e);
        }

        protected virtual void OnLeftKneeTracked(KinectJointTrackedArgs e)
        {
            if (LeftKneeTracked != null)
                LeftKneeTracked(this, e);
        }

        protected virtual void OnRightKneeTracked(KinectJointTrackedArgs e)
        {
            if (RightKneeTracked != null)
                RightKneeTracked(this, e);
        }
        #endregion

        public KinectController()
        {
            this.kinectSensors = new Dictionary<string, KinectSensor>();
            this.trackedSkeletons = new List<int>();
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

                    bool found = false;

                    foreach (Skeleton skeleton in this.skeletonData)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            if (!this.trackedSkeletons.Contains(skeleton.TrackingId))
                            {
                                this.trackedSkeletons.Add(skeleton.TrackingId);
                            }

                            OnSkeletonTracked(new KinectSkeletonTrackedArgs(skeleton));

                            if (this.trackedSkeletons.Count > 0 && skeleton.TrackingId == this.trackedSkeletons.First())
                            {
                                found = true;
                                foreach (Joint joint in skeleton.Joints)
                                {
                                    if (joint.TrackingState == JointTrackingState.Tracked)
                                    {
                                        switch (joint.JointType)
                                        {
                                            case (JointType.FootLeft):
                                                OnLeftFootTracked(new KinectJointTrackedArgs(JointType.FootLeft, joint.Position, skeletonFrame.Timestamp));
                                                break;
                                            case (JointType.FootRight):
                                                OnRightFootTracked(new KinectJointTrackedArgs(JointType.FootRight, joint.Position, skeletonFrame.Timestamp));
                                                break;
                                            case (JointType.AnkleLeft):
                                                OnLeftAnkleTracked(new KinectJointTrackedArgs(JointType.AnkleLeft, joint.Position, skeletonFrame.Timestamp));
                                                break;
                                            case (JointType.AnkleRight):
                                                OnRightAnkleTracked(new KinectJointTrackedArgs(JointType.AnkleRight, joint.Position, skeletonFrame.Timestamp));
                                                break;
                                            case (JointType.KneeLeft):
                                                OnLeftKneeTracked(new KinectJointTrackedArgs(JointType.KneeLeft, joint.Position, skeletonFrame.Timestamp));
                                                break;
                                            case (JointType.KneeRight):
                                                OnRightKneeTracked(new KinectJointTrackedArgs(JointType.KneeRight, joint.Position, skeletonFrame.Timestamp));
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!found && this.trackedSkeletons.Count > 0)
                    {
                        this.trackedSkeletons.RemoveAt(0);
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
    public delegate void KinectLeftFootTracked(object sender, KinectJointTrackedArgs e);
    public delegate void KinectRightFootTracked(object sender, KinectJointTrackedArgs e);
    public delegate void KinectLeftAnkleTracked(object sender, KinectJointTrackedArgs e);
    public delegate void KinectRightAnkleTracked(object sender, KinectJointTrackedArgs e);
    public delegate void KinectLeftKneeTracked(object sender, KinectJointTrackedArgs e);
    public delegate void KinectRightKneeTracked(object sender, KinectJointTrackedArgs e);
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

    public class KinectJointTrackedArgs : EventArgs
    {
        public KinectJointTrackedArgs(JointType type, SkeletonPoint position, long timestamp)
        {
            this.type = type;
            this.position = position;
            this.timestamp = timestamp;
        }

        public JointType type;
        public SkeletonPoint position;
        public long timestamp;
    }
    #endregion
}
