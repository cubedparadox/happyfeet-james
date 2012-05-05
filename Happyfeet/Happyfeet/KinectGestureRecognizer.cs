using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Happyfeet
{
    public class KinectGestureRecognizer
    {
        private const int bufferTime = 1000;
        private const float minKneeDepthChange = 0.1f;
        private const float minFootHeightChange = 0.15f;
        private const float maxFootDepthChange = 0.15f;

        private Dictionary<long, KinectJointTrackedArgs> trackedLeftKnees;
        private Dictionary<long, KinectJointTrackedArgs> trackedLeftAnkles;
        private Dictionary<long, KinectJointTrackedArgs> trackedLeftFeet;

        private Dictionary<long, KinectJointTrackedArgs> trackedRightKnees;
        private Dictionary<long, KinectJointTrackedArgs> trackedRightAnkles;
        private Dictionary<long, KinectJointTrackedArgs> trackedRightFeet;

        private Dictionary<long, KinectJointTrackedArgs> trackedSpines;

        private BackgroundWorker leftStampRecognizer;
        private BackgroundWorker rightStampRecognizer;

        public event KinectStampDetectedHandler StampDetected;

        protected virtual void OnStampDetected(KinectStampDetectedArgs e)
        {
            if (StampDetected != null)
                StampDetected(this, e);
        }

        public KinectGestureRecognizer(KinectController controller)
        {
            trackedLeftKnees = new Dictionary<long, KinectJointTrackedArgs>();
            trackedLeftAnkles = new Dictionary<long, KinectJointTrackedArgs>();
            trackedLeftFeet = new Dictionary<long, KinectJointTrackedArgs>();

            trackedRightKnees = new Dictionary<long, KinectJointTrackedArgs>();
            trackedRightAnkles = new Dictionary<long, KinectJointTrackedArgs>();
            trackedRightFeet = new Dictionary<long, KinectJointTrackedArgs>();

            trackedSpines = new Dictionary<long, KinectJointTrackedArgs>();

            leftStampRecognizer = new BackgroundWorker();
            rightStampRecognizer = new BackgroundWorker();

            leftStampRecognizer.DoWork += new DoWorkEventHandler(leftStampRecognizer_DoWork);
            rightStampRecognizer.DoWork += new DoWorkEventHandler(rightStampRecognizer_DoWork);

            leftStampRecognizer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(leftStampRecognizer_RunWorkerCompleted);
            rightStampRecognizer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(rightStampRecognizer_RunWorkerCompleted);

            controller.LeftKneeTracked += LeftKneeTracked;
            controller.LeftAnkleTracked += LeftAnkleTracked;
            controller.LeftFootTracked += LeftFootTracked;

            controller.RightKneeTracked += RightKneeTracked;
            controller.RightAnkleTracked += RightAnkleTracked;
            controller.RightFootTracked += RightFootTracked;

            controller.SpineTracked += SpineTracked;
        }

        void rightStampRecognizer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                trackedRightKnees.Clear();
                trackedRightAnkles.Clear();
                trackedRightFeet.Clear();
                KinectStampDetectedArgs eventArgs = (KinectStampDetectedArgs) e.Result;
                OnStampDetected(eventArgs);
            }
        }

        void leftStampRecognizer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                trackedLeftKnees.Clear();
                trackedLeftAnkles.Clear();
                trackedLeftFeet.Clear();
                KinectStampDetectedArgs eventArgs = (KinectStampDetectedArgs) e.Result;
                OnStampDetected(eventArgs);
            }
        }

        void rightStampRecognizer_DoWork(object sender, DoWorkEventArgs e)
        {
            TrackingData trackingData = (TrackingData) e.Argument;

            if (CheckForStamp(trackingData.trackedKnees, trackingData.trackedAnkles, trackingData.trackedFeet))
            {
                SkeletonPoint position = trackingData.trackedSpines[trackingData.trackedSpines.Keys.Max()].position;
                e.Result = new KinectStampDetectedArgs(position, trackingData.currentTimestamp);
            }
        }

        void leftStampRecognizer_DoWork(object sender, DoWorkEventArgs e)
        {
            TrackingData trackingData = (TrackingData) e.Argument;

            if (CheckForStamp(trackingData.trackedKnees, trackingData.trackedAnkles, trackingData.trackedFeet))
            {
                SkeletonPoint poisition = trackingData.trackedSpines[trackingData.trackedSpines.Keys.Max()].position;
                e.Result = new KinectStampDetectedArgs(poisition, trackingData.currentTimestamp);
            }
        }

        private void CheckForLeftStamp(long currentTimestamp)
        {
            if (!leftStampRecognizer.IsBusy)
            {
                TrackingData data = new TrackingData();
                data.trackedKnees = new Dictionary<long, KinectJointTrackedArgs>(trackedLeftKnees);
                data.trackedAnkles = new Dictionary<long, KinectJointTrackedArgs>(trackedLeftAnkles);
                data.trackedFeet = new Dictionary<long, KinectJointTrackedArgs>(trackedLeftFeet);
                data.trackedSpines = new Dictionary<long, KinectJointTrackedArgs>(trackedSpines);
                data.currentTimestamp = currentTimestamp;

                leftStampRecognizer.RunWorkerAsync(data);
            }
        }

        private void CheckForRightStamp(long currentTimestamp)
        {
            if (!rightStampRecognizer.IsBusy)
            {
                TrackingData data = new TrackingData();
                data.trackedKnees = new Dictionary<long, KinectJointTrackedArgs>(trackedRightKnees);
                data.trackedAnkles = new Dictionary<long, KinectJointTrackedArgs>(trackedRightAnkles);
                data.trackedFeet = new Dictionary<long, KinectJointTrackedArgs>(trackedRightFeet);
                data.trackedSpines = new Dictionary<long, KinectJointTrackedArgs>(trackedSpines);
                data.currentTimestamp = currentTimestamp;

                rightStampRecognizer.RunWorkerAsync(data);
            }
        }

        private Boolean CheckForStamp(Dictionary<long, KinectJointTrackedArgs> trackedKnees, Dictionary<long, KinectJointTrackedArgs> trackedAnkles, Dictionary<long, KinectJointTrackedArgs> trackedFeet)
        {
            try
            {
                KinectJointTrackedArgs minKneeDepth = FindMinDepth(trackedKnees);
                KinectJointTrackedArgs maxKneeDepthBefore = FindMaxDepthBefore(trackedKnees, minKneeDepth.timestamp);
                KinectJointTrackedArgs maxKneeDepthAfter = FindMaxDepthAfter(trackedKnees, minKneeDepth.timestamp);

                KinectJointTrackedArgs maxAnkleHeight = FindMaxHeight(trackedAnkles);
                KinectJointTrackedArgs minAnkleHeightBefore = FindMinHeightBefore(trackedAnkles, maxAnkleHeight.timestamp);
                KinectJointTrackedArgs minAnkleHeihgtAfter = FindMinHeightAfter(trackedAnkles, maxAnkleHeight.timestamp);

                KinectJointTrackedArgs maxFootHeight = FindMaxHeight(trackedFeet);
                KinectJointTrackedArgs minFootHeightBefore = FindMinHeightBefore(trackedFeet, maxFootHeight.timestamp);
                KinectJointTrackedArgs minFootHeightAfter = FindMinHeightAfter(trackedFeet, maxFootHeight.timestamp);

                bool kneeCorrect = (((maxKneeDepthBefore.position.Z - minKneeDepth.position.Z) >= minKneeDepthChange) && ((maxKneeDepthAfter.position.Z - minKneeDepth.position.Z) >= minKneeDepthChange));
                bool ankleCorrect = (((maxAnkleHeight.position.Y - minAnkleHeightBefore.position.Y) >= minFootHeightChange) && ((maxAnkleHeight.position.Y - minAnkleHeihgtAfter.position.Y) >= minFootHeightChange));
                bool footCorrect = (((maxFootHeight.position.Y - minFootHeightBefore.position.Y) >= minFootHeightChange) && ((maxFootHeight.position.Y - minFootHeightAfter.position.Y) >= minFootHeightChange) && (Math.Abs(maxFootHeight.position.Z - minFootHeightBefore.position.Z) <= maxFootDepthChange) && (Math.Abs(maxFootHeight.position.Z - minFootHeightAfter.position.Z) <= maxFootDepthChange));

                if (kneeCorrect && ankleCorrect && footCorrect)
                {
                    // Stamp detected
                    return true;
                }
                else
                    return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        private Dictionary<long, KinectJointTrackedArgs> RemoveOldTracks(Dictionary<long, KinectJointTrackedArgs> tracks, long currentTimestamp)
        {
            Dictionary<long, KinectJointTrackedArgs> tempTracks = new Dictionary<long, KinectJointTrackedArgs>();
            foreach (KeyValuePair<long, KinectJointTrackedArgs> track in tracks)
            {
                if ((currentTimestamp - track.Key) <= bufferTime)
                    tempTracks.Add(track.Key, track.Value);
            }
            return tempTracks;
        }

        private KinectJointTrackedArgs FindMinDepth(Dictionary<long, KinectJointTrackedArgs> tracks)
        {
            KinectJointTrackedArgs minDepth = null;
            foreach (KinectJointTrackedArgs track in tracks.Values)
            {
                if ((minDepth == null) || (track.position.Z < minDepth.position.Z))
                    minDepth = track;
            }
            return minDepth;
        }

        private KinectJointTrackedArgs FindMaxDepthBefore(Dictionary<long, KinectJointTrackedArgs> tracks, long timestamp)
        {
            KinectJointTrackedArgs maxDepth = null;
            foreach (KeyValuePair<long, KinectJointTrackedArgs> track in tracks)
            {
                if (track.Key < timestamp)
                    if ((maxDepth == null) || (track.Value.position.Z > maxDepth.position.Z))
                        maxDepth = track.Value;
            }
            return maxDepth;
        }

        private KinectJointTrackedArgs FindMaxDepthAfter(Dictionary<long, KinectJointTrackedArgs> tracks, long timestamp)
        {
            KinectJointTrackedArgs maxDepth = null;
            foreach (KeyValuePair<long, KinectJointTrackedArgs> track in tracks)
            {
                if (track.Key > timestamp)
                    if ((maxDepth == null) || (track.Value.position.Z > maxDepth.position.Z))
                        maxDepth = track.Value;
            }
            return maxDepth;
        }

        private KinectJointTrackedArgs FindMaxHeight(Dictionary<long, KinectJointTrackedArgs> tracks)
        {
            KinectJointTrackedArgs maxHeight = null;
            foreach (KinectJointTrackedArgs track in tracks.Values)
            {
                if ((maxHeight == null) || (track.position.Y > maxHeight.position.Y))
                    maxHeight = track;
            }
            return maxHeight;
        }

        private KinectJointTrackedArgs FindMinHeightBefore(Dictionary<long, KinectJointTrackedArgs> tracks, long timestamp)
        {
            KinectJointTrackedArgs minHeight = null;
            foreach (KeyValuePair<long, KinectJointTrackedArgs> track in tracks)
            {
                if (track.Key < timestamp)
                    if ((minHeight == null) || (track.Value.position.Y < minHeight.position.Y))
                        minHeight = track.Value;
            }
            return minHeight;
        }

        private KinectJointTrackedArgs FindMinHeightAfter(Dictionary<long, KinectJointTrackedArgs> tracks, long timestamp)
        {
            KinectJointTrackedArgs minHeight = null;
            foreach (KeyValuePair<long, KinectJointTrackedArgs> track in tracks)
            {
                if (track.Key > timestamp)
                    if ((minHeight == null) || (track.Value.position.Y < minHeight.position.Y))
                        minHeight = track.Value;
            }
            return minHeight;
        }

        private void LeftKneeTracked(object sender, KinectJointTrackedArgs e)
        {
            try
            {
                trackedLeftKnees.Add(e.timestamp, e);
            }
            catch (ArgumentException)
            {
                trackedLeftKnees.Remove(e.timestamp);
                trackedLeftKnees.Add(e.timestamp, e);
            }
            trackedLeftKnees = RemoveOldTracks(trackedLeftKnees, e.timestamp);
        }

        private void LeftAnkleTracked(object sender, KinectJointTrackedArgs e)
        {
            try
            {
                trackedLeftAnkles.Add(e.timestamp, e);
            }
            catch (ArgumentException)
            {
                trackedLeftAnkles.Remove(e.timestamp);
                trackedLeftAnkles.Add(e.timestamp, e);
            }
            trackedLeftAnkles = RemoveOldTracks(trackedLeftAnkles, e.timestamp);
        }

        private void LeftFootTracked(object sender, KinectJointTrackedArgs e)
        {
            try
            {
                trackedLeftFeet.Add(e.timestamp, e);
            }
            catch (ArgumentException)
            {
                trackedLeftFeet.Remove(e.timestamp);
                trackedLeftFeet.Add(e.timestamp, e);
            }
            trackedLeftFeet = RemoveOldTracks(trackedLeftFeet, e.timestamp);
            CheckForLeftStamp(e.timestamp);
        }

        private void RightKneeTracked(object sender, KinectJointTrackedArgs e)
        {
            try
            {
                trackedRightKnees.Add(e.timestamp, e);
            }
            catch (ArgumentException)
            {
                trackedRightKnees.Remove(e.timestamp);
                trackedRightKnees.Add(e.timestamp, e);
            }
            trackedRightKnees = RemoveOldTracks(trackedRightKnees, e.timestamp);
        }

        private void RightAnkleTracked(object sender, KinectJointTrackedArgs e)
        {
            try
            {
                trackedRightAnkles.Add(e.timestamp, e);
            }
            catch (ArgumentException)
            {
                trackedRightAnkles.Remove(e.timestamp);
                trackedRightAnkles.Add(e.timestamp, e);
            }
            trackedRightAnkles = RemoveOldTracks(trackedRightAnkles, e.timestamp);
        }

        private void RightFootTracked(object sender, KinectJointTrackedArgs e)
        {
            try
            {
                trackedRightFeet.Add(e.timestamp, e);
            }
            catch (ArgumentException)
            {
                trackedRightFeet.Remove(e.timestamp);
                trackedRightFeet.Add(e.timestamp, e);
            }
            trackedRightFeet = RemoveOldTracks(trackedRightFeet, e.timestamp);
            CheckForRightStamp(e.timestamp);
        }

        private void SpineTracked(object sender, KinectJointTrackedArgs e)
        {
            try
            {
                trackedSpines.Add(e.timestamp, e);
            }
            catch (ArgumentException)
            {
                trackedSpines.Remove(e.timestamp);
                trackedSpines.Add(e.timestamp, e);
            }
            trackedSpines = RemoveOldTracks(trackedSpines, e.timestamp);
        }
    }

    public delegate void KinectStampDetectedHandler(object sender, KinectStampDetectedArgs e);

    public class KinectStampDetectedArgs : EventArgs
    {
        public KinectStampDetectedArgs(SkeletonPoint position, long timestamp)
        {
            this.position = position;
            this.timestamp = timestamp;
        }
        
        public SkeletonPoint position;
        public long timestamp;
    }

    public class TrackingData
    {
        public Dictionary<long, KinectJointTrackedArgs> trackedKnees;
        public Dictionary<long, KinectJointTrackedArgs> trackedAnkles;
        public Dictionary<long, KinectJointTrackedArgs> trackedFeet;
        public Dictionary<long, KinectJointTrackedArgs> trackedSpines;
        public long currentTimestamp;
    }
}
