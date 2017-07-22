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
using Android.Support.Design.Widget;
using RescueMe.Domain;
using System.Threading;
using Android.Content.PM;
using Android.Support.V7.Widget;
using RescueMe.Droid.Adapters;
using System.Threading.Tasks;
using RescueMe.Droid.Data;
using Android.Views.Animations;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "CarsActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class CarsActivity : BaseActivity
    {

        Spinner spType;
        TextInputEditText txtMarque;

        //Android.Support.Design.Widget.TextInputLayout typeLayout;
        Android.Support.Design.Widget.TextInputLayout marqueLayout;

        RecyclerView mRecyclerView;
        AdapterVehicle mAdapter;
        RecyclerView.LayoutManager mLayoutManager;
        string[] cardViewDataset;
        string[] _typeList;
        UserProfile context;
        string message = "";
        ProgressDialog progressDialog;
        List<Vehicle> listVehicles;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MyCars);
            SetTools();

            context = _context.GetUser();

            listVehicles = await GetVehicles();
            SetRecyclerView(listVehicles);

            //Controls
            var btnAddCars = FindViewById<Button>(Resource.Id.btnAddCars);

            spType = FindViewById<Spinner>(Resource.Id.ddType);
            txtMarque = FindViewById<TextInputEditText>(Resource.Id.txtMarque);


            //typeLayout = FindViewById<Spinner>(Resource.Id.typeLayout);
            marqueLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.marqueLayout);
            _typeList = new string[] { "Seleccionar", "Autobús", "Automovil", "Camión", "Camioneta", "Jeepeta", "Motocicleta" };
            var typeListSpineer = _typeList.Select(v => new SpinnerItem
            {
                Description = v

            }).ToArray();
            var typeAdapter = new SpinnerAdapter(this, Resource.String.select_vehicle, typeListSpineer);
            spType.Adapter = typeAdapter;
            btnAddCars.Click += BtnAddCars_click;

            btnAddCars.Visibility = ViewStates.Visible;


        }


        public async Task<List<Vehicle>> GetVehicles()
        {
            List<Vehicle> vehicles = new List<Vehicle>();

            var userID = new
            {
                UserID = context.UserID
            };
            try
            {
                if (IsNetworkConnected())
                {
                    vehicles = _client.Get("Vehicle/vehicles", userID).Result.JsonToObject<List<Vehicle>>();
                    if (vehicles.Count == 0)
                    {
                        vehicles = _context.GetVehicles();
                    }
                }
                else
                {
                    vehicles = _context.GetVehicles();
                }
            }
            catch (Exception ex)
            {
                vehicles = null;
                message = ex.Message;
                vehicles = _context.GetVehicles();
            }

            return vehicles;
        }



        private void BtnAddCars_click(object sender, EventArgs e)
        {
            bool valid = true;

            //if (string.IsNullOrWhiteSpace(txType.Text))
            //{
            //    typeLayout.ErrorEnabled = true;
            //    typeLayout.Error = GetString(Resource.String.required_error_message);
            //    valid = false;
            //}
            //else
            //{
            //    typeLayout.ErrorEnabled = false;
            //}
            var selectedType = spType.SelectedItemPosition;
            if (selectedType == 0)
            {
                SetSpinnerError(spType, "Debe seleccionar un tipo de vehículo");
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(txtMarque.Text))
            {
                marqueLayout.ErrorEnabled = true;
                marqueLayout.Error = GetString(Resource.String.required_error_message);
                valid = false;
            }
            else
            {
                marqueLayout.ErrorEnabled = false;
            }



            if (valid)
            {
                Vehicle vehicle = new Vehicle();

                vehicle.Marque = txtMarque.Text;
                vehicle.Type = _typeList[selectedType];
                vehicle.UserID = context.UserID;
                var userID = new
                {
                    UserID = context.UserID
                };

                var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "Validando Información...");
                progressDialog.Indeterminate = true;
                progressDialog.SetCancelable(false);
                List<Vehicle> vehicles = null;

                new Thread(new ThreadStart(delegate
                {
                    _context.InsertVehicle(vehicle);

                    try
                    {
                        if (IsNetworkConnected())
                        {
                            vehicle = _client.Post("Vehicle/create", vehicle).Result.JsonToObject<Vehicle>();
                        }

                    }
                    catch (Exception ex)
                    {
                        vehicle = null;
                        message = ex.Message;
                    }

                    if (vehicle != null && vehicle.Id != 0)
                    {
                        listVehicles.Add(vehicle);
                        listVehicles = listVehicles.OrderByDescending(i => i.Id).ToList();
                    }
                    else
                    {
                        Snackbar.Make(marqueLayout, message, Snackbar.LengthLong)
                                .SetAction("OK", (v) =>
                                {
                                    //txType.Text = String.Empty;
                                    //spType.select
                                    txtMarque.Text = String.Empty;
                                })
                                .SetDuration(8000)
                                .SetActionTextColor(Android.Graphics.Color.Orange)
                                .Show();
                    }
                    //HIDE PROGRESS DIALOG
                    RunOnUiThread(() =>
                    {
                        SetRecyclerView(listVehicles);
                        progressDialog.Hide();
                        txtMarque.Text = String.Empty;
                        //txType.Text = String.Empty;

                    });

                })).Start();

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

        private void SetRecyclerView(List<Vehicle> dataset)
        {
            //List<Vehicle> dataset2 = (List<Vehicle>)dataset;
            mRecyclerView = (RecyclerView)FindViewById(Resource.Id.my_recycler_view);


            // use this setting to improve performance if you know that changes
            // in content do not change the layout size of the RecyclerView
            //mRecyclerView.SetHasFixedSize(true);

            // use a linear layout manager
            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            // specify an adapter (see also next example)           
            mAdapter = new AdapterVehicle(dataset, _context, marqueLayout);
            mAdapter.IsNetworkConnected = IsNetworkConnected();
            mRecyclerView.SetAdapter(mAdapter);
        }


    }
}