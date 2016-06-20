﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Xamarin.Forms;
using Xamarin.Forms.Maps;

using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using TK.CustomMap;

namespace r.mobile
{
    class MapPage:ContentPage
    {
       public MyMap m_Map { get; set; }
        
       public TKCustomMapPin m_CurMapPin { get; set; }
        
       public TKCustomMapPin m_CenterMapPin { get; set; }
        
       public IGeolocator m_GeoLocator { get; set; }
        
       public Plugin.Geolocator.Abstractions.Position m_GeoPosition { get; set; }

       public Xamarin.Forms.Maps.Position m_XPosition { get; set; }

       public StackLayout m_MainStackLayout { get; set; }

       
       public MapPage() {

            //Map
            m_Map = new MyMap

            {
                IsShowingUser = true,
                HeightRequest = 100,
                WidthRequest = 960,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };
            
            m_Map.MapClicked += M_Map_MapClicked;


            m_Map.UserLocationChanged += M_Map_UserLocationChanged;


            this.Appearing += MapPage_Appearing;
            
            //main stackLayout    
            m_MainStackLayout = new StackLayout { Spacing = 0 };
            m_MainStackLayout.Children.Add(m_Map);
            Content = m_MainStackLayout;
            
        }

        private async void MapPage_Appearing(object sender, EventArgs e)
        {
            //GeoLocator
            m_GeoLocator = CrossGeolocator.Current;

            m_GeoPosition = await m_GeoLocator.GetPositionAsync(5000);

            Xamarin.Forms.Maps.Position XPosition = new Xamarin.Forms.Maps.Position(m_GeoPosition.Latitude, m_GeoPosition.Longitude);
                
            m_CenterMapPin = new TKCustomMapPin { Position = XPosition  };
            m_Map.SelectedPin = m_CenterMapPin;

            m_Map.MoveToRegion(MapSpan.FromCenterAndRadius(XPosition, Distance.FromKilometers(3)));


        }

        private void M_Map_UserLocationChanged(object sender, TKGenericEventArgs<Xamarin.Forms.Maps.Position> e)
        {

            var position = new Xamarin.Forms.Maps.Position(e.Value.Latitude, e.Value.Longitude);


            m_CenterMapPin = new TKCustomMapPin
            {

                Position = position,
                Title = "Current Position"
            };


            m_Map.SelectedPin = m_CenterMapPin;

            m_Map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(3)));


        }

       

        /// <summary>
        /// Process Map Clicked Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M_Map_MapClicked(object sender, TKGenericEventArgs<Xamarin.Forms.Maps.Position> e)
        {
            var position = new Xamarin.Forms.Maps.Position(e.Value.Latitude, e.Value.Longitude);


            m_CurMapPin = new TKCustomMapPin
            {

                Position = position,
                Title = "Current Position"
            };


            m_Map.SelectedPin = m_CurMapPin;


            m_Map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(3)));

        }




    }
}
