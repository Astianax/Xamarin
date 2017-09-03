using System;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Util;
using System.Threading.Tasks;
using Android.Content.PM;
using Android.Gms.Location;
using Android.Gms.Common.Apis;
using static Android.Gms.Maps.GoogleMap;
using Android.Graphics;
using Java.IO;
using System.IO;
using System.Threading;
using Android.Views.Animations;
using System.Collections.Generic;
using RescueMe.Agent.Data;
using Clans.Fab;

namespace RescueMe.Agent.Activities
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
        private List<LatLng> latLngPoints;
        //Directions and Time new implementation
        private Directions directions;
        Domain.Request pendingRequest;

        //Configuration Request
        protected const string TAG = "location-settings";
        protected const int REQUEST_CHECK_SETTINGS = 0x1;
        public const long UPDATE_INTERVAL_IN_MILLISECONDS = 10000;
        public const long FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS = UPDATE_INTERVAL_IN_MILLISECONDS / 2;
        protected const string KEY_REQUESTING_LOCATION_UPDATES = "requesting-location-updates";
        protected const string KEY_LOCATION = "location";

        ImageButton available;
        ImageButton unAvailable;
        private FrameLayout frameLayoutMenu;

        int counter = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Init Btn menu
            if (savedInstanceState == null)
            {
                var menuFragment = new MenusFragment();
                menuFragment.Initialize(_client, _context);
                SupportFragmentManager.BeginTransaction().Add(Resource.Id.fragment, menuFragment).Commit();
            }


            SetContentView(Resource.Layout.HomeContainer);
            var menu = FindViewById(Resource.Id.menuIcon);
            var btnLogout = FindViewById<ImageButton>(Resource.Id.logout);
            available = FindViewById<ImageButton>(Resource.Id.btnAvailable);
            unAvailable = FindViewById<ImageButton>(Resource.Id.btnUnavailable);
            frameLayoutMenu = FindViewById<FrameLayout>(Resource.Id.fragment);
            //drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            //navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            mGeocoder = new Geocoder(this);
            menu.Click += Menu_Click;
            //navigationView.NavigationItemSelected += NavigationItemSelected;
            btnLogout.Click += Logout_Click;
            available.Click += Available_Click;
            unAvailable.Click += UnAvailable_Click;



            //Load values of last instance
            UpdateValuesFromBundle(savedInstanceState);





            //Settings for ApiClient G
            BuildGoogleApiClient();
            CreateLocationRequest();
            BuildLocationSettingsRequest();
        }

        private void UnAvailable_Click(object sender, EventArgs e)
        {
            string status = "";
            string message = "";

            var requestID = new
            {
                agentId = _context.GetUser().UserID
            };

            var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "");
            progressDialog.Indeterminate = true;
            progressDialog.SetCancelable(false);

            new Thread(new ThreadStart(delegate
                {
                    try
                    {

                        status = _client.Get("Agent/disconnect", requestID).Result.ToString();
                        message = "Desconectado";
                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                    }

                    RunOnUiThread(() =>
                    {
                        progressDialog.Hide();

                        if (status.ToLower() == "true")
                        {
                            _context.UpdateAvailability(false);
                            unAvailable.Visibility = ViewStates.Gone;
                            available.Visibility = ViewStates.Visible;
                        }

                        Toast toast = Toast.MakeText(this, message, ToastLength.Long);
                        toast.SetGravity(GravityFlags.Bottom | GravityFlags.Center, 100, 100);
                        toast.Show();
                    });

                })).Start();

        }

        private void Available_Click(object sender, EventArgs e)
        {

            string status = "";
            string message = "";
            string city = RescueMe.Agent.Activities.BaseActivity.GetAddress(mCurrentLocation, mGeocoder);


            var agentLocation = new
            {
                AgentID = _context.GetUser().UserID,
                City = city,
                Location = new
                {
                    lat = mCurrentLocation.Latitude,
                    lng = mCurrentLocation.Longitude
                }
            };

            var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "");
            progressDialog.Indeterminate = true;
            progressDialog.SetCancelable(false);

            new Thread(new ThreadStart(delegate
            {
                try
                {

                    status = _client.Post("Agent/connect", agentLocation).Result.JsonToBoolean().ToString();
                    message = "Conectado";
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }

                RunOnUiThread(() =>
                 {
                     progressDialog.Hide();
                     if (status.ToLower() == "true")
                     {
                         _context.UpdateAvailability(true);
                         available.Visibility = ViewStates.Gone;
                         unAvailable.Visibility = ViewStates.Visible;

                     }
                     Toast toast = Toast.MakeText(this, message, ToastLength.Long);
                     toast.SetGravity(GravityFlags.Bottom | GravityFlags.Center, 100, 100);
                     toast.Show();
                 });

            })).Start();

        }

        private GroundOverlay cobjGroundOverlay = null;


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
                //mMap.Clear();
                LatLng latlng = new LatLng(mCurrentLocation.Latitude, mCurrentLocation.Longitude);



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

                CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 15);
                if (agentMarker == null)
                {
                    MarkerOptions markerOptions = new MarkerOptions()
                                                         .SetPosition(latlng)
                                                          .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.marker));
                    agentMarker = mMap.AddMarker(markerOptions);
                }
                else
                {
                    agentMarker.Position = latlng;
                }

                if (pendingRequest != null && clientMarker == null && directions != null && directions.Points != null)
                //if (pendingRequest != null && clientMarker == null && latLngPoints != null)
                {
                    //if (latLngPoints.FirstOrDefault().Latitude != 0)
                    if (directions.Points.FirstOrDefault().Latitude != 0)
                    {
                        LatLng latlngClient = new LatLng(double.Parse(pendingRequest.Latitude.ToString()), double.Parse(pendingRequest.Longitude.ToString()));
                        MarkerOptions markerOptionsClient = new MarkerOptions()
                                                 .SetPosition(latlngClient)
                                                  .InvokeIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.markerClient));
                        clientMarker = mMap.AddMarker(markerOptionsClient);
                        // Polylines are useful for marking paths and routes on the map.
                        polyLine = mMap.AddPolyline(new PolylineOptions().Geodesic(true)
                             .Add(directions.Points.ToArray()));
                        //.Add(latLngPoints.ToArray()));
                    }
                    //else if ((pendingRequest == null || latLngPoints.FirstOrDefault().Latitude == 0 )
                    else if ((pendingRequest == null || directions.Points.FirstOrDefault().Latitude == 0)
                        && clientMarker != null && polyLine != null)
                    {
                        clientMarker.Remove();
                        polyLine.Remove();
                        clientMarker = null;
                        polyLine = null;
                    }
                    else if (polyLine != null)
                    {
                        //polyLine.Points = latLngPoints.ToArray();
                        polyLine.Points = directions.Points.ToArray();
                    }
                    //Perrito verde
                }
                else if (clientMarker != null && polyLine != null && pendingRequest == null)
                {
                    clientMarker.Remove();
                    polyLine.Remove();
                    clientMarker = null;
                    polyLine = null;
                }
                //Animation anim = AnimationUtils.LoadAnimation(ApplicationContext,
                //Resource.Animation.jump);
                //markerOptions.StartAnimation(anim);


                mMap.SetInfoWindowAdapter(new Adapters.MarkerInfoAdapter(LayoutInflater, mGeocoder, mCurrentLocation, directions)
                {
                    IsNetworkConnected = IsNetworkConnected()
                });
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
            RequestStatusChanged();
        }

        protected override async void OnResume()
        {
            base.OnResume();
            if (mGoogleApiClient.IsConnected)
            {
                await StartLocationUpdates();
                RequestStatusChanged();
            }
        }

        protected override async void OnPause()
        {
            base.OnPause();
            //if (mGoogleApiClient.IsConnected)
            //{
            //    await StopLocationUpdates();
            //}
        }

        protected override void OnStop()
        {
            base.OnStop();
            //mGoogleApiClient.Disconnect();
        }

        public void OnConnected(Bundle connectionHint)
        {
            Log.Info(TAG, "Connected to GoogleApiClient");
            //SetUp();

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

            if (counter == 1) // 1 is original value
            {
                new Thread(new ThreadStart(delegate
                {
                    SendAgentStatus(mCurrentLocation, mGeocoder);
                    RunOnUiThread(() =>
                    {
                        RequestStatusChanged();
                    });
                })).Start();

                counter = 0;
            }
            else
            {
                counter++;
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
            //mMap.SetMaxZoomPreference(17);
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
            isLocalActivity = true;
            Intent intent = new Intent(this, typeof(RescueActivity));
            StartActivity(intent);

        }

        //private void NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        //{
        //    var menuItem = e.MenuItem;
        //    menuItem.SetChecked(!menuItem.IsChecked);
        //    drawerLayout.CloseDrawer(GravityCompat.Start);
        //    Intent intent;
        //    switch (menuItem.ItemId)
        //    {
        //        case Resource.Id.nav_profile:
        //            intent = new Intent(this, typeof(ProfileActivity));
        //            StartActivity(intent);
        //            break;
        //        case Resource.Id.nav_rescue:
        //            intent = new Intent(this, typeof(RescueActivity));
        //            StartActivity(intent);
        //            break;
        //        case Resource.Id.nav_directory:
        //            intent = new Intent(this, typeof(DirectoryActivity));
        //            StartActivity(intent);
        //            break;
        //        case Resource.Id.nav_about:
        //            intent = new Intent(this, typeof(AboutActivity));
        //            StartActivity(intent);
        //            break;
        //        case Resource.Id.nav_logOut:
        //            _context.LogOut();
        //            this.Finish();
        //            intent = new Intent(this, typeof(MainActivity));
        //            StartActivity(intent);
        //            //intent = new Intent(this, typeof(ProfileActivity));
        //            //StartActivity(intent);
        //            break;
        //    }
        //}

        public override void OnBackPressed()
        {
            //drawerLayout.CloseDrawer(GravityCompat.Start);
            return;
        }

        //Get User Information
        //private void GetUserInfo()
        //{
        //    var name = FindViewById<TextView>(Resource.Id.userName);
        //    name.Text = _context.GetUser().Name;
        //}

        private void Logout_Click(object sender, EventArgs e)
        {
            unAvailable.CallOnClick();

            //RunOnUiThread(() =>
            //{
            _context.LogOut();
            this.Finish();
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
            //});

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
                //throw e;
                Log.Error("Agent", e.Message);
            }

        }


        public void RequestStatusChanged(int id =-1)
        {
            if (id == -1) {
                //Only for Pending Requests
                new Thread(new ThreadStart(delegate
                {
                    //First validate if have pending request local
                    pendingRequest = _context.GetRequest().FirstOrDefault(s => s.AgentStatus.Name == "asignado");
                    if (pendingRequest == null)
                    {
                        //Validate if have pending Request Online
                        var requestAssign = _context.GetPendingRescues(_client, _context.GetUser());
                        _context.InsertLastPending(requestAssign);
                    }
                    else
                    {
                        //api/Agent/requestStatus
                        var status = _context.GetStatusRescue(_client, pendingRequest.Id, _context.GetUser());
                        if (status != pendingRequest.AgentStatusID && status != -1)
                        {
                            _context.UpdateRequestStatus(pendingRequest.Id, status);
                        }
                        //_context.InsertLastPending(requestAssign);
                    }
                    //Repeat again local request
                    pendingRequest = _context.GetRequest().FirstOrDefault(s => s.AgentStatus.Name == "asignado");

                    RunOnUiThread(() =>
                    {
                        //If Pending Request is assign so Show Button is not hidden
                        if (pendingRequest != null)
                        {
                            frameLayoutMenu.Visibility = ViewStates.Visible;
                            available.Visibility = ViewStates.Gone;
                            unAvailable.Visibility = ViewStates.Gone;
                            try
                            {
                                GetDirections();
                            }
                            catch (Exception e)
                            {
                                Log.Error("DirectionsButtonHome", e.Message);
                            }

                        }
                        else
                        {
                            frameLayoutMenu.Visibility = ViewStates.Gone;
                            if (_context.GetSettings().AgentAvailability)
                            {
                                unAvailable.Visibility = ViewStates.Visible;
                                available.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                unAvailable.Visibility = ViewStates.Gone;
                                available.Visibility = ViewStates.Visible;

                            }
                        }
                    });

                })).Start();
            }
            else if (id > 0)
            {
                var statusRequest = _context.GetRequest().FirstOrDefault(s => s.Id == id);
                //Validate state of Request.
                switch (statusRequest.AgentStatus.Name.ToLower())
                {
                    case "completado":
                    case "cancelado":
                        var btnMenu = FindViewById<FloatingActionMenu>(Resource.Id.btnMenu);
                        if (directions != null && directions.Points != null)
                        {
                            directions.Points.Clear();
                            directions.Points.Add(new Android.Gms.Maps.Model.LatLng(0, 0));
                        }
                        UpdateLocationUI();
                        btnMenu.Visibility = ViewStates.Gone;
                        unAvailable.Visibility = ViewStates.Visible;
                        available.Visibility = ViewStates.Gone;
                        break;
                }
            }
            else
            {
                frameLayoutMenu.Visibility = ViewStates.Gone;
                if (_context.GetSettings().AgentAvailability)
                {
                    unAvailable.Visibility = ViewStates.Visible;
                    available.Visibility = ViewStates.Gone;
                }
                else
                {
                    unAvailable.Visibility = ViewStates.Gone;
                    available.Visibility = ViewStates.Visible;

                }
            }
        }
        public void GetDirections()
        {
            pendingRequest = _context.GetRequest().FirstOrDefault(s => s.AgentStatus.Name == "asignado");
            if (pendingRequest != null && pendingRequest.AgentStatus.Name == "asignado")
            {
                try
                {
                    var result = _client.Get("Map/Directions", new
                    {
                        Id = pendingRequest.Id,
                        app = "beta"
                    }).Result;
                    //latLngPoints = result.JsonToObject<List<LatLng>>();
                    directions = result.JsonToObject<Directions>();

                }
                catch (Exception e)
                {

                    //latLngPoints = new Directions();
                    //throw;
                    Log.Error("Directions", e.Message);
                }

            }
        }

    }
}