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
using System.Diagnostics;
using Frame = Microsoft.Azure.Kinect.BodyTracking.Frame;
using GymTrainerWPF.Model;
using GymTrainerWPF.ViewModels;

namespace GymTrainerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing1;
        }

        private void MainWindow_Closing1(object sender, CancelEventArgs e)
        {
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as GymTrainerViewModel;
            viewModel.TurnOnMedia += StartMedia;

            VideoControl.ScrubbingEnabled = true;
            VideoControl.Play();
            VideoControl.Pause();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {

        }

        /// <summary>
        /// Occurs when property changes
        /// </summary>
        /// <param name="propertyname">Name of property name</param>
        private void OnPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        private void ExerciseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as GymTrainerViewModel;
            int index = this.ExerciseList.SelectedIndex;
            if (index < 0)
                return;
            viewModel.SelectExercise(this.ExerciseList.SelectedIndex);
        }

        private void VideoControl_MediaOpened(object sender, RoutedEventArgs e)
        {
            VideoControl.ScrubbingEnabled = true;
            VideoControl.Play();
            VideoControl.Pause();
        }

        private void VideoControl_MediaEnded(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as GymTrainerViewModel;
            if (viewModel.CurrentReps < viewModel.TotalReps)
            {
                VideoControl.Position = TimeSpan.Zero;
                VideoControl.Play();
                viewModel.CurrentReps++;
                return;
            }
            StopMedia();
        }

        private void MediaStartBtn_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as GymTrainerViewModel;
            if ((string)MediaStartBtn.Content == "Start")
            {
                if (viewModel.Running)
                {
                    viewModel.SystemMode = "Preparing";
                    return;
                }
                StartMedia();
            }
            else
            {
                StopMedia();
            }
        }
        private void StopMedia()
        {
            var viewModel = this.DataContext as GymTrainerViewModel;
            VideoControl.Stop();
            MediaStartBtn.Content = "Start";
            MediaPauseBtn.Content = "Pause";
            viewModel.CurrentReps = 0;
            viewModel.SystemMode = "Free";
        }

        private void StartMedia()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
            });
            var viewModel = this.DataContext as GymTrainerViewModel;
            VideoControl.Play();
            MediaStartBtn.Content = "Stop";
            viewModel.CurrentReps++;
        }

        private void MediaPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((string)MediaPauseBtn.Content == "Pause")
            {
                VideoControl.Pause();
                MediaPauseBtn.Content = "Resume";
            }
            else
            {
                VideoControl.Play();
                MediaPauseBtn.Content = "Pause";
            }
        }

        private async void CameraToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as GymTrainerViewModel;
            await viewModel.OpenCamera();
        }

        private async void CameraToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as GymTrainerViewModel;
            await viewModel.CloseCamera();
        }

        private void ReporteBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
