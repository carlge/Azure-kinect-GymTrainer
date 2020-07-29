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
        /// Status of the application
        /// </summary>

        private bool running = false;

        public async Task OpenCamera()
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

            await StartAcquisition();
        }

        public void CloseCamera()
        {
            if (this._kinect != null)
            {
                this._kinect.Dispose();
            }
        }

        async Task StartAcquisition()
        {
            if (running)
                return;

            running = true;
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

                            // JpegBitmapEncoder to save BitmapSource to file
                            // imageSerial is the serial of the sequential image
                            //var bmpSource = BitmapSource.Create(color.WidthPixels, color.HeightPixels, 96.0, 96.0, PixelFormats.Bgr32, null, color.Memory.ToArray(), color.StrideBytes);
                            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                            //encoder.Frames.Add(BitmapFrame.Create(bmpSource));
                            //using (var fs = new FileStream("./img/" + (imageSerial++) + ".jpeg", FileMode.Create, FileAccess.Write))
                            //{
                            //    encoder.Save(fs);
                            //}

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
