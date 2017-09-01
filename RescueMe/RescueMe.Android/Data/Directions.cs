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
using Android.Gms.Maps.Model;

namespace RescueMe.Droid.Data
{
    public class Directions
    {
        public List<LatLng> Points { get; set; }
        public string Distance { get; set; }
        public string Duration { get; set; }
    }
}