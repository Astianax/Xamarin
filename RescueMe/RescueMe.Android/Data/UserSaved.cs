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
    //SqLite Database
    public class UserSaved
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string IdentificationCard { get; set; }
        public string TelephoneNumber { get; set; }
        public string City { get; set; }
        public int UserID { get; set; }
    
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string PassworDigest { get; set; }
        public DateTime LastLogged { get; set; }

    }
    
}