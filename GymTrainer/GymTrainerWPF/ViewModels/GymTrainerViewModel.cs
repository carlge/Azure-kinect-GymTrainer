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

        private string _workoutTime = "Workout Time : 00:00:00";

        private string _repsCount = "Reps : 00 / 15";

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap _bitmap = null;

        private int imageSerial = 0;

        private List<List<Vector3>> jointsList = new List<List<Vector3>>();

        private object lockObject = new object();
        

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

        public string WorkOutTime
        {
            get
            {
                return this._workoutTime;
            }

            set
            {
                if (this._workoutTime != value)
                {
                    this._workoutTime = value;
                    OnPropertyChanged("WorkOutTime");
                }
            }
        }

        public string RepsCount
        {
            get
            {
                return this._repsCount;
            }

            set
            {
                if (this._repsCount != value)
                {
                    this._repsCount = value;
                    OnPropertyChanged("RepsCount");
                }
            }
        }



        public string VideoSource
        {
            get; set;
        } = "Videos/bicepcurl.mp4";

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
    }
}
