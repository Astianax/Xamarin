using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace RescueMe.Droid.Data
{
    public class RequestSaved
    {
        public string Comments { get; set; }
        public decimal Longitude { get; set; }
        public int StatusID { get; set; }
        public int ReasonID { get; set; }
        public int VehicleID { get; set; }
        public int Id { get; set; }
        public decimal Latitude { get; set; }
        public string Status { get; set; }
    }
}