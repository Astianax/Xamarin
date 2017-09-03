using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.Gms.Maps.Model;
using System.Linq;
using System.Text;
using Android.Gms.Maps;
using Android.Locations;
using RescueMe.Agent.Data;

namespace RescueMe.Agent.Adapters
{
    public class MarkerInfoAdapter : Java.Lang.Object, GoogleMap.IInfoWindowAdapter
    {
        private LayoutInflater _layoutInflater = null;
        private Location mCurrentLocation;
        private Geocoder mGeocoder;
        private Directions _directions;
        public bool IsNetworkConnected { get; set; }

        public MarkerInfoAdapter(LayoutInflater inflater, Geocoder geocoder, Location location, Directions directions)
        {
            //This constructor does hit a breakpoint and executes
            _layoutInflater = inflater;
            mCurrentLocation = location;
            mGeocoder = geocoder;
            _directions = directions;
        }

        public View GetInfoContents(Marker marker)
        {
            return null;
        }

        public View GetInfoWindow(Marker marker)
        {
            View view = _layoutInflater.Inflate(Resource.Layout.info_window, null, false);
            string mAddress;
            if (_directions != null && !string.IsNullOrEmpty(_directions.Duration)
                && !string.IsNullOrEmpty(_directions.Distance))
            {
                view = _layoutInflater.Inflate(Resource.Layout.info_time, null, false);
                view.FindViewById<TextView>(Resource.Id.txtTime).Text = _directions.Duration;
                view.FindViewById<TextView>(Resource.Id.txtDistance).Text = _directions.Distance;
            }
          
            //if (IsNetworkConnected == true)
            //{
            //    mAddress = RescueMe.Agent.Activities.BaseActivity.GetAddress(mCurrentLocation, mGeocoder);
            //}
            //else
            //{
            //    mAddress = "No Disponible";
            //}

            //view.FindViewById<TextView>(Resource.Id.txtAddress).Text = mAddress;
            return view;
        }
    }
}