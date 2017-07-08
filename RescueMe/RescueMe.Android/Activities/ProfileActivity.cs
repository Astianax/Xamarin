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
using System.Text.RegularExpressions;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "ProfileActivity")]
    public class ProfileActivity : BaseActivity
    {

        TextInputEditText txtName;
        TextInputEditText txtCedula;
        TextInputEditText txTelefono;
        TextInputEditText txtPassword;

        Android.Support.Design.Widget.TextInputLayout nameLayout;
        Android.Support.Design.Widget.TextInputLayout passwordLayout;
        Android.Support.Design.Widget.TextInputLayout cedulaLayout;
        Android.Support.Design.Widget.TextInputLayout telefonoLayout;




        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Profile);
            SetTools();
            // Create your application here


            //Controls
            var btnCars = FindViewById<ImageView>(Resource.Id.btnCars);
            var btnSave = FindViewById<Button>(Resource.Id.btnSave);

            txtName = FindViewById<TextInputEditText>(Resource.Id.txtName);
            txtCedula = FindViewById<TextInputEditText>(Resource.Id.txtCedula);
            txTelefono = FindViewById<TextInputEditText>(Resource.Id.txTelefono);
            txtPassword = FindViewById<TextInputEditText>(Resource.Id.txtPassword);

            nameLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.nameLayout);
            passwordLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.passwordLayout);
            cedulaLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.cedulaLayout);
            telefonoLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.telefonoLayout);

            //Settings Data Profile
            load_userInformation();

            btnCars.Click += btnCars_click;
            btnSave.Click += btnSave_click;
        }





        private void btnSave_click(object sender, EventArgs e)
        {
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                nameLayout.ErrorEnabled = true;
                nameLayout.Error = GetString(Resource.String.required_error_message);
                valid = false;
            }
            else
            {
                nameLayout.ErrorEnabled = false;
            }

            if (!string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                if (!txtCedula.Text.IsValidIdCard())
                {
                    cedulaLayout.ErrorEnabled = true;
                    cedulaLayout.Error = GetString(Resource.String.invalid_idCard);
                    valid = false;
                }
                else
                {
                    cedulaLayout.ErrorEnabled = false;
                }

            }

            if (!string.IsNullOrWhiteSpace(txTelefono.Text))
            {
                if (!txTelefono.Text.IsPhoneNumber())
                {
                    telefonoLayout.ErrorEnabled = true;
                    telefonoLayout.Error = GetString(Resource.String.invalid_telephone);
                    valid = false;
                }
                else
                {
                    telefonoLayout.ErrorEnabled = false;
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
                UserProfile userProfile = new UserProfile();
                bool updatedProfile;
                UserProfile context = _context.GetUser();

                userProfile.Name = txtName.Text;
                userProfile.IdentificationCard = txtCedula.Text;
                userProfile.TelephoneNumber = txTelefono.Text;
                userProfile.Id = context.Id;
                userProfile.Email = context.Email;
                userProfile.User = new User
                {
                    PassworDigest = txtPassword.Text
                };


                var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "Validando Información...");
                progressDialog.Indeterminate = true;
                progressDialog.SetCancelable(false);



                _context.UpdateUser(userProfile); //Validation


                if (IsNetworkConnected())
                {
                    new Thread(new ThreadStart(delegate
                      {
                          //LOAD METHOD TO GET ACCOUNT INFO

                          updatedProfile = bool.Parse(_client.Post("Authentication/Update", userProfile).Result.ToString());
                  

                          if (updatedProfile != false)
                          {
                              Snackbar.Make(passwordLayout, "Perfil Actualizado", Snackbar.LengthLong)
                                    .SetAction("OK", (v) =>
                                    {
                                        this.Finish();
                                    })
                                    .SetDuration(4000)
                                    .SetActionTextColor(Android.Graphics.Color.Orange)
                                    .Show();
                          }
                          else
                          {
                              Snackbar.Make(passwordLayout, "Usuario No logueado", Snackbar.LengthLong)
                                      .SetAction("OK", (v) => { })
                                      .SetDuration(8000)
                                      .SetActionTextColor(Android.Graphics.Color.Orange)
                                      .Show();
                          }
                          //HIDE PROGRESS DIALOG
                          RunOnUiThread(() =>
                          {
                              progressDialog.Hide();

                          });

                      })).Start();
                }

            }

        }

        private void btnCars_click(object sender, EventArgs e)
        {

            StartActivity(new Intent(Application.Context, typeof(CarsActivity)));
        }

        public void load_userInformation()
        {

            UserProfile profile = _context.GetUser();
            var userViewModel = new UserViewModel();
            userViewModel.email = profile.Email;
            userViewModel.password = profile.User.PassworDigest;
            userViewModel.platform = "web"; // TO CHANGE



            var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "Cargando Perfil...");
            progressDialog.Indeterminate = true;
            progressDialog.SetCancelable(false);


            if (IsNetworkConnected())
            {
                new Thread(new ThreadStart(delegate
                {
                    //LOAD METHOD TO GET ACCOUNT INFO

                    profile = _client.Post("Authentication/IsAuthenticated", userViewModel).Result.JsonToObject<UserProfile>();
                    //    //HIDE PROGRESS DIALOG
                    RunOnUiThread(() =>
                    {

                        progressDialog.Hide();
                        txtName.Text = profile.Name;
                        txtCedula.Text = profile.IdentificationCard;
                        txTelefono.Text = profile.TelephoneNumber;
                        txtPassword.Text = profile.User.PassworDigest;
                    });

                })).Start();
            }
            else
            {
                if (_context.GetUser() != null)
                {
                    txtName.Text = profile.Name;
                    txtCedula.Text = profile.IdentificationCard;
                    txTelefono.Text = profile.TelephoneNumber;
                    txtPassword.Text = profile.User.PassworDigest;

                }
            }

        }
    }
}