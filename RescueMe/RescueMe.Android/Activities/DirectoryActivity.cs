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
using Android.Views.Animations;
using Android.Views.InputMethods;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "DirectoryActivity", NoHistory = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class DirectoryActivity : BaseActivity
    {
        ExpandableListViewAdapter mAdapter;
        ExpandableListView expandableListView;
        List<string> group = new List<string>();
        Dictionary<string, List<SoSDirectory>> dicMyMap;
        private List<SoSDirectory> directory;
        private EditText mSearch;
        private Button btnSearch;

        private LinearLayout mContainer;
        private bool mAnimatedDown;
        private bool mIsAnimating;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);



            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Directory);
            expandableListView = FindViewById<ExpandableListView>(Resource.Id.expandableListView);
            //expandableListView = FindViewById<ExpandableListView>(Resource.Id.listView);
            mContainer = FindViewById<LinearLayout>(Resource.Id.llContainer);
            btnSearch = FindViewById<Button>(Resource.Id.search);
            

            mSearch = FindViewById<EditText>(Resource.Id.etSearch);
            var btnBack = FindViewById(Resource.Id.back);




            mSearch.Alpha = 0;
            mContainer.BringToFront();
            mSearch.TextChanged += mSearch_TextChanged;
            btnSearch.Click += btnSearch_click;
            btnBack.Click += BtnBack_click;
            //Set Data
            SetData(out mAdapter);
            expandableListView.SetAdapter(mAdapter);

            expandableListView.ChildClick += (s, e) =>
            {
                //Toast.MakeText(this, "Clicked : " + mAdapter.GetBusiness(e.GroupPosition, e.ChildPosition).TelephoneNumber, ToastLength.Short).Show();
                var uri = Android.Net.Uri.Parse("tel:" + mAdapter.GetBusiness(e.GroupPosition, e.ChildPosition).TelephoneNumber);
                Intent callIntent = new Intent(Intent.ActionDial, uri);
                StartActivity(callIntent);
            };

        }

        private void btnSearch_click(object sender, EventArgs e)
        {

            mSearch.Visibility = ViewStates.Visible;

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            if (!mAnimatedDown)
            {
                //Listview is up
                SearchAnimation anim = new SearchAnimation(expandableListView, expandableListView.Height - mSearch.Height);
                anim.Duration = 500;
                expandableListView.StartAnimation(anim);
                anim.AnimationStart += anim_AnimationStartDown;
                anim.AnimationEnd += anim_AnimationEndDown;
                mContainer.Animate().TranslationYBy(mSearch.Height).SetDuration(500).Start();
            }

            else
            {
                //Listview is down
                SearchAnimation anim = new SearchAnimation(expandableListView, expandableListView.Height + mSearch.Height);
                anim.Duration = 500;
                expandableListView.StartAnimation(anim);
                anim.AnimationStart += anim_AnimationStartUp;
                anim.AnimationEnd += anim_AnimationEndUp;
                mContainer.Animate().TranslationYBy(-mSearch.Height).SetDuration(500).Start();
            }

            mAnimatedDown = !mAnimatedDown;

        }

        void anim_AnimationEndUp(object sender, Android.Views.Animations.Animation.AnimationEndEventArgs e)
        {
            mIsAnimating = false;
            mSearch.ClearFocus();
            InputMethodManager inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            inputManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
        }

        void anim_AnimationEndDown(object sender, Android.Views.Animations.Animation.AnimationEndEventArgs e)
        {
            mIsAnimating = false;
        }

        void anim_AnimationStartDown(object sender, Android.Views.Animations.Animation.AnimationStartEventArgs e)
        {
            mIsAnimating = true;
            mSearch.Animate().AlphaBy(1.0f).SetDuration(500).Start();
        }

        void anim_AnimationStartUp(object sender, Android.Views.Animations.Animation.AnimationStartEventArgs e)
        {
            mIsAnimating = true;
            mSearch.Animate().AlphaBy(-1.0f).SetDuration(300).Start();
        }
        private void SetData(out ExpandableListViewAdapter mAdapter)
        {
            directory = _client.Get("Directory", null).Result.JsonToObject<List<SoSDirectory>>();

            var groups = directory.GroupBy(g => g.Category);
            dicMyMap = new Dictionary<string, List<SoSDirectory>>();
            foreach (var category in groups)
            {
                group.Add(category.Key);
                dicMyMap.Add(category.Key, category.ToList());
            }
            mAdapter = new ExpandableListViewAdapter(this, group, dicMyMap);

        }


        void mSearch_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            var filters = directory.Where(d => d.Name!=null &&  d.Name.ToLower().Contains(mSearch.Text.ToLower())).ToList();
            var groups = filters.GroupBy(g => g.Category);
            dicMyMap = new Dictionary<string, List<SoSDirectory>>();
            group = new List<string>();
            foreach (var category in groups)
            {
                group.Add(category.Key);
                dicMyMap.Add(category.Key, category.ToList());
            }
            var mAdapterNew = new ExpandableListViewAdapter(this, group, dicMyMap);
            expandableListView.SetAdapter(mAdapterNew);
        }

        
    }
}