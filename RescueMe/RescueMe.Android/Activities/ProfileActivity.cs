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

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "ProfileActivity")]
    public class ProfileActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Profile);
            SetTools();
            // Create your application here


            //Controls
            var btnCars = FindViewById<ImageView>(Resource.Id.btnCars);
            btnCars.Click += btnCars_click;
        }

        private void btnCars_click(object sender, EventArgs e)
        {

            StartActivity(new Intent(Application.Context, typeof(CarsActivity)));
        }
    }
}