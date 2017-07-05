using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace RescueMe.Droid.Adapters
{
    class AdapterVehicle : RecyclerView.Adapter
    {
        public event EventHandler<AdapterVehicleClickEventArgs> ItemClick;
        public event EventHandler<AdapterVehicleClickEventArgs> ItemLongClick;
        string[] items;

        public AdapterVehicle(string[] data)
        {
            items = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = LayoutInflater.From(parent.Context).
                       Inflate(Resource.Layout.CardView, parent, false);
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

            // Replace the contents of the view with that element
            var holder = viewHolder as AdapterVehicleViewHolder;


            holder.info_text.Text= items[position];
            //holder.TextView.Text = items[position];
        }

        public override int ItemCount => items.Length;

        void OnClick(AdapterVehicleClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(AdapterVehicleClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class AdapterVehicleViewHolder : RecyclerView.ViewHolder
    {


        public TextView info_text { get; set; }




        
        //public TextView TextView { get; set; }
        public AdapterVehicleViewHolder(View itemView, Action<AdapterVehicleClickEventArgs> clickListener,
                            Action<AdapterVehicleClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            info_text = itemView.FindViewById<TextView>(Resource.Id.info_text);

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