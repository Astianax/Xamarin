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

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "BaseActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class BaseActivity : AppCompatActivity
    {
        protected RestClient _client;
        protected DbContext _context;
        protected bool _isAllowed = true;

        public BaseActivity()
        {
            _client = new RestClient("http://rescueme-api.azurewebsites.net/api/");
            _context = DbContext.Instance;
        }

        protected void SetTools()
        {
            var title = FindViewById<TextView>(Resource.Id.titleID);
            var titleText = FindViewById<TextView>(Resource.Id.titleText);
            var btnBack = FindViewById(Resource.Id.back);

            title.Text = titleText.Text;

            btnBack.Click += btnBack_click;
        }


        protected void btnBack_click(object sender, EventArgs e)
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
                _context.Save(new Settings()
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

        //public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        //{
        //    //switch (requestCode)
        //    //{
        //    //    case 0:
        //    //        {
        //    //            if (grantResults[0] == Permission.Granted)
        //    //            {
        //    //                //Permission granted
        //    //                var snack = Snackbar.Make(message, "Location permission is available, getting lat/long.", Snackbar.LengthShort);
        //    //                snack.Show();

        //    //                //GetLocation();
        //    //            }
        //    //            else
        //    //            {
        //    //                //Permission Denied :(
        //    //                //Disabling location functionality
        //    //                var snack = Snackbar.Make(message, "Location permission is denied.", Snackbar.LengthShort);
        //    //                snack.Show();
        //    //            }
        //    //        }
        //    //        break;
        //    //}
        //}


    }
}