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
using System.Threading;
using Android.Support.Design.Widget;
using RescueMe.Domain;
using RescueMe.Droid.Data;
using Android.Content.PM;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "RegisterActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class RegisterActivity : BaseActivity
    {

        TextInputEditText txtName;
        TextInputEditText txtEmail;
        TextInputEditText txtPassword;
        TextInputEditText txtPasswordConfirm;

        Android.Support.Design.Widget.TextInputLayout emailLayout;
        Android.Support.Design.Widget.TextInputLayout nameLayout;
        Android.Support.Design.Widget.TextInputLayout passwordLayout;
        Android.Support.Design.Widget.TextInputLayout passwordConfirmLayout;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Register);
            SetTools();


            //Controls
            var btnRegister = FindViewById<Button>(Resource.Id.btnRegister);

            txtName = FindViewById<TextInputEditText>(Resource.Id.txtName);
            txtEmail = FindViewById<TextInputEditText>(Resource.Id.txtEmail);
            txtPassword = FindViewById<TextInputEditText>(Resource.Id.txtPassword);
            txtPasswordConfirm = FindViewById<TextInputEditText>(Resource.Id.txtPasswordConfirm);


            emailLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.emailALayout);
            nameLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.nameLayout);
            passwordLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.passwordLayout);
            passwordConfirmLayout = FindViewById<Android.Support.Design.Widget.TextInputLayout>(Resource.Id.passwordConfirmLayout);

            btnRegister.Click += btnRegister_click;
        }






        private void btnRegister_click(object sender, EventArgs e)
        {
            bool valid = true;

            //Nombre
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

            //Email
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

            //Password
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

            //password match
            if (string.IsNullOrWhiteSpace(txtPasswordConfirm.Text))
            {
                passwordConfirmLayout.ErrorEnabled = true;
                passwordConfirmLayout.Error = GetString(Resource.String.required_error_message);
                valid = false;
            }
            else
            {
                passwordConfirmLayout.ErrorEnabled = false;
            }

            if (valid && txtPassword.Text != txtPasswordConfirm.Text)
            {
                passwordConfirmLayout.ErrorEnabled = true;
                passwordConfirmLayout.Error = GetString(Resource.String.password_not_match);
                valid = false;

            }



            if (valid)
            {
                UserProfile userProfile = new UserProfile();
                User user = null;

                userProfile.Name = txtName.Text;
                userProfile.Email = txtEmail.Text;
                userProfile.User = new User
                {
                    PassworDigest = txtPassword.Text
                };


                var progressDialog = ProgressDialog.Show(this, "Por favor espere...", "Validando Información...");
                progressDialog.Indeterminate = true;
                progressDialog.SetCancelable(false);
                var message = "";




                new Thread(new ThreadStart(delegate
                {
                    //LOAD METHOD TO GET ACCOUNT INFO

                    try
                    {

                        user = _client.Post("Authentication/create", userProfile).Result.JsonToObject<User>();
                    }
                    catch (Exception ex)
                    {
                        user = null;
                        message = ex.Message;
                    }


                    if (user != null)
                    {
                        //Save Database
                        _context.Save<UserSaved>(new UserSaved()
                        {
                            Email = userProfile.Email,
                            Id = user.Id,
                            FullName = userProfile.Name,
                            Password = userProfile.User.PassworDigest
                        });
                        //Save Vehicles

                        Intent intent = new Intent(this, typeof(HomeActivity));
                        StartActivity(intent);
                    }
                    else
                    {
                        Snackbar.Make(passwordLayout, message, Snackbar.LengthLong)
                                .SetAction("OK", (v) => {
                                    txtPassword.Text = String.Empty;
                                    txtPasswordConfirm.Text = String.Empty;
                                })
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
}