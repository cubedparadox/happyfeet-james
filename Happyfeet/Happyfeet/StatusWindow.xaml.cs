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

        private List<int> reportedSkeletons;
        private DispatcherTimer stampLabelTimer;
        private MainWindow mainWindow;

        public StatusWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            KinectAccessor.Initialize();

            mainWindow = new MainWindow();

            reportedSkeletons = new List<int>();

            stampLabelTimer = new DispatcherTimer();
            stampLabelTimer.Interval = new TimeSpan(0, 0, 0, 0, stampLabelTimeout);
            stampLabelTimer.Tick += ClearStampLabel;

            KinectAccessor.controller.Initializing += this.KinectInitializing;
            KinectAccessor.controller.NotPowered += this.KinectNotPowered;
            KinectAccessor.controller.Error += this.KinectError;
            KinectAccessor.controller.Ready += this.KinectReady;
            KinectAccessor.controller.Disconnected += this.KinectDisconnected;
            KinectAccessor.controller.StreamEnabled += this.KinectStreamEnabled;
            KinectAccessor.controller.StreamDisabled += this.KinectStreamDisabled;
            KinectAccessor.controller.SkeletonTracked += this.KinectSkeletonTracked;
            KinectAccessor.controller.LeftFootTracked += this.KinectLeftFootTracked;
            KinectAccessor.controller.LeftAnkleTracked += this.KinectLeftAnkleTracked;
            KinectAccessor.controller.RightFootTracked += this.KinectRightFootTracked;
            KinectAccessor.controller.RightAnkleTracked += this.KinectRightAnkleTracked;
            KinectAccessor.controller.LeftKneeTracked += this.KinectLeftKneeTracked;
            KinectAccessor.controller.RightKneeTracked += this.KinectRightKneeTracked;
            KinectAccessor.controller.SpineTracked += this.KinectSpineTracked;

            KinectAccessor.gestureRecognizer.StampDetected += this.StampDetected;

            KinectAccessor.controller.KinectStart();
        }

        private void KinectInitializing(object sender, KinectStatusArgs e)
        {
            this.kinectStatusBox.Text += "Kinect " + e.kinectID + " is initializing...\n";
        }

        private void KinectNotPowered(object sender, KinectStatusArgs e)
        {
            mainWindow.Close();
            this.kinectStatusBox.Text += "Connect Kinect " + e.kinectID + " to a power source...\n";
        }

        private void KinectError(object sender, KinectErrorArgs e)
        {
            mainWindow.Close();
            this.kinectStatusBox.Text += "Kinect " + e.kinectID + " error: " + e.status + "\n";
        }

        private void KinectReady(object sender, KinectStatusArgs e)
        {
            this.kinectStatusBox.Text += "Kinect " + e.kinectID + " is ready!\n";
            mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void KinectDisconnected(object sender, KinectStatusArgs e)
        {
            mainWindow.Close();
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
