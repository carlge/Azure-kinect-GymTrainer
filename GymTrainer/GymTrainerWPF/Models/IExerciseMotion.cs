using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymTrainerWPF.Models
{
    public interface IExerciseMotion
    {
        //return success if prepared, otherwise return warning
        string IsPrepared(Skeleton skeleton);

        //return success if prepared, otherwise return warning
        AnalysisResult analyze(Skeleton skeleton);
    }

    public enum RapStatus
    {
        NotReady = 0,
        Ready = 1
    }

    public class AnalysisResult
    {
        public string WarningMessage { get; set; }

        public RapStatus Status { get; set; }
    }
}
