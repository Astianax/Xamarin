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
using Android.Support.V4.Widget;
using Android.Support.Design.Widget;
using Android.Support.V4.View;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "HomeActivity")]
    public class HomeActivity : BaseActivity
    {
        DrawerLayout drawerLayout;
        NavigationView navigationView;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Home);
            var menu = FindViewById(Resource.Id.menuIcon);
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

            menu.Click += menu_Click;
            navigationView.NavigationItemSelected += NavigationItemSelected;


        }



        private void menu_Click(object sender, EventArgs e)
        {
            if (drawerLayout.IsDrawerOpen(GravityCompat.Start))
            {
                drawerLayout.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                drawerLayout.OpenDrawer(GravityCompat.Start);
            }
        }

        private void NavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            var menuItem = e.MenuItem;
            menuItem.SetChecked(!menuItem.IsChecked);
            drawerLayout.CloseDrawer(GravityCompat.Start);
            Intent intent;
            switch (menuItem.ItemId)
            {
                case Resource.Id.nav_profile:
                    intent = new Intent(this, typeof(ProfileActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.nav_rescue:
                    intent = new Intent(this, typeof(RescueActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.nav_directory:
                    intent = new Intent(this, typeof(DirectoryActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.nav_about:
                    intent = new Intent(this, typeof(AboutActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.nav_logOut:
                    //intent = new Intent(this, typeof(ProfileActivity));
                    //StartActivity(intent);
                    break;
            }
        }



        public override void OnBackPressed()
        {
            drawerLayout.CloseDrawer(GravityCompat.Start);
            return;
        }
    }
}