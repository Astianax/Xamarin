﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using RescueMe.Droid.Data;
using Android.Graphics;

namespace RescueMe.Droid.Adapters
{
    public class SpinnerAdapter : ArrayAdapter<SpinnerItem>
    {

        // Your sent context
        private Context context;
        // Your custom values for the spinner (User)
        private SpinnerItem[] values;
        public SpinnerAdapter(Context context, int textViewResourceId,
                   SpinnerItem[] values) : base(context, textViewResourceId, values)
        {
            this.context = context;
            this.values = values;
        }


        public int getCount()
        {
            return values.Length;
        }

        public SpinnerItem getItem(int position)
        {
            return values[position];
        }

        public long getItemId(int position)
        {
            return position;
        }


        // And the "magic" goes here
        // This is for the "passive" state of the spinner

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
             //// And finally return your dynamic (or custom) view for each spinner item
            //return label;
            View layout = LayoutInflater.From(context).Inflate(Resource.Drawable.custom_spinner, parent, false);// inflater.inflate(R.layout.custom, parent, false);

            // Declaring and Typecasting the textview in the inflated layout
            TextView txtSpinner = (TextView)layout
            .FindViewById(Resource.Id.txtSpinner);

            // Setting the text using the array
            txtSpinner.Text = values[position].Description;

            // Setting the color of the text
            txtSpinner.SetTextColor(parent.Resources.GetColor(Resource.Color.menu_text_color));

      
            // Setting Special atrributes for 1st element
            if (position == 0)
            {
             
                txtSpinner.SetTextColor(parent.Resources.GetColor(Resource.Color.menu_text_color));

            }

            return layout;
            //return null;
        }

        // And here is when the "chooser" is popped up
        // Normally is the same view, but you can customize it if you want
        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
          
            View layout = LayoutInflater.From(context).Inflate(Resource.Drawable.custom_spinner, parent, false);// inflater.inflate(R.layout.custom, parent, false);

            // Declaring and Typecasting the textview in the inflated layout
            TextView txtSpinner = (TextView)layout
            .FindViewById(Resource.Id.txtSpinner);

            // Setting the text using the array
            txtSpinner.Text = values[position].Description;

            // Setting the color of the text
            txtSpinner.SetTextColor(parent.Resources.GetColor(Resource.Color.menu_text_color));


            // Setting Special atrributes for 1st element
            if (position == 0)
            {
               
                // Setting the text Color
                txtSpinner.SetTextColor(parent.Resources.GetColor(Resource.Color.menu_text_color));

            }

            return layout;
        }

    }
}