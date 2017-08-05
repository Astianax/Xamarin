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
    public class AgentLocation
    {
        public int AgentId { get; set; }
        public LatLng Location { get; set; }
    }
}