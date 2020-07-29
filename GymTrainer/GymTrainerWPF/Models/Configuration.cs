using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;

namespace GymTrainerWPF.Models
{
    public class Configuration
    {
		private static BitmapImage LoadBitmapImage(string path) 
		{
			var bi = new BitmapImage();
			bi.BeginInit();
			bi.CacheOption = BitmapCacheOption.OnLoad;
			bi.UriSource = new Uri(path, UriKind.Relative);
			bi.EndInit();
			bi.Freeze();
			return bi;
		}

		public static List<Exercise> LoadExercise()
		{
			//string exerciseConfigFile = "Exercise.json";
			var exercises = new List<Exercise>();

			//using (StreamReader reader = new StreamReader(exerciseConfigFile))
			//{
			//	string filedata = reader.ReadToEnd();
			//	exercises = JsonConvert.DeserializeObject<List<Exercise>>(filedata);
			//}

			exercises.Add(new Exercise() {ExerciseName = "Bicep Curl", ExerciseImageSource =LoadBitmapImage(@"Images\bicepcurl.jpg") });
			exercises.Add(new Exercise() { ExerciseName = "Squat", ExerciseImageSource = LoadBitmapImage(@"Images\squat.jpg") });
			exercises.Add(new Exercise() { ExerciseName = "Plank", ExerciseImageSource = LoadBitmapImage(@"Images\plank.jpg") });
			exercises.Add(new Exercise() { ExerciseName = "Deadlift", ExerciseImageSource = LoadBitmapImage(@"Images\deadlift.jpg") });
			exercises.Add(new Exercise() { ExerciseName = "Crunch", ExerciseImageSource = LoadBitmapImage(@"Images\crunch.jpg") });
			exercises.Add(new Exercise() { ExerciseName = "Jumping Jack", ExerciseImageSource = LoadBitmapImage(@"Images\jumpjack.jpg") });
			exercises.Add(new Exercise() { ExerciseName = "Shoulder Press", ExerciseImageSource = LoadBitmapImage(@"Images\shoulderpress.jpg") });
			return exercises;
		}
	}
}
