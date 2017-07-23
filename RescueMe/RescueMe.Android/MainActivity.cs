using System;

using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using RescueMe.Api.ViewModel;
using RescueMe.Domain;
using RescueMe.Droid.Activities;
using Android.Support.Design.Widget;
using System.Threading;
using Android.Content.PM;
using System.Threading.Tasks;
using System.Collections.Generic;
using Firebase.Iid;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.Views;
using Android.Gms.Plus;
using Android.Util;
using Android.Gms.Auth.Api;
namespace RescueMe.Droid
{
    [Activity(Label = "Rescate Vial", Icon = "@drawable/appIcon", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    //[Activity(Label = "Leftdrawerlayout", Theme = "@style/Theme.DesignDemo", MainLauncher = true, Icon = "@drawable/icon")]

    public class MainActivity : BaseActivity,
        GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {

        Button btnLogin;
        const string TAG = "MainActivity";
        const string KEY_IS_RESOLVING = "is_resolving";
        const string KEY_SHOULD_RESOLVE = "should_resolve";

        private string token;
        const int RC_SIGN_IN = 9001;
        GoogleApiClient mGoogleApiClient;
        //TextView mStatus;
        bool mIsResolving = false;
        bool mShouldResolve = false;

        protected async override void OnCreate(Bundle bundle)
        {
            //Informations

            base.OnCreate(bundle);
            if (bundle != null)
            {
                mIsResolving = bundle.GetBoolean(KEY_IS_RESOLVING);
                mShouldResolve = bundle.GetBoolean(KEY_SHOULD_RESOLVE);
            }

            if (!GetString(Resource.String.google_app_id).Equals("1:851005322260:android:6288a966f5369538"))
                throw new System.Exception("Invalid Json file");

            mGoogleApiClient = new GoogleApiClient.Builder(this)
                    .AddConnectionCallbacks(this)
                    .AddOnConnectionFailedListener(this)
                    .AddApi(PlusClass.API)
                    .AddScope(new Scope(Scopes.Profile))
                    .Build();


            if (_context.GetUser() == null)
            {

                SetContentView(Resource.Layout.Login);
                //Controls
                btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
                var linkRegister = FindViewById<TextView>(Resource.Id.linkRegister);
                btnLogin.Click += BtnLogin_Click;
                linkRegister.Click += linkRegister_click;
                SetUp();

                var SignInGoogle = FindViewById<ImageButton>(Resource.Id.sign_in_button);
                SignInGoogle.Click += SignInGoogle_click;
                FindViewById(Resource.Id.linkRegister).Enabled = true;
            }
            else
            {
                StartActivity(new Intent(Application.Context, typeof(HomeActivity)));
            }



        }

        private void SignInGoogle_click(object sender, EventArgs e)
        {
            mShouldResolve = true;
            mGoogleApiClient.Connect();
        }

        private void linkRegister_click(object sender, EventArgs e)
        {
            FindViewById(Resource.Id.linkRegister).Enabled = false;
            StartActivity(new Intent(Application.Context, typeof(RegisterActivity)));
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {


            bool valid = true;
            var txtEmail = FindViewById<TextInputEditText>(Resource.Id.txtEmail);
            var emailLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.emaiLayout);

            var txtPassword = FindViewById<TextInputEditText>(Resource.Id.txtPassword);
            var passwordLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.passwordLayout);

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
                userViewModel.email = txtEmail.Text;
                userViewModel.password = txtPassword.Text.ToString();
                SignIn(userViewModel, false);
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
                vehicles = _client.Get("Vehicle/vehicles", user).Result.JsonToObject<List<Vehicle>>();
            }
            catch (Exception ex)
            {
                vehicles = null;
            }

            return vehicles;
        }

        public List<Request> GetRescues(UserProfile user)
        {
            List<Request> requests;
            try
            {
                requests = _client.Get("Request/requests", new
                {
                    UserId = user.UserID,
                    platform = "client"
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

        public void SignIn(UserViewModel userViewModel, bool isGoogle)
        {
            UserProfile user = null;

            var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "Validando Información...");
            progressDialog.Indeterminate = true;
            progressDialog.SetCancelable(false);

            var txtPassword = FindViewById<TextInputEditText>(Resource.Id.txtPassword);
            var passwordLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.passwordLayout);

            new Thread(new ThreadStart(delegate
            {
                var instanceId = FirebaseInstanceId.Instance;
                //instanceId.DeleteInstanceId();
                Android.Util.Log.Debug("TAG", "{0}. {1}", instanceId.Token, instanceId.GetToken(GetString(Resource.String.gcm_defaultSenderId),
                    Firebase.Messaging.FirebaseMessaging.InstanceIdScope));

                userViewModel.token = instanceId.Token;
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
                    var reasons = GetReasons();
                    var rescues = GetRescues(user);
                    var status = GetStatus();
                    _context.LogIn(user, vehicles, reasons, rescues, status);
                    Intent intent = new Intent(this, typeof(HomeActivity));
                    StartActivity(intent);
                }
                else
                {

                    if (isGoogle)
                    {
                        RegisterToGoogle(userViewModel);
                    }
                    else
                    {
                        Snackbar.Make(passwordLayout, "Usuario o contraseña inválido.", Snackbar.LengthLong)
                                .SetAction("OK", (v) => { txtPassword.Text = String.Empty; })
                                .SetDuration(8000)
                                .SetActionTextColor(Android.Graphics.Color.Orange)
                                .Show();
                    }
                }
                //HIDE PROGRESS DIALOG
                RunOnUiThread(() => progressDialog.Hide());

            })).Start();
        }


        public void RegisterToGoogle(UserViewModel profile)
        {
            UserProfile userProfile = new UserProfile();
            User user = null;

            userProfile.Name = profile.name;
            userProfile.Email = profile.email;
            userProfile.User = new User
            {
                PassworDigest = profile.password
            };

            try
            {
                if (IsNetworkConnected())
                {
                    user = _client.Post("Authentication/create", userProfile).Result.JsonToObject<User>();
                }
                else
                {
                    Log.Info(TAG, GetString(Resource.String.not_connection));
                }
            }
            catch (Exception ex)
            {
                user = null;
                Log.Info(TAG, ex.Message);
            }


            if (user != null)
            {
                //Set User generated
                var vehicles = GetVehicles(user.Id);
                var reasons = GetReasons();
                var rescues = GetRescues(userProfile);
                var status = GetStatus();
                userProfile.User = user;
                _context.LogIn(userProfile, vehicles, reasons, rescues, status);
                //_context.LogIn(userProfile, null, null);
                Intent intent = new Intent(this, typeof(HomeActivity));
                StartActivity(intent);
            }



        }


        void UpdateUI(bool isSignedIn)
        {
            if (isSignedIn)
            {
                var userViewModel = new UserViewModel();
                var person = PlusClass.PeopleApi.GetCurrentPerson(mGoogleApiClient);
                userViewModel.email = PlusClass.AccountApi.GetAccountName(mGoogleApiClient);
                userViewModel.password = person.Id;
                userViewModel.name = person.DisplayName;
                SignIn(userViewModel, true);


            }
        }


        protected override void OnStart()
        {
            base.OnStart();
            if (_context.GetUser() == null)
            {
                FindViewById(Resource.Id.linkRegister).Enabled = true;
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            mGoogleApiClient.Disconnect();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutBoolean(KEY_IS_RESOLVING, mIsResolving);
            outState.PutBoolean(KEY_SHOULD_RESOLVE, mIsResolving);
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Log.Debug(TAG, "onActivityResult:" + requestCode + ":" + resultCode + ":" + data);

            if (requestCode == RC_SIGN_IN)
            {
                if (resultCode != Result.Ok)
                {
                    mShouldResolve = false;
                }

                mIsResolving = false;
                mGoogleApiClient.Connect();
            }
        }







        public void OnConnectionFailed(ConnectionResult result)
        {
            Log.Debug(TAG, "onConnectionFailed:" + result);

            if (!mIsResolving && mShouldResolve)
            {
                if (result.HasResolution)
                {
                    try
                    {
                        result.StartResolutionForResult(this, RC_SIGN_IN);
                        mIsResolving = true;
                    }
                    catch (IntentSender.SendIntentException e)
                    {
                        Log.Error(TAG, "Could not resolve ConnectionResult.", e);
                        mIsResolving = false;
                        mGoogleApiClient.Connect();
                    }
                }
                else
                {
                    ShowErrorDialog(result);
                }
            }
            else
            {
                UpdateUI(false);
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            Log.Debug(TAG, "onConnected:" + connectionHint);
            UpdateUI(true);
        }

        public void OnConnectionSuspended(int cause)
        {
            Log.Warn(TAG, "onConnectionSuspended:" + cause);
        }

        class DialogInterfaceOnCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
        {
            public Action<IDialogInterface> OnCancelImpl { get; set; }

            public void OnCancel(IDialogInterface dialog)
            {
                OnCancelImpl(dialog);
            }
        }
        void ShowErrorDialog(ConnectionResult connectionResult)
        {
            int errorCode = connectionResult.ErrorCode;

            if (GooglePlayServicesUtil.IsUserRecoverableError(errorCode))
            {
                var listener = new DialogInterfaceOnCancelListener();
                listener.OnCancelImpl = (dialog) =>
                {
                    mShouldResolve = false;
                    UpdateUI(false);
                };
                GooglePlayServicesUtil.GetErrorDialog(errorCode, this, RC_SIGN_IN, listener).Show();
            }
            else
            {
                Log.Info(TAG, errorCode.ToString());

                mShouldResolve = false;
                UpdateUI(false);
            }
        }
    }
}