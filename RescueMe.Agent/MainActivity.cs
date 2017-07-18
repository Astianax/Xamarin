using System;

using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using RescueMe.Api.ViewModel;
using RescueMe.Domain;
using Android.Support.Design.Widget;
using System.Threading;
using Android.Content.PM;
using System.Threading.Tasks;
using System.Collections.Generic;
using Firebase.Iid;
using RescueMe.Agent.Activities;

namespace RescueMe.Agent
{
    [Activity(Label = "Agent Rescate Vial ", Icon = "@drawable/appIcon", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]

    public class MainActivity : BaseActivity
    {

        Button btnLogin;
        const string TAG = "MainActivity";
        private string token;
        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);
            
            if (!GetString(Resource.String.google_app_id).Equals("1:851005322260:android:dc4df3d3160f0f1d"))
                throw new System.Exception("Invalid Json file");

            Task.Run(() =>
            {
                var instanceId = FirebaseInstanceId.Instance;
                Android.Util.Log.Debug("TAG", "{0}. {1}", instanceId.Token, instanceId.GetToken(GetString(Resource.String.gcm_defaultSenderId),
                    Firebase.Messaging.FirebaseMessaging.InstanceIdScope));

                token = instanceId.Token;
            });

            if (_context.GetUser() == null)
            {
                SetContentView(Resource.Layout.Login);
                //Controls
                btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
                btnLogin.Click += BtnLogin_Click;

                SetUp();
            }
            else
            {
                StartActivity(new Intent(Application.Context, typeof(HomeActivity)));
            }

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
                userViewModel.email = "firulaisp@gmail.com";//txtEmail.Text;
                userViewModel.password = "hello123456";//txtPassword.Text.ToString();
                userViewModel.token = token;
                userViewModel.platform = "agent";


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
                        var rescues = GetRescues(user);
                        var status = GetStatus();
                        var reasons = GetReasons();
                        _context.LogIn(user, reasons, rescues, status);
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

        public List<Request> GetRescues(UserProfile user)
        {
            List<Request> requests;
            try
            {
                requests = _client.Get("Request/requests", new
                {
                    UserId = user.UserID,
                    platform = "mobile"
                }
                        ).Result.JsonToObject<List<Request>>();
            }
            catch (Exception ex)
            {
                requests = null;
            }

            return requests;
        }
        public List<ReasonRequest> GetReasons()
        {
            List<ReasonRequest> reasons;
            try
            {
                reasons = _client.Get("Request/reasons", null).Result.JsonToObject<List<ReasonRequest>>();
            }
            catch (Exception ex)
            {
                reasons = null;
            }

            return reasons;
        }
        public List<Status> GetStatus()
        {
            List<Status> status;
            try
            {
                status = _client.Get("Status/", null).Result.JsonToObject<List<Status>>();
            }
            catch (Exception ex)
            {
                status = null;
            }

            return status;
        }
    }
}