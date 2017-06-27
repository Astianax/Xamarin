using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using UK.CO.Chrisjenx.Calligraphy;

namespace RescueMe.Droid
{
    [Activity(Label = "RescueMe.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);

            //CalligraphyConfig.InitDefault(new CalligraphyConfig.Builder()
            //    .SetDefaultFontPath("fonts/OpenSans-Bold.ttf")
            //    .SetFontAttrId(Resource.Attribute.fontPath)
            //    .Build());

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.MyRescues);
            
            //var button = FindViewById<Button>(Resource.Id.btn)
        }

        //protected override void AttachBaseContext(Android.Content.Context @base)
        //{
        //    base.AttachBaseContext(CalligraphyContextWrapper.Wrap(@base));
        //}


    }
}


