using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;

namespace GymTrainerWPF.Models
{
    public class BicepCurlMotion : IExerciseMotion
    {
        //0 Pelvis,1 Neck,2 ShoulderRight,3 ElbowRight,4 WristRight,5 HipLeft,6 HipRight
        List<int> keyJoints = new List<int> {0, 3, 12, 13, 14, 18, 22};

        Queue<float> JointsQueue = new Queue<float>();
        int status = - 1;//-1 reset, 0 up, 1 down

        public string IsPrepared(Skeleton skeleton)
        {
            JointsQueue.Clear();
            status = -1;
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



        public AnalysisResult analyze(Skeleton skeleton) 
        {
            AnalysisResult analysisResult = new AnalysisResult() {WarningMessage = "success", Status= RapStatus.NotReady };
            List<Vector3> jointsPos = new List<Vector3>();
            foreach (var jointId in keyJoints)
            {
                var joint = skeleton.GetJoint(jointId);
                jointsPos.Add(joint.Position / 1000);
            }

            float neck2hipleft = Vector3.Distance(jointsPos[0], jointsPos[5]);
            float neck2hipright = Vector3.Distance(jointsPos[0], jointsPos[6]);
            float torso = (neck2hipleft + neck2hipright) / (float)2.0;

            //Norm
            Vector3 shoulder2Elbow = Vector3.Subtract(jointsPos[3], jointsPos[2]);
            Vector3 neck2Pelvis = Vector3.Subtract(jointsPos[0], jointsPos[1]);
            Vector3 elbow2Wrist = Vector3.Subtract(jointsPos[3], jointsPos[4]);

            float angleSN = Angle(shoulder2Elbow, neck2Pelvis);

            if (angleSN > 20)
                analysisResult.WarningMessage = "Please ensure your elbows remain stationary and to the sides of your body";

            float angleEW = Angle(elbow2Wrist, shoulder2Elbow);
            JointsQueue.Enqueue(angleEW);
            if (JointsQueue.Count > 5)
                JointsQueue.Dequeue();

            if (angleEW < 80 && angleEW < JointsQueue.Average() && status != 0)
            {
                status = 0;
            }

            if (angleEW < 80 && angleEW > JointsQueue.Average() && status != 1) 
            {
                status = 1;
                analysisResult.Status = RapStatus.Ready;
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

        // Returns the angle in degrees between /from/ and /to/. This is always the smallest
        public static float Angle(Vector3 from, Vector3 to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = (float)Math.Sqrt(Vector3.Dot(from,from) * Vector3.Dot(to,to));
            if (denominator < kEpsilonNormalSqrt)
                return 0F;

            double dot = Math.Clamp((double)Vector3.Dot(from, to) / denominator, -1.0, 1.0);
            return ((float)Math.Acos(dot)) * Rad2Deg;
        }
    }
}
