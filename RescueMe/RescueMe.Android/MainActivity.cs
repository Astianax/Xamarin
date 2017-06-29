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

namespace RescueMe.Droid
{
    [Activity(Label = "RescueMe.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : BaseActivity
    {
       
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //CalligraphyConfig.InitDefault(new CalligraphyConfig.Builder()
            //    .SetDefaultFontPath("fonts/OpenSans-Bold.ttf")
            //    .SetFontAttrId(Resource.Attribute.fontPath)
            //    .Build());

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Login);

            //Controls
            var btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            btnLogin.Click += BtnLogin_Click;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            var txtEmail = FindViewById<EditText>(Resource.Id.txtEmail);
            var txtPassword = FindViewById<EditText>(Resource.Id.txtPassword);
            var chkRememberMe = FindViewById<CheckBox>(Resource.Id.chkRememberMe);
            var userViewModel = new UserViewModel();
            userViewModel.email = "firulais@gmail.com";//txtEmail.Text;
            userViewModel.password = "hello123456";//txtPassword.Text.ToString();
            userViewModel.platform = "web";

            UserProfile user = _client.Post("Authentication/IsAuthenticated", userViewModel).Result.JsonToObject<UserProfile>();
            if (user != null)
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            }

        }

    }
}


