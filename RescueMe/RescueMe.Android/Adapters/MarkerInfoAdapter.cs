using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.Gms.Maps.Model;
using System.Linq;
using System.Text;
using Android.Gms.Maps;
using Android.Locations;
using RescueMe.Droid.Data;

namespace RescueMe.Droid.Adapters
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
            View view;
            if (!string.IsNullOrEmpty(marker.Title) && marker.Title != "My position") //Show time and Distance
            {
                view = _layoutInflater.Inflate(Resource.Layout.info_time, null, false);
                view.FindViewById<TextView>(Resource.Id.txtTime).Text = _directions.Duration;
                view.FindViewById<TextView>(Resource.Id.txtDistance).Text = _directions.Distance;
            }
            else
            {
                view = _layoutInflater.Inflate(Resource.Layout.info_window, null, false);
                string mAddress;

                if (IsNetworkConnected == true)
                {

                    //The Geocoder class retrieves a list of address from Google over the internet  
                    IList<Address> addressList = mGeocoder.GetFromLocation(mCurrentLocation.Latitude, mCurrentLocation.Longitude, 10);
                    Address addressCurrent = addressList.FirstOrDefault(m => m.MaxAddressLineIndex > 0);

                    if (addressCurrent != null)
                    {
                        StringBuilder deviceAddress = new StringBuilder();

                        for (int i = 0; i < addressCurrent.MaxAddressLineIndex; i++)
                            deviceAddress.Append(addressCurrent.GetAddressLine(i))
                                .AppendLine(",");

                        mAddress = deviceAddress.ToString();
                    }
                    else
                    {

                        mAddress = "Unable to determine the address.";
                    }
                }
                else
                {
                    mAddress = "No Disponible";
                }

                view.FindViewById<TextView>(Resource.Id.txtAddress).Text = mAddress;
            }
            return view;
        }
    }
}