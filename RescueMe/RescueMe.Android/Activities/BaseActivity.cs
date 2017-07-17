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
using Android.Support.V7.App;
using RescueMe.Droid.Data;
using Android.Content.PM;
using Android;
using System.Text.RegularExpressions;
using Android.Net;
using Android.Telephony;
using Java.Net;
using System.Net;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "BaseActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class BaseActivity : AppCompatActivity
    {
        protected RestClient _client;
        protected DbContext _context;
        protected bool _isAllowed = true;
        ConnectivityManager connectivityManager;

        public BaseActivity()
        {
            _client = new RestClient("http://rescueme-api.azurewebsites.net/api/");
           // _client = new RestClient("http://192.168.2.49:5000/api/");
            _context = DbContext.Instance;
            _context.IsNetworkConnected = true;
        }

        protected void SetTools()
        {
            var title = FindViewById<TextView>(Resource.Id.titleID);
            var titleText = FindViewById<TextView>(Resource.Id.titleText);
            var btnBack = FindViewById(Resource.Id.back);

            title.Text = titleText.Text;

            btnBack.Click += BtnBack_click;
            _context.IsNetworkConnected = IsNetworkConnected();
        }


        protected void BtnBack_click(object sender, EventArgs e)
        {
            this.Finish();
        }

        protected void SetUp()
        {
            var permissitionStatus = ShouldShowRequestPermissionRationale(Manifest.Permission.AccessFineLocation);
            var settings = _context.GetSettings();

            string[] PermissionsLocation =
            {
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation
            };

            if (settings == null)
            {

                RequestPermissions(PermissionsLocation, 0);
                _context.SaveSetting(new Settings()
                {
                    LocationPermission = true
                });
                _isAllowed = false;
            }
            else if (permissitionStatus)
            {
                RequestPermissions(PermissionsLocation, 0);
                _isAllowed = false;
            }

        }

        protected bool IsNetworkConnected()
        {
            connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            NetworkInfo networkInfo = connectivityManager.ActiveNetworkInfo;
            bool isOnline = false;

            if (networkInfo != null)
            {
                if (networkInfo.Type == ConnectivityType.Wifi && networkInfo.IsConnected || (networkInfo.Type == ConnectivityType.Mobile && networkInfo.IsConnected))
                {
                    isOnline = ActiveInternetConnectivity();
                }
            }

            return isOnline;
        }
        
        public bool ActiveInternetConnectivity()
        {
            string CheckUrl = "http://google.com";

            try
            {
                HttpWebRequest iNetRequest = (HttpWebRequest)WebRequest.Create(CheckUrl);

                iNetRequest.Timeout = 5000;
                WebResponse iNetResponse = iNetRequest.GetResponse();
                iNetResponse.Close();

                return true;

            }
            catch (WebException ex)
            {
                return false;
            }
        }


        protected override void OnStart()
        {
        
            base.OnStart();
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            MobileCenter.Start("feb54400-4cbc-4f14-b342-6791df4f72f0", typeof(Analytics), typeof(Crashes));
            //MobileCenter.Start("feb54400-4cbc-4f14-b342-6791df4f72f0", typeof(Analytics), typeof(Crashes));
            base.OnCreate(savedInstanceState);
        }

    }
}