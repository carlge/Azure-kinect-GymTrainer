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
        string analyze(Skeleton skeleton);
    }
}
