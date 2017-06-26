using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace RescueMe.Droid
{
    [Activity(Label = "RescueMe.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Login);

            //String webLinkText = '< a href = "https://prativas.files.wordpress.com/2013/05/screenshot-mozilla-firefox-private-browsing.png" >< img src = "https://prativas.files.wordpress.com/2013/05/screenshot-mozilla-firefox-private-browsing.png" alt = "Screenshot-Mozilla Firefox (Private Browsing)" width = "293" height = "254" class="alignnone size-full wp-image-291" /></a> ';
                TextView linkTextView = (TextView) FindViewById(Resource.Id.textView1);
              //  holder.profileFeedTitle.setText(Html.fromHtml(webLinkText))); 

        }
    }
}


