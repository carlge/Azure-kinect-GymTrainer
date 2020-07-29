using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymTrainerWPF.Models
{
    public class BicepCurlMotion : IExerciseMotion
    {
         List<int> keyJoints = new List<int> {0, 3, 12, 13, 14, 18, 22};

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

            return "success";
        }
    }
}
