using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public KinectGestureRecognizer(KinectController controller)
        {
            trackedLeftKnees = new Dictionary<long, KinectJointTrackedArgs>();
            trackedLeftAnkles = new Dictionary<long, KinectJointTrackedArgs>();
            trackedLeftFeet = new Dictionary<long, KinectJointTrackedArgs>();

            controller.LeftKneeTracked += LeftKneeTracked;
            controller.LeftAnkleTracked += LeftAnkleTracked;
            controller.LeftFootTracked += LeftFootTracked;
        }

        private void CheckForStamp(long currentTimestamp)
        {
            float minKneeDepth = float.MaxValue;
            float maxKneeDepth = float.MinValue;
            float minAnkleHeight = float.MaxValue;
            float maxAnkleHeight = float.MinValue;
            float minFootHeight = float.MaxValue;
            float maxFootHeight = float.MinValue;

            Dictionary<long, KinectJointTrackedArgs> leftKnees = new Dictionary<long, KinectJointTrackedArgs>(trackedLeftKnees);
            foreach (KeyValuePair<long, KinectJointTrackedArgs> trackInfo in leftKnees)
            {
                if ((currentTimestamp - trackInfo.Key) > bufferTime)
                    trackedLeftKnees.Remove(trackInfo.Key);
                else
                {
                    if (trackInfo.Value.position.Z < minKneeDepth)
                        minKneeDepth = trackInfo.Value.position.Z;
                    if (trackInfo.Value.position.Z > maxKneeDepth)
                        maxKneeDepth = trackInfo.Value.position.Z;
                }
            }

            Dictionary<long, KinectJointTrackedArgs> leftAnkles = new Dictionary<long, KinectJointTrackedArgs>(trackedLeftAnkles);
            foreach (KeyValuePair<long, KinectJointTrackedArgs> trackInfo in leftAnkles)
            {
                if ((currentTimestamp - trackInfo.Key) > bufferTime)
                    trackedLeftAnkles.Remove(trackInfo.Key);
                else
                {
                    if (trackInfo.Value.position.Y < minAnkleHeight)
                        minAnkleHeight = trackInfo.Value.position.Y;
                    if (trackInfo.Value.position.Y > maxAnkleHeight)
                        maxAnkleHeight = trackInfo.Value.position.Y;
                }
            }

            Dictionary<long, KinectJointTrackedArgs> leftFeet = new Dictionary<long, KinectJointTrackedArgs>(trackedLeftFeet);
            foreach (KeyValuePair<long, KinectJointTrackedArgs> trackInfo in leftFeet)
            {
                if ((currentTimestamp - trackInfo.Key) > bufferTime)
                    trackedLeftFeet.Remove(trackInfo.Key);
                else
                {
                    if (trackInfo.Value.position.Y < minFootHeight)
                        minFootHeight = trackInfo.Value.position.Y;
                    if (trackInfo.Value.position.Y > maxFootHeight)
                        maxFootHeight = trackInfo.Value.position.Y;
                }
            }

            if ((minKneeDepth != float.MaxValue) && (maxKneeDepth != float.MinValue) && (minAnkleHeight != float.MaxValue) && (maxAnkleHeight != float.MinValue) && (minFootHeight != float.MaxValue) && (maxFootHeight != float.MinValue))
            {
                if (((maxKneeDepth - minKneeDepth) > minKneeDepthChange) && ((maxAnkleHeight - minAnkleHeight) > minFootHeightChange) && ((maxFootHeight - minFootHeight) > minFootHeightChange))
                {
                    // Stamp detected
                    trackedLeftKnees.Clear();
                    trackedLeftAnkles.Clear();
                    trackedLeftFeet.Clear();
                }
            }
        }

        private void LeftKneeTracked(object sender, KinectJointTrackedArgs e)
        {
            trackedLeftKnees.Add(e.timestamp, e);
            CheckForStamp(e.timestamp);
        }

        private void LeftAnkleTracked(object sender, KinectJointTrackedArgs e)
        {
            trackedLeftAnkles.Add(e.timestamp, e);
            CheckForStamp(e.timestamp);
        }

        private void LeftFootTracked(object sender, KinectJointTrackedArgs e)
        {
            trackedLeftFeet.Add(e.timestamp, e);
            CheckForStamp(e.timestamp);
        }
    }
}
