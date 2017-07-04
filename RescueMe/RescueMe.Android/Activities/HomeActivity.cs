﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using RescueMe.Droid.Data;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Util;
using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Common.Apis;
using Android.Gms.Common;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "HomeActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class HomeActivity : BaseActivity, GoogleApiClient.IConnectionCallbacks,
        GoogleApiClient.IOnConnectionFailedListener, Android.Gms.Location.ILocationListener, IOnMapReadyCallback


    {
        //UI MAp
        DrawerLayout drawerLayout;
        NavigationView navigationView;
        private GoogleMap mMap;

        //GoogleApiClient apiClient;
        //LocationRequest locRequest;


        //
        //Location currentLocation;
        //LatLng latlng;

        //Google Api Location
        protected GoogleApiClient mGoogleApiClient;
        protected LocationRequest mLocationRequest;
        protected LocationSettingsRequest mLocationSettingsRequest;
        protected Location mCurrentLocation;
        
        protected Boolean mRequestingLocationUpdates;

        //Configuration Request
        protected const string TAG = "location-settings";
        protected const int REQUEST_CHECK_SETTINGS = 0x1;
        public const long UPDATE_INTERVAL_IN_MILLISECONDS = 10000;
        public const long FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS = UPDATE_INTERVAL_IN_MILLISECONDS / 2;
        protected const string KEY_REQUESTING_LOCATION_UPDATES = "requesting-location-updates";
        protected const string KEY_LOCATION = "location";



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Home);
            var menu = FindViewById(Resource.Id.menuIcon);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

            menu.Click += menu_Click;
            navigationView.NavigationItemSelected += NavigationItemSelected;




            //Load values of last instance
            UpdateValuesFromBundle(savedInstanceState);





            //Settings for ApiClient G
            BuildGoogleApiClient();
            CreateLocationRequest();
            BuildLocationSettingsRequest();

        }
        

        void UpdateValuesFromBundle(Bundle savedInstanceState)
        {
            if (savedInstanceState != null)
            {
                if (savedInstanceState.KeySet().Contains(KEY_REQUESTING_LOCATION_UPDATES))
                {
                    mRequestingLocationUpdates = savedInstanceState.GetBoolean(
                        KEY_REQUESTING_LOCATION_UPDATES);
                }

                if (savedInstanceState.KeySet().Contains(KEY_LOCATION))
                {
                    mCurrentLocation = (Location)savedInstanceState.GetParcelable(KEY_LOCATION);
                }

                UpdateLocationUI();
            }
        }

        protected void BuildGoogleApiClient()
        {
            Log.Info(TAG, "Building GoogleApiClient");
            mGoogleApiClient = new GoogleApiClient.Builder(this)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .AddApi(LocationServices.API)
                .Build();
        }

        protected void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL_IN_MILLISECONDS);
            mLocationRequest.SetFastestInterval(FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
        }

        protected void BuildLocationSettingsRequest()
        {
            LocationSettingsRequest.Builder builder = new LocationSettingsRequest.Builder();
            builder.AddLocationRequest(mLocationRequest);
            mLocationSettingsRequest = builder.Build();
        }

        protected async Task CheckLocationSettings()
        {
            var result = await LocationServices.SettingsApi.CheckLocationSettingsAsync(
                mGoogleApiClient, mLocationSettingsRequest);
            await HandleResult(result);
        }

        void UpdateLocationUI()
        {
            if (mCurrentLocation != null && mMap != null)
            {
                mMap.Clear();

                LatLng latlng = new LatLng(mCurrentLocation.Latitude, mCurrentLocation.Longitude);

                CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 15);
                MarkerOptions markerOptions = new MarkerOptions()
                                                     .SetPosition(latlng)
                                                      .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.market))
                                                     .SetTitle("My Position");


                mMap.AddMarker(markerOptions);


                mMap.UiSettings.ZoomControlsEnabled = true;
                mMap.UiSettings.CompassEnabled = true;
                mMap.MoveCamera(camera);
            }
        }



        public async Task HandleResult(LocationSettingsResult locationSettingsResult)
        {
            var status = locationSettingsResult.Status;
            switch (status.StatusCode)
            {
                case CommonStatusCodes.Success:
                    Log.Info(TAG, "All location settings are satisfied.");
                    await StartLocationUpdates();
                    break;
                case CommonStatusCodes.ResolutionRequired:
                    Log.Info(TAG, "Location settings are not satisfied. Show the user a dialog to" +
                    "upgrade location settings ");

                    try
                    {
                        status.StartResolutionForResult(this, REQUEST_CHECK_SETTINGS);
                    }
                    catch (IntentSender.SendIntentException)
                    {
                        Log.Info(TAG, "PendingIntent unable to execute request.");
                    }
                    break;
                case LocationSettingsStatusCodes.SettingsChangeUnavailable:
                    Log.Info(TAG, "Location settings are inadequate, and cannot be fixed here. Dialog " +
                    "not created.");
                    break;
            }
        }

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case REQUEST_CHECK_SETTINGS:
                    switch (resultCode)
                    {
                        case Result.Ok:
                            Log.Info(TAG, "User agreed to make required location settings changes.");
                            await StartLocationUpdates();
                            break;
                        case Result.Canceled:
                            Log.Info(TAG, "User chose not to make required location settings changes.");
                            break;
                    }
                    break;
            }
        }


        protected async Task StartLocationUpdates()
        {
            await LocationServices.FusedLocationApi.RequestLocationUpdates(
                mGoogleApiClient,
                mLocationRequest,
                this
            );

            mRequestingLocationUpdates = true;
        }


        protected async Task StopLocationUpdates()
        {
            await LocationServices.FusedLocationApi.RemoveLocationUpdates(
                    mGoogleApiClient,
                    this
                );

            mRequestingLocationUpdates = false;
        }

        protected override void OnStart()
        {
            base.OnStart();
            mGoogleApiClient.Connect();
        }

        protected override async void OnResume()
        {
            base.OnResume();
            if (mGoogleApiClient.IsConnected)
            {
                await StartLocationUpdates();
            }
        }

        protected override async void OnPause()
        {
            base.OnPause();
            if (mGoogleApiClient.IsConnected)
            {
                await StopLocationUpdates();
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            mGoogleApiClient.Disconnect();
        }

        public void OnConnected(Bundle connectionHint)
        {
            Log.Info(TAG, "Connected to GoogleApiClient");
            SetUp();

            if (mCurrentLocation == null)
            {
                mCurrentLocation = LocationServices.FusedLocationApi.GetLastLocation(mGoogleApiClient);

                SetUpMap();
                if (_isAllowed)
                {
                    UpdateLocationUI();
                }
                    

            }
        }

        public void OnConnectionSuspended(int cause)
        {
            Log.Info(TAG, "Connection suspended");
        }

        public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
        {
            Log.Info(TAG, "Connection failed: ConnectionResult.getErrorCode() = " + result.ErrorCode);
        }

        public void OnLocationChanged(Location location)
        {
            mCurrentLocation = location;
            UpdateLocationUI();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutBoolean(KEY_REQUESTING_LOCATION_UPDATES, mRequestingLocationUpdates);
            outState.PutParcelable(KEY_LOCATION, mCurrentLocation);
            base.OnSaveInstanceState(outState);
        }




        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {

            var message = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.message);
            switch (requestCode)
            {
                case 0:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            //Permission granted :)
                            CheckLocationSettings();
                            _isAllowed = true;
                        }
                        else
                        {
                            //Permission Denied :( :(
                            //Disabling location functionality
                            var snack = Snackbar.Make(message, "No se puede Ubicar por los permisos negados...", Snackbar.LengthIndefinite);
                            snack.Show();
                        }
                    }
                    break;
            }
        }


        

        private void SetUpMap()
        {
            if (mMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map).GetMapAsync(this);
            }
        }


        public void OnMapReady(GoogleMap googleMap)
        {
            mMap = googleMap;
            CheckLocationSettings();
        }
        

        private void menu_Click(object sender, EventArgs e)
        {
            if (drawerLayout.IsDrawerOpen(GravityCompat.Start))
            {
                drawerLayout.CloseDrawer(GravityCompat.Start);

            }
            else
            {
                drawerLayout.OpenDrawer(GravityCompat.Start);
                GetUserInfo();
            }
        }

        private void NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            var menuItem = e.MenuItem;
            menuItem.SetChecked(!menuItem.IsChecked);
            drawerLayout.CloseDrawer(GravityCompat.Start);
            Intent intent;
            switch (menuItem.ItemId)
            {
                case Resource.Id.nav_profile:
                    intent = new Intent(this, typeof(ProfileActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.nav_rescue:
                    intent = new Intent(this, typeof(RescueActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.nav_directory:
                    intent = new Intent(this, typeof(DirectoryActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.nav_about:
                    intent = new Intent(this, typeof(AboutActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.nav_logOut:
                    _context.LogOut();
                    this.Finish();
                    intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                    //intent = new Intent(this, typeof(ProfileActivity));
                    //StartActivity(intent);
                    break;
            }
        }

        public override void OnBackPressed()
        {
            drawerLayout.CloseDrawer(GravityCompat.Start);
            return;
        }

        //Get User Information
        private void GetUserInfo()
        {
            var name = FindViewById<TextView>(Resource.Id.userName);
            name.Text = _context.GetUser().Name;
        }
    }
}