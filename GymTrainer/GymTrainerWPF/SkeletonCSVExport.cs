using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace GymTrainerWPF
{
    public class SkeletonCSVExport
    {
		public void Write(string fileName, List<List<Vector3>> jointsList)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));
			if (jointsList == null)
				throw new ArgumentNullException(nameof(jointsList));

			FileInfo file = new FileInfo(fileName);

			if (!file.Directory.Exists)
			{
				file.Directory.Create();
			}
			System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
			System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
			InitCSVTable(sw);
			foreach (var joints in jointsList) 
			{
				if (joints.Count == 0)
					continue;
				String data = "";
				for (int i = 0; i < (int)JointId.Count; i++)
				{
					data += joints[i].X + " " + joints[i].Y + " " + joints[i].Z;
					data += ",";
				}
				sw.WriteLine(data.TrimEnd(','));
			}
			sw.Close();
			fs.Close();
		}

		private void InitCSVTable(StreamWriter streamWriter)
		{
			String data = "";
			for (int i = 0; i < (int)JointId.Count; i++)
			{
				data += Enum.GetName(typeof(JointId), i);
				data += ",";
			}
			streamWriter.WriteLine(data.TrimEnd(','));
		}
	}
}
