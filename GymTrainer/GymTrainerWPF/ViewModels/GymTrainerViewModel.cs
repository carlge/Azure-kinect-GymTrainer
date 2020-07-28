using GymTrainerWPF.Model;
using GymTrainerWPF.Models;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GymTrainerWPF.ViewModels
{
    public partial class GymTrainerViewModel : ViewModelBase
    {
        /// <summary>
        /// Azure Kinect sensor
        /// </summary>
        private Device _kinect = null;

        /// <summary>
        /// The width in pixels of the color image from the Azure Kinect DK
        /// </summary>
        private int _colorWidth = 1920;

        /// <summary>
        /// The height in pixels of the color image from the Azure Kinect DK
        /// </summary>
        private int _colorHeight = 1080;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string _statusText = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap _bitmap = null;

        private int imageSerial = 0;

        private List<List<Vector3>> jointsList = new List<List<Vector3>>();

        private object lockObject = new object();
        /// <summary>
        /// Status of the application
        /// </summary>

        private bool running = true;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this._bitmap;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this._statusText;
            }

            set
            {
                if (this._statusText != value)
                {
                    this._statusText = value;
                    OnPropertyChanged("StatusText");
                }
            }
        }

        /// <summary>
        /// Gets/sets collection of cameras
        /// </summary>
        public ObservableCollection<Exercise> Exercises
        {
            get; set;
        } = new ObservableCollection<Exercise>();

        public GymTrainerViewModel() 
        {
            this._bitmap = new WriteableBitmap(_colorWidth, _colorHeight, 96.0, 96.0, PixelFormats.Bgra32, null);
            var exercises = Configuration.LoadExercise();
            foreach (var exercise in exercises)
            {
                Exercises.Add(exercise);
            }
        }

        private void OpenCamera() 
        {
            // Open the default device
            this._kinect = Device.Open();

            // Configure camera modes
            this._kinect.StartCameras(new DeviceConfiguration
            {
                CameraFPS = FPS.FPS15,
                ColorFormat = ImageFormat.ColorBGRA32,
                ColorResolution = ColorResolution.R1080p,
                DepthMode = DepthMode.NFOV_Unbinned,
                SynchronizedImagesOnly = true
            });
        }

        public void CloseCamera() 
        {
            if (this._kinect != null)
            {
                this._kinect.Dispose();
            }
        }

        private void InitVideoSaving() 
        {
            try
            {
                var path = "./img/";
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    Console.WriteLine("That path exists already.");
                    // Deleting all previous image in ./img directory
                    System.IO.DirectoryInfo directory = new DirectoryInfo("./img/");
                    foreach (FileInfo file in directory.GetFiles())
                    {
                        file.Delete();
                    }
                }
                else
                {
                    // Try to create the directory.
                    DirectoryInfo di = Directory.CreateDirectory(path);
                    Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(path));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }

        private void ExportData() 
        {
            lock (lockObject)
            {
                string fileName = DateTime.Now.ToString("dd_MM_HH_mm_ss");
                SkeletonCSVExport skeletonCSVExport = new SkeletonCSVExport();
                skeletonCSVExport.Write(fileName + ".csv", jointsList);
                Process.Start("ffmpeg.exe", "-framerate 10 -i ./img/%d.jpeg -c:v libx264 -r 30 -pix_fmt yuv420p " + fileName + ".mp4");
            }
        }


        public void OpenExerciseWindow(int exerciseIndex) 
        {
            
        }

        async Task StartAcquisition() 
        {
            Tracker tracker = Tracker.Create(this._kinect.GetCalibration(), new TrackerConfiguration() { ProcessingMode = TrackerProcessingMode.Gpu, SensorOrientation = SensorOrientation.Default });

            while (running)
            {
                using (Capture capture = await Task.Run(() => { return this._kinect.GetCapture(); }))
                {
                    this.StatusText = "Received Capture: " + capture.Depth.DeviceTimestamp;

                    this._bitmap.Lock();

                    var color = capture.Color;
                    var region = new Int32Rect(0, 0, color.WidthPixels, color.HeightPixels);



                    unsafe
                    {
                        using (var pin = color.Memory.Pin())
                        {
                            this._bitmap.WritePixels(region, (IntPtr)pin.Pointer, (int)color.Size, color.StrideBytes);
                            var bmpSource = BitmapSource.Create(color.WidthPixels, color.HeightPixels, 96.0, 96.0, PixelFormats.Bgr32, null, color.Memory.ToArray(), color.StrideBytes);

                            // JpegBitmapEncoder to save BitmapSource to file
                            // imageSerial is the serial of the sequential image
                            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bmpSource));
                            using (var fs = new FileStream("./img/" + (imageSerial++) + ".jpeg", FileMode.Create, FileAccess.Write))
                            {
                                encoder.Save(fs);
                            }

                        }
                    }

                    this._bitmap.AddDirtyRect(region);
                    this._bitmap.Unlock();

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
