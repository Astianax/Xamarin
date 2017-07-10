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
using Android.Support.Design.Widget;
using System.Drawing;
using Android.Views.Animations;
using Android.Support.V4.Content;
using Android.Telephony;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "Request")]
    public class RequestActivity : BaseActivity
    {
        //Controls
        private Spinner _spReasons;
        private Spinner _spVehicles;
        private EditText _comment;
        private RelativeLayout requestLayout;
        //Lists
        private List<Vehicle> _vehicles;
        private List<ReasonRequest> _reasons;

        //Information
        private Double _latitude;
        private Double _longitude;


        Android.Support.V7.Widget.Toolbar reasonsLayout;
        private Button btnRequestRescue;
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

            SetContentView(Resource.Layout.RequestRescue);

            reasonsLayout = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.spinerReasonsLayout);
            requestLayout = FindViewById<RelativeLayout>(Resource.Id.layoutRequest);


            if (String.IsNullOrEmpty(user.IdentificationCard) || String.IsNullOrEmpty(user.TelephoneNumber))
            {
                // If User don't have user information
                Snackbar.Make(requestLayout, "Es necesario que tenga cedula y telefono registrado para la asistencia", Snackbar.LengthIndefinite)
                             .SetAction("OK", (v) =>
                             {
                                 StartActivity(typeof(ProfileActivity));
                             })
                             //.SetDuration(8000)
                             .SetActionTextColor(Android.Graphics.Color.Orange)
                             .Show();
             
            }
            else if (_vehicles.Count == 0)
            {
                //Show Activity for Vehicles
                Snackbar.Make(requestLayout, "Es necesario que tenga al menos un vehiculo registrado para la asistencia", Snackbar.LengthIndefinite)
                           .SetAction("OK", (v) =>
                           {
                               StartActivity(typeof(CarsActivity));
                           })
                           //.SetDuration(8000)
                           .SetActionTextColor(Android.Graphics.Color.Orange)
                           .Show();
              
            }
            else
            {
                //Load Activity
                
                _spReasons =
                       FindViewById<Spinner>(Resource.Id.ddReasons);
                _spVehicles =
                        FindViewById<Spinner>(Resource.Id.ddVehicles);
                _comment = FindViewById<EditText>(Resource.Id.txtComment);
                btnRequestRescue = FindViewById<Button>(Resource.Id.RequestRescue);

                SetTools();

                //spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
                //Adapter's 
                var vehicleList = _vehicles.Select(v => new SpinnerItem
                {
                    Description =$"{v.Marque}{(v.Type != null ? " ("+v.Type+")" : "")}",// v.Marque + (v.Type != "" ? ) "(" + v.Type+")",
                    Id = v.Id
                }).ToArray();
                var reasonsList = _reasons.Select(v => new SpinnerItem
                {
                    Description = v.Name,
                    Id = v.Id
                }).ToArray();
                //var reasons = reas

                var reasonsAdapter = new SpinnerAdapter(this, Resource.String.select_spinner, reasonsList);
                var vehicleAdapter = new SpinnerAdapter(this, Resource.String.select_vehicle, vehicleList);
                //adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                _spVehicles.Adapter = vehicleAdapter;
                _spReasons.Adapter = reasonsAdapter;


                if (IsNetworkConnected())
                {
                    _comment.Text = GetAddress(_latitude, _longitude);
                    _comment.SetSelection(_comment.Text.Length);
                }
                else
                {
                    _comment.Text = String.Empty;
                }




                //Set Event's
                btnRequestRescue.Click += BtnRequestRescue_Click;
                //Button fadein = FindViewById<Button>(Resource.Id.fadein);
                //fadein.Click += Btn_Click;




                //btnRequestRescue.Click += async (sender, eventargs) =>
                //{

                //    Animation shake = AnimationUtils.LoadAnimation(this, Resource.Animation.outToBottom);
                //    relativeLayout.StartAnimation(shake);
                //    shake.FillAfter = true;
                //    shake.FillEnabled = true;

                //    shake.AnimationEnd += delegate
                //    {
                //        StartActivity(typeof(editProfileActivity));
                //        OverridePendingTransition(Resource.Animation.inFromRight, Resource.Animation.outToLeft);
                //    };
                //};


            }
        }
        //public void Btn_Click(object sender, EventArgs e)
        //{
        //    var txtMessage = FindViewById<TextView>(Resource.Id.txtMessage);
        //    Button b = sender as Button;
        //    Animation anim = AnimationUtils.LoadAnimation(ApplicationContext,
        //                   Resource.Animation.fade_in);
        //    txtMessage.StartAnimation(anim);
        //}

        private void BtnRequestRescue_Click(object sender, EventArgs e)
        {
            var request = new Request();
            string message = "";
           
            var selectedVehicle = _spVehicles.SelectedItemPosition;
            var selectedReason = _spReasons.SelectedItemPosition;
            if (selectedReason == 0)
            {
                SetSpinnerError(_spReasons, "Debe seleccionar una razon");
            }
            else
            if (selectedVehicle == 0)
            {
                SetSpinnerError(_spVehicles, "Debe seleccionar un vehicle");
            }
            else
            {
                var vehicle = _vehicles[selectedVehicle];
                var reason = _reasons[selectedReason];

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
                    try
                    {
                        if (IsNetworkConnected())
                        {
                            request = _client.Post("Request/create", request).Result.JsonToObject<Request>();
                            message = "Se ha enviado su solicitud";
                            btnRequestRescue.Enabled = false;
                        }
                        else
                        {
                            //SMS
                            SmsManager sms = SmsManager.Default;
                            PendingIntent sentPI;
                            string requestData = request.Serialize();
                            sentPI = PendingIntent.GetBroadcast(this, 0, new Intent(requestData), 0);
                            sms.SendTextMessage("8296370019", null, requestData, sentPI, null);
                            btnRequestRescue.Enabled = false;

                        }
                    }
                    catch (Exception ex)
                    {
                        request = null;
                        message = ex.Message;
                    }
                    //HIDE PROGRESS DIALOG
                    RunOnUiThread(() =>
                        {
                            progressDialog.Hide();
                            if (request != null)
                            {
                                _context.InsertRequest(request);
                            }
                            Snackbar.Make(requestLayout, message, Snackbar.LengthIndefinite)
                                .SetAction("OK", (v) =>
                                {
                                    this.Finish();
                                })
                                //.SetDuration(8000)
                                .SetActionTextColor(Android.Graphics.Color.Orange)
                                .Show();
                        });
                }

                )).Start();
            }
        }

        private void SetSpinnerError(Spinner element, string message)
        {
            View view = element.SelectedView;
            //ImageView image = view.FindViewById<ImageView>(Resource.Id.setError);
            TextView error = view.FindViewById<TextView>(Resource.Id.txtSpinner);
            error.Text = message;
            //error.SetBackgroundColor(Android.Graphics.Color.Red);
            //error.
            error.SetTextColor(Android.Graphics.Color.Red);
            //error.SetTextColor(ContextCompat.GetColor(this, Resource.Color.my_purple));
            //error.SetError(message, Resources.GetDrawable(Resource.Drawable.ic_delete));
            error.RequestFocus();
            Animation shake = AnimationUtils.LoadAnimation(this, Resource.Animation.shake);
            element.StartAnimation(shake);

        }
        //Verificar con sin conexion a internet
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