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

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "DirectoryActivity", NoHistory = true)]
    public class DirectoryActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Directory);
            SetTools();
           
        }
    }
}