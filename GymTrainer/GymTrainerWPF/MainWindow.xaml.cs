using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Frame = Microsoft.Azure.Kinect.BodyTracking.Frame;

namespace GymTrainerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Azure Kinect sensor
        /// </summary>
        private readonly Device kinect = null;


        //private readonly Tracker tracker = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private readonly WriteableBitmap bitmap = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// The width in pixels of the color image from the Azure Kinect DK
        /// </summary>
        private readonly int colorWidth = 0;

        /// <summary>
        /// The height in pixels of the color image from the Azure Kinect DK
        /// </summary>
        private readonly int colorHeight = 0;

        private List<List<Vector3>> jointsList = new List<List<Vector3>>();

        private object lockObject = new object();
        /// <summary>
        /// Status of the application
        /// </summary>

        private bool running = true;
        public MainWindow()
        {
            // Open the default device
            this.kinect = Device.Open();

            // Configure camera modes
            this.kinect.StartCameras(new DeviceConfiguration
            {
                CameraFPS = FPS.FPS15,
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R1080p,
                DepthMode = DepthMode.NFOV_Unbinned,
                SynchronizedImagesOnly = true
            });

            this.colorWidth = this.kinect.GetCalibration().ColorCameraCalibration.ResolutionWidth;
            this.colorHeight = this.kinect.GetCalibration().ColorCameraCalibration.ResolutionHeight;

            this.bitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgra32, null);

            this.DataContext = this;

            InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.bitmap;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            running = false;

            lock (lockObject) 
            {
                string fileName = DateTime.Now.ToString("dd_MM_HH_mm_ss") + ".csv";
                SkeletonCSVExport skeletonCSVExport = new SkeletonCSVExport();
                skeletonCSVExport.Write(fileName, jointsList);
            }

            if (this.kinect != null)
            {
                this.kinect.Dispose();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Tracker tracker = Tracker.Create(this.kinect.GetCalibration(), new TrackerConfiguration() { ProcessingMode = TrackerProcessingMode.Gpu, SensorOrientation = SensorOrientation.Default });

            while (running)
            {
                using (Capture capture = await Task.Run(() => { return this.kinect.GetCapture(); }))
                {
                    this.StatusText = "Received Capture: " + capture.Depth.DeviceTimestamp;

                    this.bitmap.Lock();

                    var color = capture.Color;
                    var region = new Int32Rect(0, 0, color.WidthPixels, color.HeightPixels);

                    unsafe
                    {
                        using (var pin = color.Memory.Pin())
                        {
                            this.bitmap.WritePixels(region, (IntPtr)pin.Pointer, (int)color.Size, color.StrideBytes);
                        }
                    }

                    this.bitmap.AddDirtyRect(region);
                    this.bitmap.Unlock();

                    tracker.EnqueueCapture(capture);
                }

                // Try getting latest tracker frame.
                using (Frame lastFrame = tracker.PopResult(TimeSpan.Zero, throwOnTimeout: false))
                {
                    if (lastFrame == null)
                        continue;
                    lock (lockObject) 
                    {
                        List<Vector3> joints = new List<Vector3>();
                        for (uint i = 0; i < lastFrame.NumberOfBodies; ++i)
                        {
                            Skeleton skeleton = lastFrame.GetBodySkeleton(i);
                            var bodyId = lastFrame.GetBodyId(i);
                            for (int jointId = 0; jointId < (int)JointId.Count; ++jointId)
                            {
                                var joint = skeleton.GetJoint(jointId);
                                joints.Add(joint.Position / 1000);
                            }
                        }
                        jointsList.Add(joints);
                    }     
                }
            }
        }

    }
}
