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

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "DirectoryActivity", NoHistory = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class DirectoryActivity : BaseActivity
    {
        ExpandableListViewAdapter mAdapter;
        ExpandableListView expandableListView;
        List<string> group = new List<string>();
        Dictionary<string, List<string>> dicMyMap = new Dictionary<string, List<string>>();

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
                Toast.MakeText(this, "Clicked : " + mAdapter.GetChild(e.GroupPosition, e.ChildPosition), ToastLength.Short).Show();
            };


            SetTools();
        }


        private void SetData(out ExpandableListViewAdapter mAdapter)
        {
            List<string> groupA = new List<string>();
            groupA.Add("A-1");
            groupA.Add("A-2");
            groupA.Add("A-3");

            List<string> groupB = new List<string>();
            groupB.Add("B-1");
            groupB.Add("B-2");
            groupB.Add("B-3");

            group.Add("Group A");
            group.Add("Group B");

            dicMyMap.Add(group[0], groupA);
            dicMyMap.Add(group[1], groupB);

            mAdapter = new ExpandableListViewAdapter(this, group, dicMyMap);

        }
    }
}