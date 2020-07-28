using System;
using System.Windows.Media.Imaging;

namespace GymTrainerWPF.Models
{
    [Serializable()]
    public class Exercise
    {
        public string ExerciseName { get; set; }

        public BitmapImage ExerciseImageSource { get; set; }
    }
}
