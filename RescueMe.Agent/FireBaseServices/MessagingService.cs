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
using Firebase.Messaging;
using Android.Media;
using Android.Support.V4.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using RescueMe.Agent.Activities;

namespace RescueMe.Agent.FireBaseServices
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            SendNotification(message.GetNotification());
        }

        private void SendNotification(RemoteMessage.Notification data)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);


            Drawable myDrawable = GetDrawable(Resource.Drawable.logoIcon);
            Bitmap myLogo = ((BitmapDrawable)myDrawable).Bitmap;

            var defaultSoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            var notificationBuilder = new NotificationCompat.Builder(this)
                .SetSmallIcon(Resource.Drawable.logoIcon)
                .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
                .SetContentTitle(data.Title)
                .SetAutoCancel(true)
                .SetSound(defaultSoundUri)
                .SetContentIntent(pendingIntent)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(data.Body))
                .SetContentText(data.Body)
                .SetColor(Resource.Color.menu_text_color)
                .SetPriority((int)NotificationPriority.High);
            //.SetLargeIcon(myLogo);


            var notificationManager = NotificationManager.FromContext(this);
            notificationManager.Notify(data.Tag, 0, notificationBuilder.Build());

        }
    }
}