using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Happyfeet
{
    /// <summary>
    /// Interaction logic for StatusWindow.xaml
    /// </summary>
    public partial class StatusWindow : Window
    {
        private const Int32 stampLabelTimeout = 3000;

        private KinectController kinectController;
        private KinectGestureRecognizer kinectGestureRecognizer;
        private List<int> reportedSkeletons;
        private DispatcherTimer stampLabelTimer;

        public StatusWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            reportedSkeletons = new List<int>();

            stampLabelTimer = new DispatcherTimer();
            stampLabelTimer.Interval = new TimeSpan(0, 0, 0, 0, stampLabelTimeout);
            stampLabelTimer.Tick += ClearStampLabel;

            kinectController = new KinectController();

            kinectController.Initializing += this.KinectInitializing;
            kinectController.NotPowered += this.KinectNotPowered;
            kinectController.Error += this.KinectError;
            kinectController.Ready += this.KinectReady;
            kinectController.Disconnected += this.KinectDisconnected;
            kinectController.StreamEnabled += this.KinectStreamEnabled;
            kinectController.StreamDisabled += this.KinectStreamDisabled;
            kinectController.SkeletonTracked += this.KinectSkeletonTracked;
            kinectController.LeftFootTracked += this.KinectLeftFootTracked;
            kinectController.LeftAnkleTracked += this.KinectLeftAnkleTracked;
            kinectController.RightFootTracked += this.KinectRightFootTracked;
            kinectController.RightAnkleTracked += this.KinectRightAnkleTracked;
            kinectController.LeftKneeTracked += this.KinectLeftKneeTracked;
            kinectController.RightKneeTracked += this.KinectRightKneeTracked;
            kinectController.SpineTracked += this.KinectSpineTracked;

            kinectGestureRecognizer = new KinectGestureRecognizer(kinectController);

            kinectGestureRecognizer.StampDetected += this.StampDetected;

            kinectController.KinectStart();
        }

        private void KinectInitializing(object sender, KinectStatusArgs e)
        {
            this.kinectStatusBox.Text += "Kinect " + e.kinectID + " is initializing...\n";
        }

        private void KinectNotPowered(object sender, KinectStatusArgs e)
        {
            this.kinectStatusBox.Text += "Connect Kinect " + e.kinectID + " to a power source...\n";
        }

        private void KinectError(object sender, KinectErrorArgs e)
        {
            this.kinectStatusBox.Text += "Kinect " + e.kinectID + " error: " + e.status + "\n";
        }

        private void KinectReady(object sender, KinectStatusArgs e)
        {
            this.kinectStatusBox.Text += "Kinect " + e.kinectID + " is ready!\n";
        }

        private void KinectDisconnected(object sender, KinectStatusArgs e)
        {
            this.kinectStatusBox.Text += "Kinect " + e.kinectID + " has been disconnected...\n";
        }

        private void KinectStreamEnabled(object sender, KinectStatusArgs e)
        {
            this.kinectStatusBox.Text += "Kinect " + e.kinectID + ": Stream enabled...\n";
        }

        private void KinectStreamDisabled(object sender, KinectStatusArgs e)
        {
            this.kinectStatusBox.Text += "Kinect " + e.kinectID + ": Stream disabled...\n";
        }

        private void KinectSkeletonTracked(object sender, KinectSkeletonTrackedArgs e)
        {
            if (!reportedSkeletons.Contains(e.skeleton.TrackingId))
            {
                reportedSkeletons.Add(e.skeleton.TrackingId);
                this.kinectSkeletonBox.Text += "Skeleton " + e.skeleton.TrackingId + "\n";
            }
        }

        private void KinectLeftFootTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectLeftFootBox.Content = "(" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")";
        }

        private void KinectRightFootTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectRightFootBox.Content = "(" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")";
        }

        private void KinectLeftAnkleTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectLeftAnkleBox.Content = "(" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")";
        }

        private void KinectRightAnkleTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectRightAnkleBox.Content = "(" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")";
        }

        private void KinectLeftKneeTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectLeftKneeBox.Content = "(" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")";
        }

        private void KinectRightKneeTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectRightKneeBox.Content = "(" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")";
        }

        private void KinectSpineTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectSpineBox.Content = "(" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")";
        }

        private void StampDetected(object sender, KinectStampDetectedArgs e)
        {
            kinectStampLabel.Content = "Stamp detected at (" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")";
            kinectStampLabel.Visibility = System.Windows.Visibility.Visible;
            stampLabelTimer.Start();
        }

        private void kinectStatusBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            kinectStatusBox.ScrollToEnd();
        }

        private void kinectSkeletonBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            kinectSkeletonBox.ScrollToEnd();
        }

        private void ClearStampLabel(object sender, EventArgs e)
        {
            kinectStampLabel.Visibility = System.Windows.Visibility.Hidden;
            stampLabelTimer.Stop();
        }
    }
}
