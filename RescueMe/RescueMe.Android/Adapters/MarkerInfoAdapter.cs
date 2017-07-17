﻿using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using Android.Gms.Maps.Model;
using System.Linq;
using System.Text;
using Android.Gms.Maps;
using Android.Locations;

namespace RescueMe.Droid.Adapters
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

                //The Geocoder class retrieves a list of address from Google over the internet  
                IList<Address> addressList = mGeocoder.GetFromLocation(mCurrentLocation.Latitude, mCurrentLocation.Longitude, 10);
                Address addressCurrent = addressList.FirstOrDefault();

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
            return view;
        }
    }
}