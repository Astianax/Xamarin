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
using Clans.Fab;
using Android.Content.PM;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "Request",
          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
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


            _vehicles = new List<Vehicle>()
            {
                 new Vehicle()
                {
                    Id = 0,
                    Marque ="Seleccionar Vehículo"
                },
                new Vehicle()
                {
                    Id = 0,
                    Marque ="Vehículo Tercero"
                }
            };
            _vehicles.AddRange(_context.GetVehicles());
            _reasons = _context.GetReasons();
            //Adapter's 
            var vehicleList = _vehicles.Select(v => new SpinnerItem
            {
                Description = $"{v.Marque}{(v.Type != null ? " (" + v.Type + ")" : "")}",// v.Marque + (v.Type != "" ? ) "(" + v.Type+")",
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

            SetContentView(Resource.Layout.RequestRescue);

            reasonsLayout = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.spinerReasonsLayout);
            requestLayout = FindViewById<RelativeLayout>(Resource.Id.layoutRequest);

            //Load Activity

            _spReasons =
                   FindViewById<Spinner>(Resource.Id.ddReasons);
            _spVehicles =
                    FindViewById<Spinner>(Resource.Id.ddVehicles);
            _comment = FindViewById<EditText>(Resource.Id.txtComment);
            btnRequestRescue = FindViewById<Button>(Resource.Id.RequestRescue);

            SetTools();

        
            btnRequestRescue.Click += BtnRequestRescue_Click;
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
        }

        private void BtnRequestRescue_Click(object sender, EventArgs e)
        {
            var request = new Request();
            string message = "";
            var user = _context.GetUser();

            if (String.IsNullOrEmpty(user.IdentificationCard))// || String.IsNullOrEmpty(user.TelephoneNumber))
            {
                // If User don't have user information
                btnRequestRescue.Enabled = false;
                Snackbar.Make(requestLayout, "Es necesario que tenga cédula registrado para la asistencia", Snackbar.LengthIndefinite)
                             .SetAction("OK", (v) =>
                             {
                                 StartActivity(typeof(ProfileActivity));
                                 btnRequestRescue.Enabled = true;
                             })
                             //.SetDuration(8000)
                             .SetActionTextColor(Android.Graphics.Color.Orange)
                             .Show();

            }
            else
            {

                var selectedVehicle = _spVehicles.SelectedItemPosition;
                var selectedReason = _spReasons.SelectedItemPosition;
                if (selectedReason == 0)
                {
                    SetSpinnerError(_spReasons, "Debe seleccionar una razon");
                }
                else
                if (selectedVehicle == 0)
                {
                    SetSpinnerError(_spVehicles, "Debe seleccionar un vehículo");
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
                                if (request != null && request.Id != 0)
                                {
                                    if (request.Status.Name.ToLower() == "no disponible")
                                    {
                                        message = "Nuestros agentes estan ocupados, se le notificara cuando este disponible";
                                        message = "No hay agentes disponibles, espere una notificación";
                                    }
                                }
                                else 
                                {
                                    message = "Se ha enviado su solicitud";
                                }
                            }
                            else
                            {
                                //SMS
                                request.StatusID = _context.getStatusList().FirstOrDefault(s => s.Name.ToLower() == "pendiente").Id;
                                message = "No tiene internet, se envió su solicitud por SMS";
                                SmsManager sms = SmsManager.Default;
                                PendingIntent sentPI;

                                string requestData = request.Latitude + "|" + request.Longitude + "|" + request.UserID + "|" + request.ReasonID
                                + "|" + request.VehicleID + "|" + request.Comments;


                                sentPI = PendingIntent.GetBroadcast(this, 0, new Intent(requestData), 0);
                                sms.SendTextMessage("13345819944", null, requestData, sentPI, null);
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
                                    btnRequestRescue.Enabled = false;
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
            string mAddress;
            try
            {
                var geocoder = new Geocoder(this);
                IList<Address> addressList =
                    geocoder.GetFromLocation(latitude, longitude, 10);
                Address addressCurrent = addressList.FirstOrDefault();

                if (addressCurrent != null)
                {
                    StringBuilder deviceAddress = new StringBuilder();

                    //for (int i = 0; i < addressCurrent.MaxAddressLineIndex; i++)
                    //    deviceAddress.Append(addressCurrent.GetAddressLine(i))
                    //        .AppendLine(",");
                    deviceAddress.Append(addressCurrent.FeatureName + ", " + addressCurrent.Locality);
                    mAddress = deviceAddress.ToString();
                }
                else
                {

                    mAddress = "Unable to determine the address.";
                }
            }catch(Exception e){
                mAddress = String.Empty;
            }
            return mAddress;
        }

    }
}