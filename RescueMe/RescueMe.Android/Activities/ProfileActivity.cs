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
using Android.Support.Design.Widget;
using RescueMe.Api.ViewModel;
using RescueMe.Domain;
using System.Threading;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "ProfileActivity")]
    public class ProfileActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Profile);
            SetTools();
            // Create your application here


            //Controls
            var btnCars = FindViewById<ImageView>(Resource.Id.btnCars);
            var btnSave = FindViewById<Button>(Resource.Id.btnSave);
            btnCars.Click += btnCars_click;
            btnSave.Click += btnSave_click;
        }

        private void btnSave_click(object sender, EventArgs e)
        {
            bool valid = true;
            var txtName = FindViewById<TextInputEditText>(Resource.Id.txtName);
            var nameLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.nameLayout);

            var txtCedula = FindViewById<TextInputEditText>(Resource.Id.txtCedula);
            //var cedulaLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.cedulaLayout);

            var txTelefono = FindViewById<TextInputEditText>(Resource.Id.txTelefono);
            //var telefonoLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.telefonoLayout);

            var txtPassword = FindViewById<TextInputEditText>(Resource.Id.txtPassword);
            var passwordLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.passwordLayout);
            


            //if (string.IsNullOrWhiteSpace(txtName.Text))
            //{
            //    nameLayout.ErrorEnabled = true;
            //    nameLayout.Error = GetString(Resource.String.required_error_message);
            //    valid = false;
            //}
            //if (string.IsNullOrWhiteSpace(txtCedula.Text))
            //{
            //    cedulaLayout.ErrorEnabled = true;
            //    cedulaLayout.Error = GetString(Resource.String.required_error_message);
            //    valid = false;
            //}
            //if (string.IsNullOrWhiteSpace(txTelefono.Text))
            //{
            //    telefonoLayout.ErrorEnabled = true;
            //    telefonoLayout.Error = GetString(Resource.String.required_error_message);
            //    valid = false;
            //}
            //if (string.IsNullOrWhiteSpace(txtPassword.Text))
            //{
            //    passwordLayout.ErrorEnabled = true;
            //    passwordLayout.Error = GetString(Resource.String.required_error_message);
            //    valid = false;
            //}




            if (valid)
            {
                UserProfile userProfile = new UserProfile();
                userProfile.Name= txtName.Text;
                userProfile.IdentificationCard = txtCedula.Text;
                userProfile.TelephoneNumber = txTelefono.Text;
                //userProfile.User.PassworDigest = txtPassword.Text;


                var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "Validando Información...");
                progressDialog.Indeterminate = true;
                progressDialog.SetCancelable(false);


                Snackbar snackbar = Snackbar.Make(passwordLayout, "Información Actualizada", Snackbar.LengthLong)
                               .SetAction("OK", (v) =>
                               {
                                   this.Finish();
                               });

                new Thread(new ThreadStart(delegate
                {
                    //LOAD METHOD TO GET ACCOUNT INFO
                    userProfile = null;// _client.Post("Authentication/Update", userProfile).Result.JsonToObject<UserProfile>();

                    if (userProfile == null)
                    {
                        snackbar.SetDuration(4000);
                        snackbar.SetActionTextColor(Android.Graphics.Color.Orange);
                        snackbar.Show();

                        

                        //Intent intent = new Intent(this, typeof(HomeActivity));
                        //StartActivity(intent);
                    }
                    else
                    {
                        Snackbar.Make(passwordLayout, "Usuario No logueado", Snackbar.LengthLong)
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

        private void btnCars_click(object sender, EventArgs e)
        {

            StartActivity(new Intent(Application.Context, typeof(CarsActivity)));
        }
    }
}