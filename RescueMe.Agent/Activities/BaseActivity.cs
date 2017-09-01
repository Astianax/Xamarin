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
using Android.Content.PM;
using Android;
using System.Text.RegularExpressions;
using Android.Net;
using Android.Telephony;
using Java.Net;
using System.Net;
using RescueMe.Agent.Data;
using Android.Locations;
using Android.Util;

namespace RescueMe.Agent.Activities
{
    [Activity(Label = "BaseActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class BaseActivity : AppCompatActivity
    {
        protected RestClient _client;
        protected DbContext _context;
        protected bool _isAllowed = true;
        ConnectivityManager connectivityManager;
        public static string Url = "http://rescueme-api.azurewebsites.net/api/";
        //public static string Url = "http://192.168.1.14:5000/api/";
        public BaseActivity()
        {
            _client = new RestClient(Url);
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
            var permissitionStatus = false;//ShouldShowRequestPermissionRationale(Manifest.Permission.AccessFineLocation);
            var settings = _context.GetSettings();
            if ((int)Build.VERSION.SdkInt >= 23)
            {

                permissitionStatus = ShouldShowRequestPermissionRationale(Manifest.Permission.AccessFineLocation);
            }
          
          

            string[] PermissionsLocation =
            {
                Manifest.Permission.AccessCoarseLocation,
                Manifest.Permission.AccessFineLocation
            };

            if (settings == null)
            {

                if ((int)Build.VERSION.SdkInt >= 23)
                {
                    RequestPermissions(PermissionsLocation, 0);
                }
                _context.SaveSetting(new Settings()
                {
                    LocationPermission = true,
                    AgentAvailability = true
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

        public static string GetAddress(Location location, Geocoder mGeocoder)
        {
            string mAddress = "";
            try
            {
               

                //The Geocoder class retrieves a list of address from Google over the internet  
                IList<Address> addressList = mGeocoder.GetFromLocationAsync(location.Latitude, location.Longitude, 10).Result;
                Address addressCurrent = addressList.FirstOrDefault();

                if (addressCurrent != null)
                {
                    //StringBuilder deviceAddress = new StringBuilder();

                    //for (int i = 0; i < addressCurrent.MaxAddressLineIndex; i++)
                    ////deviceAddress.Append(addressCurrent.GetAddressLine(i))
                    //.AppendLine(",");

                    //mAddress = deviceAddress.ToString();
                    mAddress = addressCurrent.Locality;
                }
                else
                {

                    mAddress = "Unable to determine the address.";
                }
            }catch(Exception e)
            {
                //Error of Address
            }
            return mAddress;
        }

        protected void SendAgentStatus(Location location, Geocoder mGeocoder)
        {
            bool status = _context.GetSettings().AgentAvailability;
            string city = RescueMe.Agent.Activities.BaseActivity.GetAddress(location, mGeocoder);

            var agentLocation = new
            {
                AgentID = _context.GetUser().UserID,
                City = city,
                Location = new
                {
                    lat = location.Latitude,
                    lng = location.Longitude
                }
            };

            if (status)
            {
                try
                {
                    
                    _client.Post("Agent/update", agentLocation).Result.ToString();
                }
                catch (Exception e)
                {
                    Log.Info("Conexion", "Conexion Problem : "+e.InnerException);
                }
            }
        }
    }
}