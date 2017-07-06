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

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "CarsActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class CarsActivity : BaseActivity
    {

        TextInputEditText txType;
        TextInputEditText txtMarque;

        Android.Support.Design.Widget.TextInputLayout typeLayout;
        Android.Support.Design.Widget.TextInputLayout marqueLayout;

        RecyclerView mRecyclerView;
        RecyclerView.Adapter mAdapter;
        RecyclerView.LayoutManager mLayoutManager;
        string[] cardViewDataset;
        UserProfile context;
        string message = "";
        ProgressDialog progressDialog;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MyCars);
            SetTools();

            context = _context.GetUser();

            List<Vehicle> listVehicles = await getVehicles();
            SetRecyclerView(listVehicles);

            //Controls
            var btnAddCars = FindViewById<Button>(Resource.Id.btnAddCars);

            txType = FindViewById<TextInputEditText>(Resource.Id.txType);
            txtMarque = FindViewById<TextInputEditText>(Resource.Id.txtMarque);


            typeLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.typeLayout);
            marqueLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.marqueLayout);

            btnAddCars.Click += btnAddCars_click;

            btnAddCars.Visibility = ViewStates.Visible;


        }


        public async Task<List<Vehicle>> getVehicles()
        {
            List<Vehicle> vehicles = new List<Vehicle>();

            var userID = new
            {
                UserID = context.Id
            };
            try
            {
                vehicles = _client.Get("Vehicle/vehicles", userID).Result.JsonToObject<List<Vehicle>>();
            }
            catch (Exception ex)
            {
                vehicles = null;
                message = ex.Message;
            }
            
            return vehicles;
        }



        private void btnAddCars_click(object sender, EventArgs e)
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txType.Text))
            {
                typeLayout.ErrorEnabled = true;
                typeLayout.Error = GetString(Resource.String.required_error_message);
                valid = false;
            }
            else
            {
                typeLayout.ErrorEnabled = false;
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
                vehicle.Type = txType.Text;
                vehicle.UserID = context.Id;
                var userID = new
                {
                    UserID = context.Id
                };

                var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "Validando Información...");
                progressDialog.Indeterminate = true;
                progressDialog.SetCancelable(false);
                List<Vehicle> vehicles = null;

                new Thread(new ThreadStart(delegate
                {
                    //LOAD METHOD TO GET ACCOUNT INFO

                    try
                    {
                        vehicle = _client.Post("Vehicle/create", vehicle).Result.JsonToObject<Vehicle>();

                    }
                    catch (Exception ex)
                    {
                        vehicle = null;
                        message = ex.Message;
                    }

                    if (vehicle != null)
                    {
                        vehicles = _client.Get("Vehicle/vehicles", userID).Result.JsonToObject<List<Vehicle>>();
                    }
                    else
                    {
                        Snackbar.Make(marqueLayout, message, Snackbar.LengthLong)
                                .SetAction("OK", (v) =>
                                {
                                    txType.Text = String.Empty;
                                    txtMarque.Text = String.Empty;
                                })
                                .SetDuration(8000)
                                .SetActionTextColor(Android.Graphics.Color.Orange)
                                .Show();
                    }
                    //HIDE PROGRESS DIALOG
                    RunOnUiThread(() =>
                    {
                       progressDialog.Hide();
                       SetRecyclerView(vehicles);
                       txtMarque.Text = String.Empty;
                       txType.Text = String.Empty;

                    });

                })).Start();

            }

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
            mAdapter = new AdapterVehicle(dataset);
            mRecyclerView.SetAdapter(mAdapter);
        }


    }
}