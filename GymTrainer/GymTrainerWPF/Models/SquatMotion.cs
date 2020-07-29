using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Collections.Generic;
using System.IO;

namespace GymTrainerWPF.Models
{
    public class SquatMotion : IExerciseMotion
    {
        List<JointId> keyJoints = new List<JointId> { JointId.KneeRight, JointId.KneeLeft, JointId.HipRight, JointId.HipLeft, JointId.AnkleRight, JointId.AnkleLeft, JointId.ShoulderLeft, JointId.ShoulderRight };
        public string IsPrepared(Skeleton skeleton)
        {
            foreach (var jointId in keyJoints)
            {
                var joint = skeleton.GetJoint(jointId);
                if (joint.ConfidenceLevel < JointConfidenceLevel.Medium)
                {
                    return $"{Enum.GetName(typeof(JointId), jointId)} cannot be captured";
                }
            }
            return "success";
        }

        public string analyze(Skeleton skeleton)
        {
            // Read from file
            using (var reader = new StreamReader(@"C:\Users\chhsiao\source\repos\Azure-kinect-GymTrainer\Data Analysis\Data\bodyweight_squat_side_view.csv"))
            {
                List<string> listA = new List<string>();
                List<string> listB = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    listA.Add(values[0]);
                    listB.Add(values[1]);
                }
            }

            // Extract out the body points we need
            return "success";
        }
    }
}
