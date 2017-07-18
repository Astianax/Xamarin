using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.Gms.Maps.Model;
using System.Linq;
using System.Text;
using Android.Gms.Maps;
using Android.Locations;

namespace RescueMe.Agent.Adapters
{
    public class MarkerInfoAdapter : Java.Lang.Object, GoogleMap.IInfoWindowAdapter
    {
        private LayoutInflater _layoutInflater = null;
        private Location mCurrentLocation;
        private Geocoder mGeocoder;
        public bool IsNetworkConnected { get; set; }

        public MarkerInfoAdapter(LayoutInflater inflater, Geocoder geocoder, Location location)
        {
            //This constructor does hit a breakpoint and executes
            _layoutInflater = inflater;
            mCurrentLocation = location;
            mGeocoder = geocoder;
        }

        public View GetInfoContents(Marker marker)
        {
            return null;
        }

        public View GetInfoWindow(Marker marker)
        {
            View view = _layoutInflater.Inflate(Resource.Layout.info_window, null, false);
            string mAddress;

            if (IsNetworkConnected == true)
            {
                mAddress = RescueMe.Agent.Activities.BaseActivity.GetAddress(mCurrentLocation, mGeocoder);
            }
            else
            {
                mAddress = "No Disponible";
            }

            view.FindViewById<TextView>(Resource.Id.txtAddress).Text = mAddress;
            return view;
        }
    }
}