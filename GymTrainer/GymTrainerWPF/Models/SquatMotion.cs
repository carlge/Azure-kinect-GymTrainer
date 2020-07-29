using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Collections.Generic;
using System.Numerics;

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
            List<Vector3> jointsPos = new List<Vector3>();
            foreach (var jointId in keyJoints)
            {
                var joint = skeleton.GetJoint(jointId);
                jointsPos.Add(joint.Position / 1000);
            }

            var shoulderLeftRightDistance = Vector3.Distance(jointsPos[6], jointsPos[7]);
            // distance of shoulderLeftRightDistance
            // distance of ankleLeftRightDistance
            var delta = shoulderLeftRightDistance * .10;

           



            //Norm


            return "success";

        }
    }
}
