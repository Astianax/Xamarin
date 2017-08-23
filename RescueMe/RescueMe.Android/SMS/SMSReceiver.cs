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
using Android.Telephony;
using Android.Provider;
using RescueMe.Droid.Data;
using Android.Media;
using Android.Support.V4.App;
using Android.Util;

namespace RescueMe.Droid.SMS
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new string[] { "android.provider.Telephony.SMS_RECEIVED" }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class SMSReceiver : BroadcastReceiver
    {
        public static readonly string IntentAction = "android.provider.Telephony.SMS_RECEIVED";
        public override void OnReceive(Context context, Intent intent)
        {
            //context.SendOrderedBroadcast(intent, IntentAction);
            InvokeAbortBroadcast();
            try
            {
                if (intent.Action != IntentAction) return;
                //DeleteMessage(context);
                SmsMessage[] messages = Telephony.Sms.Intents.GetMessagesFromIntent(intent);

                foreach (var message in messages) {
                    if (message.OriginatingAddress.Contains("78500")) {
                        //Status messages
                        var values = message.MessageBody.Split('-');
                        var db = DbContext.Instance;
                        var request = db.GetRequest().Where(s => s.Status.Name.ToLower() == "pendiente").FirstOrDefault();
                        //.Where(r => r.Id == 0).FirstOrDefault();
                        db.UpdateRequest(request, int.Parse(values[1]), int.Parse(values[2]));
                        if (values[3] == "")
                        {
                            string statusName = db.getStatusList().Where(s => s.Id == int.Parse(values[2])).FirstOrDefault().Name;
                            string body = "";
                            if (statusName.ToLower() == "no disponible")
                            {
                                body = "No hay agentes disponibles, por el momento";
                            }
                            else
                            {
                                body = "Se ha " + statusName + " su solicitud";
                            }
                            SendNotification(body, context);
                        }
                        else
                        {
                            SendNotification(values[3], context);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Long).Show();
            }
           
          

        }

        private void SendNotification(string body, Context context)
        {

            var defaultSoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            var notificationBuilder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetContentTitle("Actualización de Status")
                .SetContentText(body)
                .SetAutoCancel(true)
                .SetSound(defaultSoundUri);

            var notificationManager = NotificationManager.FromContext(context);
            notificationManager.Notify(0, notificationBuilder.Build());
        }
        private void DeleteMessage(Context context)
        {
            //context.ContentResolver.Delete(Android.Net.Uri.Parse("content://sms/78500"), null, null);
            var c = context.ContentResolver.Query(Android.Net.Uri.Parse("content://sms/inbox"), null, null, null, null);
            //c.moveToFirst(); 

            while (c.MoveToNext())
            {
               

                try
                {
                    String address = c.GetString(2);
                    String MobileNumber = "78500";

                    if (address.Trim().Equals(MobileNumber))
                    {
                        String pid = c.GetString(1);
                        String uri = "content://sms/conversations/" + pid;
                        context.ContentResolver.Delete(Android.Net.Uri.Parse(uri), null, null);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("log>>>", e.ToString());
                }
            }
        }
    }
}