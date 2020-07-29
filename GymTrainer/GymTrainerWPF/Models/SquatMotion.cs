using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GymTrainerWPF.Models
{
    public class SquatMotion : IExerciseMotion
    {
        //0 JointId.KneeRight, 1 JointId.KneeLeft, 2 JointId.HipRight, 3 JointId.HipLeft, 4 JointId.AnkleRight, 5 JointId.AnkleLeft, 6 JointId.ShoulderLeft, 7 JointId.ShoulderRight
        List<JointId> keyJoints = new List<JointId> { JointId.KneeRight, JointId.KneeLeft, JointId.HipRight, JointId.HipLeft, JointId.AnkleRight, JointId.AnkleLeft, JointId.ShoulderLeft, JointId.ShoulderRight };
        List<Vector3> jointsPos = new List<Vector3>();
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
            
            foreach (var jointId in keyJoints)
            {
                var joint = skeleton.GetJoint(jointId);
                jointsPos.Add(joint.Position / 1000);
            }
            var shoulderLeftRightDistance = Vector3.Distance(jointsPos[6], jointsPos[7]);
            var delta = shoulderLeftRightDistance * .10;
            var ankleLeftRightDistance = Vector3.Distance(jointsPos[4], jointsPos[5]);

            var heuristic_1 = shoulderLeftRightDistance - delta; // min heuristic value that is allowed
            var heuristic_2 = shoulderLeftRightDistance + delta;

            if (ankleLeftRightDistance < heuristic_1)
            {
                return "Please widen your stance";
            }
            else if (ankleLeftRightDistance > heuristic_2)
            {
                return "Please narrow your stance";
            }
            return "success";
        }

        public AnalysisResult analyze(Skeleton skeleton)
        {
            AnalysisResult analysisResult = new AnalysisResult() { WarningMessage = "success", Status = RapStatus.NotReady };
            // For a complete rep, the knee-hip and ankle-knee angle should start close to 180 deg and reach to about 90+-10 degrees
            // calculate the dot product of knee-hip and ankle-knee 1-2
            Vector3 rightKneeHip = Vector3.Subtract(jointsPos[0], jointsPos[2]);
            Vector3 leftKneeHip = Vector3.Subtract(jointsPos[1], jointsPos[3]);
            Vector3 rightAnkleKnee = Vector3.Subtract(jointsPos[4], jointsPos[0]);
            Vector3 leftAnkleKnee = Vector3.Subtract(jointsPos[5], jointsPos[1]);
            float rightAngle = Angle(rightKneeHip, rightAnkleKnee);
            float leftAngle = Angle(leftKneeHip, leftAnkleKnee);

            if (leftAngle < 80 || rightAngle < 80)
            {
                analysisResult.WarningMessage =  "Too Shallow";
            }
            else if (leftAngle > 100 || rightAngle > 100)
            {
                analysisResult.WarningMessage = "Too Deep";
            }
            return analysisResult;
        }

        public const float kEpsilonNormalSqrt = 1e-15F;
        // The infamous ''3.14159265358979...'' value (RO).
        public const float PI = (float)Math.PI;
        // Degrees-to-radians conversion constant (RO).
        public const float Deg2Rad = PI * 2F / 360F;

        // Radians-to-degrees conversion constant (RO).
        public const float Rad2Deg = 1F / Deg2Rad;

        public static float Angle(Vector3 from, Vector3 to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = (float)Math.Sqrt(Vector3.Dot(from, from) * Vector3.Dot(to, to));
            if (denominator < kEpsilonNormalSqrt)
                return 0F;

            double dot = Math.Clamp((double)Vector3.Dot(from, to) / denominator, -1.0, 1.0);
            return ((float)Math.Acos(dot)) * Rad2Deg;
        }
    }
}
