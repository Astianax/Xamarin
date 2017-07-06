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

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "Request")]
    public class RequestActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            //Get Location information
            var latitud = savedInstanceState.GetBundle("Latitud");
            var longitude = savedInstanceState.GetBundle("Longitude");

            var user = _context.GetUser();
            var vehicles = _context.GetVehicles();
            if (user.IdentificationCard == "" || user.TelephoneNumber == "")
            {
                // If User don't have user information
                StartActivity(typeof(ProfileActivity));
            }
            else if (vehicles.Count == 0)
            {
                //Show Activity for Vehicles
                StartActivity(typeof(CarsActivity));
            }
            else
            {

            }

        }
    }
}