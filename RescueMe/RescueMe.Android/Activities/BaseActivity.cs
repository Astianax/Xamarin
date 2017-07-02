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

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "BaseActivity")]
    public class BaseActivity : AppCompatActivity
    {
        protected RestClient _client;
        protected DbContext _context;
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
    }
}