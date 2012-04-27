using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace Happyfeet
{
    class KinectGestureRecognizer
    {
        private const int bufferTime = 1000;
        private const float minKneeDepthChange = 0.1f;
        private const float minFootHeightChange = 0.15f;

        private Dictionary<long, KinectJointTrackedArgs> trackedLeftKnees;
        private Dictionary<long, KinectJointTrackedArgs> trackedLeftAnkles;
        private Dictionary<long, KinectJointTrackedArgs> trackedLeftFeet;

        private Dictionary<long, KinectJointTrackedArgs> trackedRightKnees;
        private Dictionary<long, KinectJointTrackedArgs> trackedRightAnkles;
        private Dictionary<long, KinectJointTrackedArgs> trackedRightFeet;

        private Dictionary<long, KinectJointTrackedArgs> trackedSpines;

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

            controller.LeftKneeTracked += LeftKneeTracked;
            controller.LeftAnkleTracked += LeftAnkleTracked;
            controller.LeftFootTracked += LeftFootTracked;

            controller.RightKneeTracked += RightKneeTracked;
            controller.RightAnkleTracked += RightAnkleTracked;
            controller.RightFootTracked += RightFootTracked;

            controller.SpineTracked += SpineTracked;
        }

        private void CheckForStamp(ref Dictionary<long, KinectJointTrackedArgs> trackedKnees, ref Dictionary<long, KinectJointTrackedArgs> trackedAnkles, ref Dictionary<long, KinectJointTrackedArgs> trackedFeet, long currentTimestamp)
        {
            trackedKnees = RemoveOldTracks(trackedKnees, currentTimestamp);
            trackedAnkles = RemoveOldTracks(trackedAnkles, currentTimestamp);
            trackedFeet = RemoveOldTracks(trackedFeet, currentTimestamp);
            trackedSpines = RemoveOldTracks(trackedSpines, currentTimestamp);

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
                bool footCorrect = (((maxFootHeight.position.Y - minFootHeightBefore.position.Y) >= minFootHeightChange) && ((maxFootHeight.position.Y - minFootHeightAfter.position.Y) >= minFootHeightChange));

                if (kneeCorrect && ankleCorrect && footCorrect)
                {
                    // Stamp detected
                    SkeletonPoint position = trackedSpines[trackedSpines.Keys.Max()].position;
                    OnStampDetected(new KinectStampDetectedArgs(position, currentTimestamp));

                    trackedKnees.Clear();
                    trackedAnkles.Clear();
                    trackedFeet.Clear();
                }
            }
            catch (NullReferenceException ex)
            {
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
            trackedLeftKnees.Add(e.timestamp, e);
        }

        private void LeftAnkleTracked(object sender, KinectJointTrackedArgs e)
        {
            trackedLeftAnkles.Add(e.timestamp, e);
        }

        private void LeftFootTracked(object sender, KinectJointTrackedArgs e)
        {
            trackedLeftFeet.Add(e.timestamp, e);
            CheckForStamp(ref trackedLeftKnees, ref trackedLeftAnkles, ref trackedLeftFeet, e.timestamp);
        }

        private void RightKneeTracked(object sender, KinectJointTrackedArgs e)
        {
            trackedRightKnees.Add(e.timestamp, e);
        }

        private void RightAnkleTracked(object sender, KinectJointTrackedArgs e)
        {
            trackedRightAnkles.Add(e.timestamp, e);
        }

        private void RightFootTracked(object sender, KinectJointTrackedArgs e)
        {
            trackedRightFeet.Add(e.timestamp, e);
            CheckForStamp(ref trackedRightKnees, ref trackedRightAnkles, ref trackedRightFeet, e.timestamp);
        }

        private void SpineTracked(object sender, KinectJointTrackedArgs e)
        {
            trackedSpines.Add(e.timestamp, e);
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
}
