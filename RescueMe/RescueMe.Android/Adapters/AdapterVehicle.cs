using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using RescueMe.Domain;
using System.Collections.Generic;
using RescueMe.Droid.Data;
using Android.Support.Design.Widget;

namespace RescueMe.Droid.Adapters
{
    public class AdapterVehicle : RecyclerView.Adapter
    {
        public event EventHandler<AdapterVehicleClickEventArgs> ItemClick;
        public event EventHandler<AdapterVehicleClickEventArgs> ItemLongClick;
        List<Vehicle> items;
        DbContext _context;
        UserProfile _user;
        private RestClient _client;
        public bool IsNetworkConnected { get; set; }
        TextInputLayout _marqueLayout;
        public AdapterVehicle(List<Vehicle> data, DbContext context, TextInputLayout marqueLayout)
        {
            items = data;
            _context = context;
            _marqueLayout = marqueLayout;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.CardView, parent, false);
            //var id = Resource.Layout.__YOUR_ITEM_HERE;
            //itemView = LayoutInflater.From(parent.Context).
            //       Inflate(id, parent, false);

            var vh = new AdapterVehicleViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];
            Vehicle vehicle = item;
            bool removed = false;
            _user = _context.GetUser();
            _client = new RestClient("http://rescueme-api.azurewebsites.net/api/");
            // Replace the contents of the view with that element
            var holder = viewHolder as AdapterVehicleViewHolder;


            holder.Type.Text = items[position].Type;
            holder.Marque.Text = items[position].Marque;



            holder.RemoveCar.Click += delegate
            {
                string marque = items[position].Marque;
                items.RemoveAt(position);


                NotifyItemRemoved(position);
                NotifyItemRangeChanged(position, items.Count);
                _context.UpdateVehicles(items);


                try
                {
                    if (IsNetworkConnected == true)
                    {
                        removed = bool.Parse(_client.Post("Vehicle/remove", vehicle).Result.ToString());
                    }
                }
                catch (Exception)
                {
                }

                if (removed)
                {
                    Snackbar.Make(_marqueLayout, "Vehículo "+ marque+" ha sido eliminado", Snackbar.LengthLong)
                        .SetAction("OK", (v) => {})
                        .SetDuration(4000)
                        .SetActionTextColor(Android.Graphics.Color.Orange)
                        .Show();
                }

            };


        }

        public override int ItemCount => items.Count;

        void OnClick(AdapterVehicleClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(AdapterVehicleClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class AdapterVehicleViewHolder : RecyclerView.ViewHolder
    {


        public TextView Type { get; set; }
        public TextView Marque { get; set; }
        public ImageView RemoveCar { get; set; }




        //public TextView TextView { get; set; }
        public AdapterVehicleViewHolder(View itemView, Action<AdapterVehicleClickEventArgs> clickListener,
                            Action<AdapterVehicleClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            Type = itemView.FindViewById<TextView>(Resource.Id.type);
            Marque = itemView.FindViewById<TextView>(Resource.Id.marque);
            RemoveCar = itemView.FindViewById<ImageView>(Resource.Id.removeCar);

            itemView.Click += (sender, e) => clickListener(new AdapterVehicleClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new AdapterVehicleClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class AdapterVehicleClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}