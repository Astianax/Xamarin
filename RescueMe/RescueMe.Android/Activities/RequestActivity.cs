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
            var location = Intent.GetBundleExtra("location");
            var latitud = location.GetDouble("Latitude");
            var longitude = location.GetDouble("Longitude");
            
            var user = _context.GetUser();
            var vehicles = _context.GetVehicles();
            if (String.IsNullOrEmpty(user.IdentificationCard) || String.IsNullOrEmpty(user.TelephoneNumber))
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
                SetContentView(Resource.Layout.RequestRescue);

                Spinner spinner = FindViewById<Spinner>(Resource.Id.Reasons);

                //spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
                var adapter = ArrayAdapter.CreateFromResource(
                        this, Resource.Array.planets_array, Android.Resource.Layout.SimpleSpinnerItem);

                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinner.Adapter = adapter;
            }

        }

        public void SendRequest_Click(EventArgs e, object sender)
        {

        }
    }
}