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
using Android.Support.Design.Widget;
using RescueMe.Domain;
using System.Threading;
using Android.Content.PM;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "CarsActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class CarsActivity : BaseActivity
    {


        TextInputEditText txType;
        TextInputEditText txtMarque;

        Android.Support.Design.Widget.TextInputLayout typeLayout;
        Android.Support.Design.Widget.TextInputLayout marqueLayout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MyCars);
            SetTools();

            //Controls
            var btnAddCars= FindViewById<Button>(Resource.Id.btnAddCars);

            txType = FindViewById<TextInputEditText>(Resource.Id.txType);
            txtMarque = FindViewById<TextInputEditText>(Resource.Id.txtMarque);


            typeLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.typeLayout);
            marqueLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.marqueLayout);

            btnAddCars.Click += btnAddCars_click;



        }

        private void btnAddCars_click(object sender, EventArgs e)
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txType.Text))
            {
                typeLayout.ErrorEnabled = true;
                typeLayout.Error = GetString(Resource.String.required_error_message);
                valid = false;
            }
            else
            {
                typeLayout.ErrorEnabled = false;
            }
            if (string.IsNullOrWhiteSpace(txtMarque.Text))
            {
                marqueLayout.ErrorEnabled = true;
                marqueLayout.Error = GetString(Resource.String.required_error_message);
                valid = false;
            }
            else
            {
                marqueLayout.ErrorEnabled = false;
            }



            if (valid)
            {
                UserProfile user = new UserProfile();
                //user.User.

            }

        }

    }
}