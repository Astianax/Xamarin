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
using Android.Content.PM;
using RescueMe.Domain;
using System.Threading.Tasks;
using Android.Support.V7.Widget;
using RescueMe.Droid.Adapters;

namespace RescueMe.Droid.Activities
{
    [Activity(Label = "RescueActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class RescueActivity : BaseActivity
    {
        private List<Request> _requests;
        private RecyclerView mRecyclerView;
        RecyclerView.LayoutManager mLayoutManager;
        AdapterRescues mAdapter;
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MyRescues);
            SetTools();
            // Create your application here
            _requests = await GetRequests();
            SetRecyclerView(_requests);

        }
        public async Task<List<Request>> GetRequests()
        {
            var requests = new List<Request>();
            //try
            //{
            //    if (IsNetworkConnected())
            //    {
            //        requests = _client.Get("Request/requests", new
            //                                                {
            //                                                    UserId = _context.GetUser().UserID,
            //                                                    platform = "mobile"
            //                                                }
            //            ).Result.JsonToObject<List<Request>>(); 
            //    }
            //    else
            //    {
            //        //Offline
            //        requests = _context.GetRequest();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    requests = null;
            //    //message = ex.Message;
            //}
            requests = _context.GetRequest();
            return requests;
        }
        private void SetRecyclerView(List<Request> dataset)
        {
            //List<Vehicle> dataset2 = (List<Vehicle>)dataset;
            mRecyclerView = (RecyclerView)FindViewById(Resource.Id.my_recycler_view);


            // use this setting to improve performance if you know that changes
            // in content do not change the layout size of the RecyclerView
            //mRecyclerView.SetHasFixedSize(true);

            // use a linear layout manager
            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            // specify an adapter (see also next example)           
            mAdapter = new AdapterRescues(dataset, _context);
            mAdapter.IsNetworkConnected = IsNetworkConnected();
            mRecyclerView.SetAdapter(mAdapter);
        }
    }
}