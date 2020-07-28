using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace GymTrainerWPF.Models
{
    public class Configuration
    {
		public static List<Exercise> LoadExercise()
		{
			//string exerciseConfigFile = "Exercise.json";
			var exercises = new List<Exercise>();

			//using (StreamReader reader = new StreamReader(exerciseConfigFile))
			//{
			//	string filedata = reader.ReadToEnd();
			//	exercises = JsonConvert.DeserializeObject<List<Exercise>>(filedata);
			//}

			exercises.Add(new Exercise() {ExerciseName = "Bicep Curl", ExerciseImageSource = new BitmapImage(new Uri(@"Images\bicepcurl.jpg", UriKind.Relative)) });
			exercises.Add(new Exercise() { ExerciseName = "Squat", ExerciseImageSource = new BitmapImage(new Uri(@"Images\squat.jpg", UriKind.Relative)) });

			return exercises;
		}
	}
}
