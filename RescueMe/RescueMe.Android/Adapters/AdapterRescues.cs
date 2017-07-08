using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using RescueMe.Domain;
using System.Collections.Generic;
using RescueMe.Droid.Data;
using Android.Support.Design.Widget;
using Android.Graphics;
using System.Net;

namespace RescueMe.Droid.Adapters
{
    public class AdapterRescues : RecyclerView.Adapter
    {
        public event EventHandler<AdapterRescuesClickEventArgs> ItemClick;
        public event EventHandler<AdapterRescuesClickEventArgs> ItemLongClick;
        List<Request> items;
        DbContext _context;
        public bool IsNetworkConnected { get; set; }
        TextInputLayout _marqueLayout;
        public AdapterRescues(List<Request> data, DbContext context)
        {
            items = data;
            _context = context;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.RescuesCardView, parent, false);
            //var id = Resource.Layout.__YOUR_ITEM_HERE;
            //itemView = LayoutInflater.From(parent.Context).
            //       Inflate(id, parent, false);

            var vh = new AdapterRescuesViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }


        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];
            Request request = item;
            // Replace the contents of the view with that element
            var holder = viewHolder as AdapterRescuesViewHolder;
             
           
            holder.Type.Text = request.Vehicle.Type;
            holder.Marque.Text = request.Vehicle.Marque;
            holder.Status.Text = request.Status.Name;
            //Get and validate if image exist 
            var imageBitmap = _context.GetImageBitmapFromRequest(request).Result;
            holder.Map.SetImageBitmap(imageBitmap);
           

        }
        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }
            return imageBitmap;
        }
        public override int ItemCount => items.Count;
        
        void OnClick(AdapterRescuesClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(AdapterRescuesClickEventArgs args) => ItemLongClick?.Invoke(this, args);
    }

    public class AdapterRescuesViewHolder : RecyclerView.ViewHolder
    {

        public TextView Status { get; set; }
        public TextView Type { get; set; }
        public TextView Marque { get; set; }
        public ImageView Map { get; set; }




        //public TextView TextView { get; set; }
        public AdapterRescuesViewHolder(View itemView, Action<AdapterRescuesClickEventArgs> clickListener,
                            Action<AdapterRescuesClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            Type = itemView.FindViewById<TextView>(Resource.Id.type);
            Marque = itemView.FindViewById<TextView>(Resource.Id.marque);
            Map = itemView.FindViewById<ImageView>(Resource.Id.map);
            Status = itemView.FindViewById<TextView>(Resource.Id.status);

            itemView.Click += (sender, e) => clickListener(new AdapterRescuesClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new AdapterRescuesClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class AdapterRescuesClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}