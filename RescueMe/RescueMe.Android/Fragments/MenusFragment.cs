
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
using RescueMe.Droid.Data;
using System.Threading;
using System.Text.RegularExpressions;

namespace RescueMe.Droid
{
    public class MenusFragment : Fragment, View.IOnClickListener
    {

        private FloatingActionMenu btnMenu;
        private FloatingActionButton cancelRescue;
        private FloatingActionButton completeRescue;

        private List<FloatingActionMenu> menus = new List<FloatingActionMenu>(6);
        private Handler mUiHandler = new Handler();

        protected RestClient _client;
        protected DbContext _context;

        public void RestClient()
        {
            _client = new RestClient("http://192.168.2.49:5000/api/");
            _context = DbContext.Instance;
            _context.IsNetworkConnected = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.menus_fragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            btnMenu = view.FindViewById<FloatingActionMenu>(Resource.Id.btnMenu);
            cancelRescue = view.FindViewById<FloatingActionButton>(Resource.Id.cancelRescue);
            completeRescue = view.FindViewById<FloatingActionButton>(Resource.Id.completeRescue);
            btnMenu.SetOnMenuButtonClickListener(this);
            btnMenu.SetClosedOnTouchOutside(true);
            btnMenu.HideMenuButton(false);
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            menus.Add(btnMenu);

            cancelRescue.Click += ActionButton_Click;
            completeRescue.Click += ActionButton_Click;
            int delay = 400;
            foreach (var menu in menus)
            {
                mUiHandler.PostDelayed(() => menu.ShowMenuButton(true), delay);
                delay += 150;
            }
            CreateCustomAnimation();
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
                btnMenu.MenuIconView.SetImageResource(!btnMenu.IsOpened ? Resource.Drawable.ic_floating_back : Resource.Drawable.ic_request_menu);
            };

            set.Play(scaleOutX).With(scaleOutY);
            set.Play(scaleInX).With(scaleInY).After(scaleOutX);
            set.SetInterpolator(new OvershootInterpolator(2));

            btnMenu.IconToggleAnimatorSet = set;
        }


        private void ActionButton_Click(object sender, EventArgs e)
        {
            FloatingActionButton fabButton = sender as FloatingActionButton;
            string message = "";
            //Verificar la parte de OffLine

            if (fabButton != null)
            {
                RestClient();

                var request = _context.GetRequest().FirstOrDefault(p => p.Status.Name == "pendiente"
                                                                   || p.Status.Name == "asignado");

                //var requestID =

                if (fabButton.Id == Resource.Id.cancelRescue)
                {
                    new Thread(new ThreadStart(delegate
                    {
                        try
                        {

                            var status = _client.Post("Request/Cancel", new
                            {
                                requestID =request.Id
                            }).Result;
                            message = "Se ha cancelado su solicitud";
                        }
                        catch (Exception ex)
                        {
                            message = ex.Message;
                        }
                        this.Activity.RunOnUiThread(() =>
                        {
                            ///DB Update


                            Toast.MakeText(this.Activity, message, ToastLength.Short).Show();
                        });

                    })).Start();
                }
                else if (fabButton.Id == Resource.Id.completeRescue)
                {
                    if (request.Status.Name == "asignado")
                    {
                        new Thread(new ThreadStart(delegate
                        {
                            try
                            {

                                var status = _client.Post("Request/close", new
                                {
                                    requestID =request.Id
                                }).Result;
                                message = "Se ha completado su solicitud";
                            }
                            catch (Exception ex)
                            {
                                message = ex.Message;
                            }
                            this.Activity.RunOnUiThread(() =>
                            {
                                ///DB Update


                                Toast.MakeText(this.Activity, message, ToastLength.Short).Show();
                            });

                        })).Start();
                    }
                    else
                    {
                        message = "La solicitud debe estar asignada";

                        Toast.MakeText(this.Activity, message, ToastLength.Short).Show();
                    }

                }
                btnMenu.Toggle(animate: true);
            }
        }


        public void OnClick(View v)
        {
            FloatingActionMenu menu = (FloatingActionMenu)v.Parent;
            menu.Toggle(animate: true);
        }

    }
}

