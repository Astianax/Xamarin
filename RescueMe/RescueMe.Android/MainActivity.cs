using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using UK.CO.Chrisjenx.Calligraphy;
using RescueMe.Api.ViewModel;
using RescueMe.Domain;
using RescueMe.Droid.Activities;
using Android.Support.Design.Widget;
using System.Threading;
using RescueMe.Droid.Data;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Content.Res;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RescueMe.Droid
{
    [Activity(Label = "Rescate Vial", Icon = "@drawable/appIcon", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    //[Activity(Label = "Leftdrawerlayout", Theme = "@style/Theme.DesignDemo", MainLauncher = true, Icon = "@drawable/icon")]

    public class MainActivity : BaseActivity
    {



        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //CalligraphyConfig.InitDefault(new CalligraphyConfig.Builder()
            //    .SetDefaultFontPath("fonts/OpenSans-Bold.ttf")
            //    .SetFontAttrId(Resource.Attribute.fontPath)
            //    .Build());
            //SetContentView(Resource.Layout.Login);
            // Set our view from the "main" layout resource
            //SetContentView(Resource.Layout.Profile);
            //var btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            //btnLogin.Click += BtnLogin_Click;
            //var linkRegister = FindViewById<TextView>(Resource.Id.card_v);
            //linkRegister.Click += linkRegister_click;

            if (_context.GetUser() == null)
            {
                SetContentView(Resource.Layout.Login);
                //Controls
                var btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
                var linkRegister = FindViewById<TextView>(Resource.Id.linkRegister);
                btnLogin.Click += BtnLogin_Click;
                linkRegister.Click += linkRegister_click;

                SetUp();
            }
            else
            {
                StartActivity(new Intent(Application.Context, typeof(HomeActivity)));
            }

        }


        private void linkRegister_click(object sender, EventArgs e)
        {
            StartActivity(new Intent(Application.Context, typeof(RegisterActivity)));
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            bool valid = true;
            var txtEmail = FindViewById<TextInputEditText>(Resource.Id.txtEmail);
            var emailLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.emaiLayout);

            var txtPassword = FindViewById<TextInputEditText>(Resource.Id.txtPassword);
            var passwordLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.passwordLayout);
            //var loginLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.loginLayout);


            //var chkRememberMe = FindViewById<CheckBox>(Resource.Id.chkRememberMe);
            var userViewModel = new UserViewModel();


            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                emailLayout.ErrorEnabled = true;
                emailLayout.Error = GetString(Resource.String.required_error_message);
                valid = false;
            }
            else
            {
                if (!txtEmail.Text.IsValidEmail())
                {
                    emailLayout.ErrorEnabled = true;
                    emailLayout.Error = GetString(Resource.String.invalid_email);
                    valid = false;
                }
                else
                {
                    emailLayout.ErrorEnabled = false;
                }
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                passwordLayout.ErrorEnabled = true;
                passwordLayout.Error = GetString(Resource.String.required_error_message);
                valid = false;
            }
            else
            {
                passwordLayout.ErrorEnabled = false;
            }




            if (valid)
            {
                UserProfile user = null;
                userViewModel.email = "firulais@gmail.com";//txtEmail.Text;
                userViewModel.password = "hello123456";//txtPassword.Text.ToString();
                userViewModel.platform = "web";


                var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "Validando Información...");
                progressDialog.Indeterminate = true;
                progressDialog.SetCancelable(false);

              
                new Thread(new ThreadStart(delegate
                {
                    //LOAD METHOD TO GET ACCOUNT INFO
                    user = _client.Post("Authentication/IsAuthenticated", userViewModel).Result.JsonToObject<UserProfile>();

                    if (user != null)
                    {
                        //Save Database
                        //_context.Save<UserSaved>(new UserSaved()
                        //{
                        //    Email = user.Email,
                        //    Id = user.Id,
                        //    FullName = user.Name,
                        //    Password = user.User.PassworDigest
                        //});
                     
                        //Save Vehicles
                        var vehicles = GetVehicles(user.Id);
                        _context.LogIn(user, vehicles);
                        Intent intent = new Intent(this, typeof(HomeActivity));
                        StartActivity(intent);
                    }
                    else
                    {
                        Snackbar.Make(passwordLayout, "Usuario o contraseña inválido.", Snackbar.LengthLong)
                                .SetAction("OK", (v) => { txtPassword.Text = String.Empty; })
                                .SetDuration(8000)
                                .SetActionTextColor(Android.Graphics.Color.Orange)
                                .Show();
                    }
                    //HIDE PROGRESS DIALOG
                    RunOnUiThread(() => progressDialog.Hide());

                })).Start();

            }

        }
        public List<Vehicle> GetVehicles(int userID)
        {
            List<Vehicle> vehicles = new List<Vehicle>();

            var user = new
            {
                UserID = userID
            };
            try
            {
                vehicles =  _client.Get("Vehicle/vehicles", user).Result.JsonToObject<List<Vehicle>>();
            }
            catch (Exception ex)
            {
                vehicles = null;
            }

            return vehicles;
        }
    }
}