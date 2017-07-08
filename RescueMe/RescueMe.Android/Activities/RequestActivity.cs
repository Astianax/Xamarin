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
using RescueMe.Droid.Adapters;
using RescueMe.Droid.Data;
using RescueMe.Domain;
using Android.Locations;
using System.Threading;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "Request")]
    public class RequestActivity : BaseActivity
    {
        //Controls
        private Spinner _spReasons;
        private Spinner _spVehicles;
        private EditText _comment;
        //Lists
        private List<Vehicle> _vehicles;
        private List<ReasonRequest> _reasons;

        //Information
        private Double _latitude;
        private Double _longitude;
        protected override void OnCreate(Bundle savedInstanceState)
        {
           
            base.OnCreate(savedInstanceState);

            // Create your application here
            //Get Location information
            var location = Intent.GetBundleExtra("location");
            _latitude = location.GetDouble("Latitude");
            _longitude = location.GetDouble("Longitude");
            
            var user = _context.GetUser();
            _vehicles = _context.GetVehicles();
            _reasons = _context.GetReasons();
            if (String.IsNullOrEmpty(user.IdentificationCard) || String.IsNullOrEmpty(user.TelephoneNumber))
            {
                // If User don't have user information
                StartActivity(typeof(ProfileActivity));
            }
            else if (_vehicles.Count == 0)
            {
                //Show Activity for Vehicles
                StartActivity(typeof(CarsActivity));
            }
            else
            {
                //Load Activity
                SetContentView(Resource.Layout.RequestRescue);
                _spReasons =
                       FindViewById<Spinner>(Resource.Id.ddReasons);
                _spVehicles =
                        FindViewById<Spinner>(Resource.Id.ddVehicles);
                _comment = FindViewById<EditText>(Resource.Id.txtComment);
                var btnRequestRescue = FindViewById<Button>(Resource.Id.RequestRescue);
            
                SetTools();

                //spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
                //Adapter's 
                var vehicleList = _vehicles.Select(v => new SpinnerItem
                {
                    Description = v.Marque + "," + v.Type,
                    Id = v.Id
                }).ToArray();
                var reasonsList = _reasons.Select(v => new SpinnerItem
                {
                    Description =v.Name,
                    Id = v.Id
                }).ToArray();
                //var reasons = reas
               
                var reasonsAdapter = new SpinnerAdapter(this, Resource.String.select_spinner, reasonsList);
                var vehicleAdapter = new SpinnerAdapter(this, Resource.String.select_vehicle, vehicleList);
                //adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                _spVehicles.Adapter = vehicleAdapter;
                _spReasons.Adapter = reasonsAdapter;
                _comment.Text = GetAddress(_latitude, _longitude);

                //Set Event's
                btnRequestRescue.Click += BtnRequestRescue_Click;       
            }

        }

        private void BtnRequestRescue_Click(object sender, EventArgs e)
        {
            var request = new Request();
            var vehicle = _vehicles[_spVehicles.SelectedItemPosition];
            var reason = _reasons[_spReasons.SelectedItemPosition];

            request.Latitude = decimal.Parse(_latitude.ToString());
            request.Longitude = decimal.Parse(_longitude.ToString());
            request.UserID = _context.GetUser().UserID;
            request.ReasonID = reason.Id;
            request.VehicleID = vehicle.Id;
            request.Comments = _comment.Text;

            var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "Validando Información...");
            progressDialog.Indeterminate = true;
            progressDialog.SetCancelable(false);

            new Thread(new ThreadStart(delegate
            {
                //LOAD METHOD TO GET ACCOUNT INFO
                try
                {
                    request = _client.Post("Request/create", request).Result.JsonToObject<Request>();

                }
                catch (Exception ex)
                {
                    request = null;
                    // request = ex.Message;
                }

                if (request != null)
                {

                }
                else
                {
                    //Snackbar.Make(marqueLayout, message, Snackbar.LengthLong)
                    //        .SetAction("OK", (v) =>
                    //        {
                    //            txType.Text = String.Empty;
                    //            txtMarque.Text = String.Empty;
                    //        })
                    //        .SetDuration(8000)
                    //        .SetActionTextColor(Android.Graphics.Color.Orange)
                    //        .Show();
                }
                //HIDE PROGRESS DIALOG
                RunOnUiThread(() =>
                {
                    progressDialog.Hide();

                });

            })).Start();
        }

        public void SendRequest_Click(object sender, EventArgs e)
        {
            var request = new Request();
            var vehicle = _vehicles[_spVehicles.SelectedItemPosition];
            var reason = _reasons[_spReasons.SelectedItemPosition];

            request.Latitude = decimal.Parse(_latitude.ToString());
            request.Longitude = decimal.Parse(_longitude.ToString());
            request.UserID = _context.GetUser().UserID;
            request.ReasonID = reason.Id;
            request.VehicleID = vehicle.Id;
            request.Comments = _comment.Text;

            var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "Validando Información...");
            progressDialog.Indeterminate = true;
            progressDialog.SetCancelable(false);
          
            new Thread(new ThreadStart(delegate
            {
                //LOAD METHOD TO GET ACCOUNT INFO
                try
                {
                    request = _client.Post("Request/create", request).Result.JsonToObject<Request>();

                }
                catch (Exception ex)
                {
                    request = null;
                   // request = ex.Message;
                }

                if (request != null)
                {
                 
                }
                else
                {
                    //Snackbar.Make(marqueLayout, message, Snackbar.LengthLong)
                    //        .SetAction("OK", (v) =>
                    //        {
                    //            txType.Text = String.Empty;
                    //            txtMarque.Text = String.Empty;
                    //        })
                    //        .SetDuration(8000)
                    //        .SetActionTextColor(Android.Graphics.Color.Orange)
                    //        .Show();
                }
                //HIDE PROGRESS DIALOG
                RunOnUiThread(() =>
                {
                    progressDialog.Hide();

                });

            })).Start();

        }

        private string GetAddress(Double latitude, Double longitude)
        {
            var geocoder = new Geocoder(this);
            IList<Address> addressList =
                geocoder.GetFromLocation(latitude, longitude, 10);
            Address addressCurrent = addressList.FirstOrDefault();
            string mAddress;
            if (addressCurrent != null)
            {
                StringBuilder deviceAddress = new StringBuilder();

                for (int i = 0; i < addressCurrent.MaxAddressLineIndex; i++)
                    deviceAddress.Append(addressCurrent.GetAddressLine(i))
                        .AppendLine(",");

                mAddress = deviceAddress.ToString();
            }
            else
            {

                mAddress = "Unable to determine the address.";
            }
            return mAddress;
        }
    }
}