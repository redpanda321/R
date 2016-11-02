using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Xamarin.Forms;
using Xamarin.Forms.Maps;

using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using TK.CustomMap;

using Newtonsoft.Json;
using R.Mobile.Models;

using WebSocketSharp;

namespace R.Mobile
{
    class MapPage:ContentPage
    {

      //map
       public MyMap m_Map { get; set; }
        
       public TKCustomMapPin m_CurMapPin { get; set; }
        
       public TKCustomMapPin m_CenterMapPin { get; set; }

       public Xamarin.Forms.Maps.Position m_XPosition { get; set; }

       //Geo
       public IGeolocator m_GeoLocator { get; set; }
        
       public Plugin.Geolocator.Abstractions.Position m_GeoPosition { get; set; }
       //UI
       public StackLayout m_MainStackLayout { get; set; }
       
       //WebSocket
       public WebSocketSharp.WebSocket m_WebSocketResult { get; set; }
        
       //Data
       public List<Result> m_Results { get; set; }
       
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

            m_Map.MapLongPress += M_Map_MapLongPress;

            m_Map.SizeChanged += M_Map_SizeChanged;

           
            
            //Page

            this.Appearing += MapPage_Appearing;
           
            //UI   
            m_MainStackLayout = new StackLayout { Spacing = 0 };
            m_MainStackLayout.Children.Add(m_Map);
            Content = m_MainStackLayout;
            
            //WebSocket
           
            m_WebSocketResult = new WebSocketSharp.WebSocket("ws://192.168.0.121:51151/ResultBehavior");
            m_WebSocketResult.OnMessage += M_WebSocketResult_OnMessage;


        }

        private async void M_Map_SizeChanged(object sender, EventArgs e)
        {

            //GeoLocator
            m_GeoLocator = CrossGeolocator.Current;

            m_GeoPosition = await m_GeoLocator.GetPositionAsync(5000);

            Xamarin.Forms.Maps.Position xPosition = new Xamarin.Forms.Maps.Position(m_GeoPosition.Latitude, m_GeoPosition.Longitude);

            m_CenterMapPin = new TKCustomMapPin { Position = xPosition };
            m_Map.SelectedPin = m_CenterMapPin;

            m_Map.MoveToRegion(MapSpan.FromCenterAndRadius(xPosition, Distance.FromKilometers(3)));

            //WebSocket

            R.Mobile.Models.Position jPosition = R.Mobile.Util.Tool.GetPosition(xPosition.Latitude, xPosition.Longitude, 3);

            m_WebSocketResult.Connect();
            m_WebSocketResult.Send(JsonConvert.SerializeObject(jPosition));


        }

        private async void M_Map_MapLongPress(object sender, TKGenericEventArgs<Xamarin.Forms.Maps.Position> e)
        {
            //GeoLocator
            m_GeoLocator = CrossGeolocator.Current;

            m_GeoPosition = await m_GeoLocator.GetPositionAsync(5000);

            Xamarin.Forms.Maps.Position xPosition = new Xamarin.Forms.Maps.Position(m_GeoPosition.Latitude, m_GeoPosition.Longitude);

            m_CenterMapPin = new TKCustomMapPin { Position = xPosition };
            m_Map.SelectedPin = m_CenterMapPin;

            m_Map.MoveToRegion(MapSpan.FromCenterAndRadius(xPosition, Distance.FromKilometers(3)));

            //WebSocket

            R.Mobile.Models.Position jPosition = R.Mobile.Util.Tool.GetPosition(xPosition.Latitude, xPosition.Longitude, 3);

            m_WebSocketResult.Connect();
            m_WebSocketResult.Send(JsonConvert.SerializeObject(jPosition));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MapPage_Appearing(object sender, EventArgs e)
        {
            //GeoLocator
            m_GeoLocator = CrossGeolocator.Current;

            m_GeoPosition = await m_GeoLocator.GetPositionAsync(5000);

            Xamarin.Forms.Maps.Position xPosition = new Xamarin.Forms.Maps.Position(m_GeoPosition.Latitude, m_GeoPosition.Longitude);
                
            m_CenterMapPin = new TKCustomMapPin { Position = xPosition  };
            m_Map.SelectedPin = m_CenterMapPin;

            m_Map.MoveToRegion(MapSpan.FromCenterAndRadius(xPosition, Distance.FromKilometers(3)));

            //WebSocket
            
            R.Mobile.Models.Position jPosition = R.Mobile.Util.Tool.GetPosition(xPosition.Latitude, xPosition.Longitude, 3);

            m_WebSocketResult.Connect();
            m_WebSocketResult.Send(JsonConvert.SerializeObject(jPosition));

        }

        private void M_WebSocketResult_OnMessage(object sender, MessageEventArgs e)
        {
            m_Results = JsonConvert.DeserializeObject<List<Result>>(e.Data);

            foreach (var x  in m_Results ) {

                var position = new Xamarin.Forms.Maps.Position(x.Property.Address.Latitude, x.Property.Address.Longitude);

                Xamarin.Forms.Maps.Pin pin = new Xamarin.Forms.Maps.Pin
                {

                    Position = position,
                    Label = "Price:" + x.Property.Price + "" +
                               "Bathrooms:" + x.Building.BathroomTotal + " " +
                               "Bedrooms:" + x.Building.Bedrooms + " " +
                               "SizeInterior:" + x.Building.SizeInterior + " " +
                               "StoriesTotal:" + x.Building.StoriesTotal + " " +
                               "Type:" + x.Building.Type,
            

                    Address = x.RelativeDetailsURL                      
                };

                m_Map.Pins.Add(pin);
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        ///      
        private void M_Map_MapClicked(object sender, TKGenericEventArgs<Xamarin.Forms.Maps.Position> e)
        {
            Xamarin.Forms.Maps.Position xPosition = new Xamarin.Forms.Maps.Position(e.Value.Latitude, e.Value.Longitude);


            m_CurMapPin = new TKCustomMapPin
            {

                Position = xPosition,
                Title = "Current Position"
            };


            m_Map.SelectedPin = m_CurMapPin;


            m_Map.MoveToRegion(MapSpan.FromCenterAndRadius(xPosition, Distance.FromKilometers(3)));

            //WebSocket

            R.Mobile.Models.Position jPosition = R.Mobile.Util.Tool.GetPosition(xPosition.Latitude, xPosition.Longitude, 3);
            m_WebSocketResult.Send(JsonConvert.SerializeObject(jPosition));

        }


    }
}
