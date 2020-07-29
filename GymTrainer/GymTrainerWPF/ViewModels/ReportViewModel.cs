using GymTrainerWPF.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymTrainerWPF.ViewModels
{
    public class ReportViewModel
    {
        public List<Person> Data { get; set; }

        public ReportViewModel()
        {
            Data = new List<Person>()
            {
                new Person { Name = "David", Height = 180 },
                new Person { Name = "Michael", Height = 170 },
                new Person { Name = "Steve", Height = 160 },
                new Person { Name = "Joel", Height = 182 }
            };
        }
    }
}
