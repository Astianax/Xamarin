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
using Android.Content.PM;
using RescueMe.Droid.Adapters;
using RescueMe.Domain;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "DirectoryActivity", NoHistory = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class DirectoryActivity : BaseActivity
    {
        ExpandableListViewAdapter mAdapter;
        ExpandableListView expandableListView;
        List<string> group = new List<string>();
        Dictionary<string, List<SoSDirectory>> dicMyMap = new Dictionary<string, List<SoSDirectory>>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

         

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Directory);
            expandableListView = FindViewById<ExpandableListView>(Resource.Id.expandableListView);

            //Set Data
            SetData(out mAdapter);
            expandableListView.SetAdapter(mAdapter);

            expandableListView.ChildClick += (s, e) => {
                //Toast.MakeText(this, "Clicked : " + mAdapter.GetBusiness(e.GroupPosition, e.ChildPosition).TelephoneNumber, ToastLength.Short).Show();
                 var uri = Android.Net.Uri.Parse("tel:" + mAdapter.GetBusiness(e.GroupPosition, e.ChildPosition).TelephoneNumber);
                Intent callIntent = new Intent(Intent.ActionDial, uri);
                StartActivity(callIntent);
            };


            SetTools();
        }


        private void SetData(out ExpandableListViewAdapter mAdapter)
        {
            var list = _client.Get("Directory", null).Result.JsonToObject<List<SoSDirectory>>();

            var groups = list.GroupBy(g => g.Category);
            foreach (var category in groups)
            {
                group.Add(category.Key);
                dicMyMap.Add(category.Key, category.ToList());
            }
            mAdapter = new ExpandableListViewAdapter(this, group, dicMyMap);

        }
    }
}