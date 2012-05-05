using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Happyfeet
{
    public static class KinectAccessor
    {
        public static KinectController controller;
        public static KinectGestureRecognizer gestureRecognizer;

        public static void Initialize()
        {
            controller = new KinectController();
            gestureRecognizer = new KinectGestureRecognizer(controller);
        }
    }
}
