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

namespace r.mobile
{
    class MapPage:ContentPage
    {


       public Map map { get; set; }

       public IGeolocator locator { get; set; }
        
       public Plugin.Geolocator.Abstractions.Position position { get; set; }


        public MapPage() {


            map = new MyMap

            {
                IsShowingUser = true,
                HeightRequest = 100,
                WidthRequest = 960,
                VerticalOptions = LayoutOptions.FillAndExpand,
               

            };

           
           

            ///reLocate button

            var reLocate = new Button { Text = "Re-center" };
            reLocate.Clicked += async (sender, e) =>
            {


                locator = CrossGeolocator.Current;
                locator.DesiredAccuracy = 50;


                 position = await locator.GetPositionAsync(timeoutMilliseconds: 10000);
                if (position == null)
                {
                   
                    return;
                }
                var pos = new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude);

                map.MoveToRegion(MapSpan.FromCenterAndRadius(pos, Distance.FromMiles(3)));

                var pin = new Pin
                {

                    Type = PinType.Place,
                    Position = pos,
                    Label = "Current Place"

                };

                 map.Pins.Add(pin);

                
            };

            //segments StackLayout
            var segments = new StackLayout
            {
                Spacing = 30,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Horizontal,
                Children = { reLocate} 
            };


            //main stackLayout    
            var stack = new StackLayout { Spacing = 0 };

            stack.Children.Add(map);
            stack.Children.Add(segments);
            Content = stack;


        }


        /// <summary>
        /// Get Current Position
        /// </summary>
        /// <returns></returns>
        public async Task<Plugin.Geolocator.Abstractions.Position> GetCurrentPosition()
        {


            locator = CrossGeolocator.Current;

            position = await locator.GetPositionAsync(timeoutMilliseconds: 10000);


            return position;


        }

    }
}
