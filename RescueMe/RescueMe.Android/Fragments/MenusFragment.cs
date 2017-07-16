
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

using FloatingActionButton = Clans.Fab.FloatingActionButton;
using FloatingActionMenu = Clans.Fab.FloatingActionMenu;
using Fragment = Android.Support.V4.App.Fragment;
using Android;

namespace RescueMe.Droid
{
    public class MenusFragment : Fragment, View.IOnClickListener
    {

        private FloatingActionMenu btnMenu;
        private FloatingActionButton cancelRescue;
        private FloatingActionButton completeRescue;
        
        private List<FloatingActionMenu> menus = new List<FloatingActionMenu> (6);
        private Handler mUiHandler = new Handler ();


        public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate (Resource.Layout.menus_fragment, container, false);
        }

        public override void OnViewCreated (View view, Bundle savedInstanceState)
        {
            base.OnViewCreated (view, savedInstanceState);

            btnMenu = view.FindViewById<FloatingActionMenu> (Resource.Id.btnMenu);
            cancelRescue = view.FindViewById<FloatingActionButton> (Resource.Id.cancelRescue);
            completeRescue = view.FindViewById<FloatingActionButton> (Resource.Id.completeRescue);
            btnMenu.SetOnMenuButtonClickListener (this);
            btnMenu.SetClosedOnTouchOutside (true);
            btnMenu.HideMenuButton (false);
        }

        public override void OnActivityCreated (Bundle savedInstanceState)
        {
            base.OnActivityCreated (savedInstanceState);
            
            menus.Add (btnMenu);

            cancelRescue.Click += ActionButton_Click;
            completeRescue.Click += ActionButton_Click;
            int delay = 400;
            foreach (var menu in menus) 
            {
                mUiHandler.PostDelayed (() => menu.ShowMenuButton (true), delay);
                delay += 150;
            }
            CreateCustomAnimation ();
        }


        private void CreateCustomAnimation()
        {
            AnimatorSet set = new AnimatorSet();

            ObjectAnimator scaleOutX = ObjectAnimator.OfFloat(btnMenu.MenuIconView, "scaleX", 1.0f, 0.2f);
            ObjectAnimator scaleOutY = ObjectAnimator.OfFloat(btnMenu.MenuIconView, "scaleY", 1.0f, 0.2f);

            ObjectAnimator scaleInX = ObjectAnimator.OfFloat(btnMenu.MenuIconView, "scaleX", 0.2f, 1.0f);
            ObjectAnimator scaleInY = ObjectAnimator.OfFloat(btnMenu.MenuIconView, "scaleY", 0.2f, 1.0f);

            scaleOutX.SetDuration(50);
            scaleOutY.SetDuration(50);

            scaleInX.SetDuration(150);
            scaleInY.SetDuration(150);

            scaleInX.AnimationStart += (object sender, EventArgs e) =>
            {
                btnMenu.MenuIconView.SetImageResource(!btnMenu.IsOpened ? Resource.Drawable.ic_close : Resource.Drawable.ic_request_menu);
            };

            set.Play(scaleOutX).With(scaleOutY);
            set.Play(scaleInX).With(scaleInY).After(scaleOutX);
            set.SetInterpolator(new OvershootInterpolator(2));

            btnMenu.IconToggleAnimatorSet = set;
        }


        private void ActionButton_Click (object sender, EventArgs e)
        {
            FloatingActionButton fabButton = sender as FloatingActionButton;
            if (fabButton != null) 
            {
                //if (fabButton.Id == Resource.Id.fab2) 
                //{
                //    fabButton.Visibility = ViewStates.Gone;
                //} 
                //else if (fabButton.Id == Resource.Id.fab3) 
                //{
                //    fabButton.Visibility = ViewStates.Visible;
                //}
                //Toast.MakeText (this.Activity, fabButton.LabelText, ToastLength.Short).Show ();
            }
        }


        public void OnClick (View v)
        {
            FloatingActionMenu menu = (FloatingActionMenu)v.Parent;
            menu.Toggle (animate: true);
        }

    }
}

