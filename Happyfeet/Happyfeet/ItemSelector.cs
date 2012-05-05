using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Kinect;

namespace Happyfeet
{
    class ItemSelector
    {
        public static int numItems = 8;

        private Label calibrationLabel;
        private Label[] items;
        private SkeletonPoint[] itemPositions;
        private Boolean calibrated;
        private int currentCalibration;
        private int currentHover = -1;
        private int currentSelection = -1;

        public ItemSelector(Label[] items, Label calibrationLabel)
        {
            this.items = items;
            this.itemPositions = new SkeletonPoint[numItems];
            this.calibrationLabel = calibrationLabel;
        }

        public void Calibrate()
        {
            calibrated = false;
            currentCalibration = 0;
            items[currentCalibration].Background = Brushes.Orange;
            KinectAccessor.gestureRecognizer.StampDetected += this.StampDetected;
        }

        private void StampDetected(object sender, KinectStampDetectedArgs e)
        {
            if (calibrated)
            {
                if ((currentHover != -1) && (currentHover != currentSelection))
                {
                    if (currentSelection != -1)
                        items[currentSelection].Background = Brushes.Transparent;

                    currentSelection = currentHover;
                    items[currentHover].Background = Brushes.LightGreen;
                }
            }
            else
            {
                itemPositions[currentCalibration] = e.position;
                items[currentCalibration].Background = Brushes.Transparent;
                currentCalibration++;
                if (currentCalibration < numItems)
                    items[currentCalibration].Background = Brushes.Orange;
                else
                {
                    calibrated = true;
                    calibrationLabel.Visibility = System.Windows.Visibility.Hidden;
                    KinectAccessor.controller.SpineTracked += this.SpineTracked;
                }
            }
        }

        private void SpineTracked(object sender, KinectJointTrackedArgs e)
        {
            int nearestId = -1;
            double minDist = Double.MaxValue;
            for (int i = 0; i < itemPositions.Length; i++)
            {
                double dist = DistanceBetween(e.position, itemPositions[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestId = i;
                }
            }

            if ((currentHover != -1) && (currentHover != nearestId) && (currentHover != currentSelection))
                items[currentHover].Background = Brushes.Transparent;

            currentHover = nearestId;

            if (nearestId != currentSelection)
                items[nearestId].Background = Brushes.Orange;
        }

        private double DistanceBetween(SkeletonPoint current, SkeletonPoint itemPosition)
        {
            float xDiff = current.X - itemPosition.X;
            float yDiff = current.Y - itemPosition.Y;
            float zDiff = current.Z - itemPosition.Z;
            return Math.Sqrt(Math.Pow(xDiff, 2) + Math.Pow(yDiff, 2) + Math.Pow(zDiff, 2));
        }
    }
}
