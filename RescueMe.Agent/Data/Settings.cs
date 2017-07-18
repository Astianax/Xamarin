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
using SQLite;
using SQLite.Net.Attributes;

namespace RescueMe.Agent.Data
{
    public class Settings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public bool LocationPermission { get; set; }
        public bool AgentaAvailability { get; set; }
    }
}