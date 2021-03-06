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
using static Android.Gms.Maps.GoogleMap;
using Android.Graphics;
using Android.Views.Animations;
using Android.Animation;
using Java.IO;
using System.IO;
using System.Threading;
using Clans.Fab;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "HomeActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class HomeActivity : BaseActivity, GoogleApiClient.IConnectionCallbacks,
        GoogleApiClient.IOnConnectionFailedListener, Android.Gms.Location.ILocationListener, IOnMapReadyCallback, ISnapshotReadyCallback


    {
        //UI MAp
        DrawerLayout drawerLayout;
        NavigationView navigationView;
        private GoogleMap mMap;
        Bitmap bitmap;
        public Marker agentMarker;
        public Marker clientMarker;
        private List<Marker> agentsAvailables;
        public Polyline polyLine;
        private Boolean isLocalActivity = false;

        //
        //Location currentLocation;
        //LatLng latlng;

        //Google Api Location
        protected GoogleApiClient mGoogleApiClient;
        protected LocationRequest mLocationRequest;
        protected LocationSettingsRequest mLocationSettingsRequest;
        protected Location mCurrentLocation;
        private Geocoder mGeocoder;
        protected Boolean mRequestingLocationUpdates;
        //public List<LatLng> latLngPoints;
        public Directions directions;
        Domain.Request pendingRequest;
        LatLng agentLatLng;
        public List<AgentLocation> agentsLatLngPoints;

        //Configuration Request
        protected const string TAG = "location-settings";
        protected const int REQUEST_CHECK_SETTINGS = 0x1;
        public const long UPDATE_INTERVAL_IN_MILLISECONDS = 10000;
        public const long FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS = UPDATE_INTERVAL_IN_MILLISECONDS / 2;
        protected const string KEY_REQUESTING_LOCATION_UPDATES = "requesting-location-updates";
        protected const string KEY_LOCATION = "location";

        ImageButton request;
        private FrameLayout frameLayoutMenu;
        int counter = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Init Btn menu
            if (savedInstanceState == null)
            {
                //SupportFragmentManager.BeginTransaction().Add(Resource.Id.fragment, new MenusFragment()).Commit();
                var menuFragment = new MenusFragment();
                _context.IsNetworkConnected = IsNetworkConnected();
                menuFragment.Initialize(_client, _context);
                SupportFragmentManager.BeginTransaction().Add(Resource.Id.fragment, menuFragment).Commit();
            }

            SetContentView(Resource.Layout.Home);
            var menu = FindViewById(Resource.Id.menuIcon);
            var call = FindViewById<ImageButton>(Resource.Id.btnCall);
            request = FindViewById<ImageButton>(Resource.Id.btnRescue);
            frameLayoutMenu = FindViewById<FrameLayout>(Resource.Id.fragment);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            mGeocoder = new Geocoder(this);
            menu.Click += Menu_Click;
            navigationView.NavigationItemSelected += NavigationItemSelected;
            call.Click += Call_Click;
            request.Click += Request_Click;



            //Load values of last instance
            UpdateValuesFromBundle(savedInstanceState);





            //Settings for ApiClient G
            BuildGoogleApiClient();
            CreateLocationRequest();
            BuildLocationSettingsRequest();
        }


        private GroundOverlay cobjGroundOverlay = null;

        private void Request_Click(object sender, EventArgs e)
        {
            var bundle = new Bundle();
            bundle.PutDouble("Latitude", mCurrentLocation.Latitude);
            bundle.PutDouble("Longitude", mCurrentLocation.Longitude);

            var intent = new Intent(this, typeof(RequestActivity));
            intent.PutExtra("location", bundle);

            //Animation anim = AnimationUtils.LoadAnimation(ApplicationContext,
            //               Resource.Animation.fade_in);
            //request.StartAnimation(anim);

            mMap.Snapshot(this);
            isLocalActivity = true;
            StartActivity(intent);

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

        public void UpdateLocationUI()
        {
            if (mCurrentLocation != null && mMap != null)
            {
                //mMap.Clear();
                LatLng latlng;

                if (pendingRequest == null)
                {
                    latlng = new LatLng(mCurrentLocation.Latitude, mCurrentLocation.Longitude);
                }
                else
                {
                    latlng = new LatLng(Double.Parse(pendingRequest.Latitude.ToString()), Double.Parse(pendingRequest.Longitude.ToString()));
                }

                int intWidth = 100;
                int intHeight = 100;

                // To use the map or image of the map in the offline (not internet) mode.
                using (Bitmap objBitmap = GenerateMyCustomDrawnOverlay(intWidth, intHeight))
                {
                    using (Android.Gms.Maps.Model.BitmapDescriptor objBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptorFactory.FromBitmap(objBitmap))
                    {
                        GroundOverlayOptions objGroundOverlayOptions = new GroundOverlayOptions()
                            .Position(latlng, intWidth, intHeight)
                            .InvokeImage(objBitmapDescriptor);
                        //
                        cobjGroundOverlay = mMap.AddGroundOverlay(objGroundOverlayOptions);
                    }
                }

                if (clientMarker == null)
                {

                    CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 15);
                    MarkerOptions markerOptions = new MarkerOptions()
                                                         .SetPosition(latlng)
                                                          .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.market))
                                                         // .SetTitle("My Position")
                                                         ;
                    clientMarker = mMap.AddMarker(markerOptions);
                    mMap.MoveCamera(camera);
                }
                else
                {
                    clientMarker.Position = latlng;
                }

                //if (pendingRequest != null && agentLatLng != null && latLngPoints != null)
                if (pendingRequest != null && agentLatLng != null && directions.Points != null)
                {
                    LatLng latlngAgent = new LatLng(agentLatLng.Latitude, agentLatLng.Longitude);

                    if (agentMarker == null && latlngAgent.Latitude != 0 && polyLine == null)
                    {
                        MarkerOptions markerOptionsClient = new MarkerOptions()
                                             .SetPosition(latlngAgent)
                                                .SetTitle(pendingRequest.UpdatedBy.ToString())
                                              .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.markerAgent));

                        agentMarker = mMap.AddMarker(markerOptionsClient);

                        polyLine = mMap.AddPolyline(new PolylineOptions().Geodesic(true)
                                //.Add(latLngPoints.ToArray()));
                                .Add(directions.Points.ToArray()));
                        //Remove all Agents Availables 
                        if (agentsAvailables != null && agentsAvailables.Count > 0)
                        {
                            foreach (var marker in agentsAvailables)
                            {
                                marker.Remove();
                            }
                            agentsAvailables = null;
                        }
                    }
                    //else if (latLngPoints.FirstOrDefault().Latitude == 0 && agentMarker != null && polyLine != null)
                    else if (directions.Points.FirstOrDefault().Latitude == 0 && agentMarker != null && polyLine != null)
                    {
                        //polyLine.Points.Clear();
                        polyLine.Remove();
                        agentMarker.Remove();
                        polyLine = null;
                        agentMarker = null;
                    }
                    else if (agentMarker != null && polyLine != null)
                    {
                        agentMarker.Position = latlngAgent;
                        //polyLine.Points = latLngPoints.ToArray();
                        polyLine.Points = directions.Points.ToArray();
                    }
                }

                //Get Agents availables markers
                if (agentMarker == null && agentsAvailables == null)
                {

                    if (agentsLatLngPoints != null)
                    {
                        agentsAvailables = new List<Marker>();
                        foreach (var agentPosition in agentsLatLngPoints)
                        {
                            MarkerOptions markerOptions = new MarkerOptions()
                                                            .SetTitle(agentPosition.AgentId.ToString())
                                                           .SetPosition(agentPosition.Location)
                                                            .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.markerAgent))
                                                           ;
                            Marker mAgentMarker = mMap.AddMarker(markerOptions);
                            agentsAvailables.Add(mAgentMarker);
                        }
                    }

                }
                else if (agentsAvailables != null)
                {
                    //Get all news points about agents
                    if (agentsLatLngPoints != null)
                    {
                        foreach (var agentPosition in agentsLatLngPoints)
                        {
                            //Get Agent marker
                            var marker =
                                agentsAvailables.FirstOrDefault(a => a.Title == agentPosition.AgentId.ToString());
                            if (marker != null)
                            {
                                marker.Position = agentPosition.Location;
                            }

                            //else
                            //{
                            //    //Remove marker of Agent
                            //    marker.Remove();
                            //}
                        }
                        //Get all markers
                        foreach (var marker in agentsAvailables)
                        {
                            var agent = agentsLatLngPoints.FirstOrDefault(f => f.AgentId.ToString() == marker.Title);
                            if (agent == null)
                            {
                                marker.Remove();
                            }
                            marker.Position = agent.Location;
                        }

                        //Is not same count agents get excepts
                        if (agentsAvailables.Count != agentsLatLngPoints.Count)
                        {
                            foreach (var agentPosition in agentsLatLngPoints)
                            {
                                if (!agentsAvailables.Any(a => a.Title == agentPosition.AgentId.ToString()))
                                {
                                    MarkerOptions markerOptions = new MarkerOptions()
                                                         .SetTitle(agentPosition.AgentId.ToString())
                                                        .SetPosition(agentPosition.Location)
                                                         .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.markerAgent))
                                                        ;
                                    Marker mAgentMarker = mMap.AddMarker(markerOptions);
                                    agentsAvailables.Add(mAgentMarker);
                                }
                            }
                        }
                    }
                }
                mMap.SetInfoWindowAdapter(new Adapters.MarkerInfoAdapter(LayoutInflater, mGeocoder, mCurrentLocation, directions)
                {
                    IsNetworkConnected = IsNetworkConnected()
                });
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
            isLocalActivity = false;
            mGoogleApiClient.Connect(); 
        }


        protected override async void OnResume()
        {
            base.OnResume();
            //RequestStatusChanged(); // Validate status request
            if (mGoogleApiClient.IsConnected)
            {
                await StartLocationUpdates();
            }
            new Thread(new ThreadStart(delegate
            {
                //Thread.Sleep(1000);
                this.RunOnUiThread(() =>
                {
                    RequestStatusChanged(); // Validate status request
                });
            })).Start();

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
            if (isLocalActivity == false)
            {
                mGoogleApiClient.Disconnect();
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            Log.Info(TAG, "Connected to GoogleApiClient");
            SetUp();

            if (mCurrentLocation == null)
            {
                mCurrentLocation = LocationServices.FusedLocationApi.GetLastLocation(mGoogleApiClient);
                if (_isAllowed)
                {
                    SetUpMap();
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

            if (counter == 2 && IsNetworkConnected())
            {
                new Thread(new ThreadStart(delegate
                {
                    if (pendingRequest != null)
                    {
                        GetDirections();
                    }
                    else
                    {
                        GetAgentsAvailables();
                    }

                })).Start();
                counter = 0;

            }
            else
            {
                counter++;
            }
        }

        public void GetDirections()
        {
            pendingRequest = _context.GetRequest().FirstOrDefault(s => s.Status.Name == "pendiente" || s.Status.Name == "asignado" || s.Status.Name == "no disponible");

            if (pendingRequest != null)
            {
                if (pendingRequest.Status.Name == "asignado")
                {
                    try
                    {
                        //latLngPoints = _client.Get("Map/Directions", new
                        //{
                        //    Id = pendingRequest.Id
                        //}).Result.JsonToObject<List<LatLng>>();

                        directions = _client.Get("Map/Directions", new
                        {
                            Id = pendingRequest.Id
                        }).Result.JsonToObject<Directions>();

                        agentLatLng = _client.Get("Request/CurrentAgent", new
                        {
                            Id = pendingRequest.Id
                        }).Result.JsonToObject<List<LatLng>>().FirstOrDefault();


                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
            }
        }

        private void GetAgentsAvailables()
        {
            pendingRequest = _context.GetRequest().FirstOrDefault(s => s.Status.Name == "pendiente" || s.Status.Name == "asignado" || s.Status.Name == "no disponible");

            if (pendingRequest == null)
            {
                try
                {
                    var address =
                        mGeocoder.GetFromLocation(mCurrentLocation.Latitude, mCurrentLocation.Longitude, 10);
                    var cityName = address.FirstOrDefault().Locality;

                    agentsLatLngPoints = _client.Get("Agent/Availables", new
                    {
                        city = cityName
                    }).Result.JsonToObject<List<AgentLocation>>();
                }
                catch (Exception e)
                {

                    throw;
                }
            }
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
                            SetUpMap();
                            CheckLocationSettings();
                            _isAllowed = true;

                        }
                        else
                        {
                            CheckLocationSettings();
                            //Permission Denied :( :(
                            //Disabling location functionality
                            //var snack = Snackbar.Make(message, "No se puede Ubicar por los permisos negados...", Snackbar.LengthIndefinite);
                            //snack.Show();

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
            //Resource _resources;
            mMap.UiSettings.ZoomControlsEnabled = true;
            mMap.UiSettings.ZoomGesturesEnabled = true;
            mMap.SetMaxZoomPreference(17);
            //mMap.SetMinZoomPreference(14);
            mMap.UiSettings.CompassEnabled = false;
            mMap.UiSettings.MyLocationButtonEnabled = true;

            bool success = mMap.SetMapStyle(new MapStyleOptions(GetString(Resource.String.style_json)));
            if (!success)
            {
                Log.Info(TAG, "Style parsing failed.");
            }
            else
            {
                CheckLocationSettings();

            }

        }

        private Bitmap GenerateMyCustomDrawnOverlay(int pintWidth, int pintHeight)
        {
            Bitmap objBitmap = Bitmap.CreateBitmap(pintWidth, pintHeight, Bitmap.Config.Argb8888);
            return objBitmap;
        }



        private void Menu_Click(object sender, EventArgs e)
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
            isLocalActivity = true;
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
                    if (IsNetworkConnected())
                    {
                        _context.LogOut();
                        mGoogleApiClient.Disconnect();
                        this.Finish();
                        intent = new Intent(this, typeof(MainActivity));
                        StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(this, "No puede desloguearse sin conexión a internet.", ToastLength.Long).Show();
                    }
                  
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

        private void Call_Click(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse("tel:8296881000");
            Intent callIntent = new Intent(Intent.ActionDial, uri);
            isLocalActivity = true;
            StartActivity(callIntent);
        }

        public void OnSnapshotReady(Bitmap snapshot)
        {
            bitmap = snapshot;
            try
            {
                //int height = Resources.DisplayMetrics.HeightPixels;
                //int width = _imageView.Height;
                //App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);

                string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string requestID = (_context.GetRequest().FirstOrDefault() == null ? 0 : _context.GetRequest().FirstOrDefault().Id + 1).ToString();
                string localFilename = $"{requestID}_{_context.GetUser().UserID}.png";
                string localPath = System.IO.Path.Combine(documentsPath, localFilename);

                if (!System.IO.File.Exists(localPath))
                {
                    FileStream stream = new FileStream(localPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                    bitmap = Bitmap.CreateScaledBitmap(bitmap, 620, 400, false);
                    ByteArrayOutputStream bytes = new ByteArrayOutputStream();
                    bitmap.Compress(Bitmap.CompressFormat.Png, 90, stream);


                    stream.Close();
                }

            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public void RequestStatusChanged(int id = -1)
        {
            //var pendingRequest = _context.GetRequest().FirstOrDefault(p => p.Status.Name == "pendiente"
            //                                                     || p.Status.Name == "asignado" || p.Status.Name == "no disponible");
            Domain.Request statusRequest = null;
            if (id == -1)
            {
                statusRequest = _context.GetRequest().FirstOrDefault(p => p.Status.Name == "pendiente"
                                                                  || p.Status.Name == "asignado" || p.Status.Name == "no disponible");
            }
            else
            {
                statusRequest = _context.GetRequest().FirstOrDefault(r => r.Id == id);
            }

            //Menu
            var btnMenu = FindViewById<FloatingActionMenu>(Resource.Id.btnMenu);
           
            if (statusRequest != null)
            {
                switch (statusRequest.Status.Name.ToLower())
                {
                    case "pendiente":
                    case "asignado":
                    case "no disponible":
                        if (IsNetworkConnected())
                        {
                            GetDirections();
                        }
                        frameLayoutMenu.Visibility = ViewStates.Visible;
                        btnMenu.Visibility = ViewStates.Visible;
                        request.Visibility = ViewStates.Gone;
                        break;
                    case "cancelado":
                    case "completado":
                        if (directions != null && directions.Points != null)
                        {
                            directions.Points.Clear();
                            directions.Points.Add(new Android.Gms.Maps.Model.LatLng(0, 0));
                        }
                        UpdateLocationUI();
                        btnMenu.Visibility = ViewStates.Gone;
                        request.Visibility = ViewStates.Visible;

                        break;
                        //default:
                        //    frameLayoutMenu.Visibility = ViewStates.Gone;
                        //    request.Visibility = ViewStates.Visible;
                        //    break;

                }
            }
            else
            {
                frameLayoutMenu.Visibility = ViewStates.Gone;
                request.Visibility = ViewStates.Visible;
            }
        }

    }
}