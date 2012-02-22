using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Happyfeet
{
    /// <summary>
    /// Interaction logic for StatusWindow.xaml
    /// </summary>
    public partial class StatusWindow : Window
    {
        private KinectController kinectController;

        public StatusWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
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
            this.kinectStatusBox.Text += "Skeleton " + e.skeleton.TrackingId + ": (" + e.skeleton.Position.X + "," + e.skeleton.Position.Y + "," + e.skeleton.Position.Z + ")\n";
        }

        private void KinectLeftFootTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectStatusBox.Text += "Left foot: (" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")\n";
        }

        private void KinectRightFootTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectStatusBox.Text += "Right foot: (" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")\n";
        }

        private void KinectLeftAnkleTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectStatusBox.Text += "Left ankle: (" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")\n";
        }

        private void KinectRightAnkleTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectStatusBox.Text += "Right ankle: (" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")\n";
        }

        private void KinectLeftKneeTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectStatusBox.Text += "Left knee: (" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")\n";
        }

        private void KinectRightKneeTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectStatusBox.Text += "Right knee: (" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")\n";
        }

        private void KinectSpineTracked(object sender, KinectJointTrackedArgs e)
        {
            this.kinectStatusBox.Text += "Spine: (" + e.position.X + "," + e.position.Y + "," + e.position.Z + ")\n";
        }

        private void kinectStatusBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            kinectStatusBox.ScrollToEnd();
        }
    }
}
