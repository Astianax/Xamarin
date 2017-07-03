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

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "HomeActivity")]
    public class HomeActivity : BaseActivity, IOnMapReadyCallback, ILocationListener
    {
        DrawerLayout drawerLayout;
        NavigationView navigationView;

        private GoogleMap mMap;
        LocationManager locationManager;
        string _provider;
        Location currentLocation = new Location("gps");

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Home);
            var menu = FindViewById(Resource.Id.menuIcon);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

            menu.Click += menu_Click;
            navigationView.NavigationItemSelected += NavigationItemSelected;

            TryGetLocationAsync();



        }



        async Task TryGetLocationAsync()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                GetLocation();
                return;
            }
            else
            {

                SetUp();
                GetLocation();

            }
        }


        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {

            //var message = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.message);
            switch (requestCode)
            {
                case 0:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            //Permission granted :)
                            GetLocation();
                        }
                        else
                        {
                            //Permission Denied :( :(
                            //Disabling location functionality
                            //var snack = Snackbar.Make(message, "No se puede Ubicar por los permisos negados...", Snackbar.LengthIndefinite);
                            //snack.Show();
                        }
                    }
                    break;
            }
        }

        public void GetLocation()
        {


            locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria()
            {
                Accuracy = Accuracy.Fine,
                PowerRequirement = Power.Medium
            };
            _provider = locationManager.GetBestProvider(criteriaForLocationService, true);

            currentLocation = locationManager.GetLastKnownLocation(_provider);
            //locationManager.RequestLocationUpdates(_provider, 0, 0, this);
            if (currentLocation == null)
            {
                //Santo Domingo by Default
                currentLocation.Longitude = 18.5;
                currentLocation.Latitude = -69.983333;
            }

            SetUpMap();
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
            LatLng latlng = new LatLng(currentLocation.Latitude, currentLocation.Longitude);

            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 15);
            MarkerOptions markerOptions = new MarkerOptions()
                                                 .SetPosition(latlng)
                                                 .SetTitle("My Position");


            mMap.AddMarker(markerOptions);
            mMap.UiSettings.ZoomControlsEnabled = true;
            mMap.UiSettings.CompassEnabled = true;
            mMap.MoveCamera(camera);
            //mMap.MoveCamera(CameraUpdateFactory.ZoomIn());
        }


        protected override void OnResume()
        {
            base.OnResume();
            locationManager.RequestLocationUpdates(_provider, 0, 0, this);
        }

        //doesn't need location updates while its Activity is not on the screen
        protected override void OnPause()
        {
            base.OnPause();
            if (locationManager != null)
            {
                locationManager.RemoveUpdates(this);

            }
        }





        // ILocationListener interface Implementation
        public void OnLocationChanged(Location locationUpdated)
        {
            currentLocation.Latitude = locationUpdated.Latitude;
            currentLocation.Longitude = locationUpdated.Longitude;
        }

        public void OnProviderDisabled(string provider)
        {
            Log.Info("", provider + " is not available. Does the device have location services enabled?");
        }

        public void OnProviderEnabled(string provider)
        {

        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            throw new NotImplementedException();
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










