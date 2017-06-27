using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace RescueMe.Droid
{
    [Activity(Label = "RescueMe.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Profile);
            
            //var button = FindViewById<Button>(Resource.Id.btn)
        }

        private void OnClick()
        {

        }
    }
}


