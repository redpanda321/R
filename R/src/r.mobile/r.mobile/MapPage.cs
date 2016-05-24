using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Xamarin.Forms;
using Xamarin.Forms.Maps;

using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace r.mobile
{
    class MapPage:ContentPage
    {


        Map map;

        IGeolocator locator;
        
        Plugin.Geolocator.Abstractions.Position position;


        public MapPage() {


            map = new Map
            {
                IsShowingUser = true,
                HeightRequest = 100,
                WidthRequest = 960,
                VerticalOptions = LayoutOptions.FillAndExpand

            };

            //////////////////////////////////////////////////////////////
           
            





            //////////////////////////////////////////////////////////////    
            var stack = new StackLayout { Spacing = 0 };

            stack.Children.Add(map);
            Content = stack;


        }



        public async Task<Plugin.Geolocator.Abstractions.Position> GetCurrentPosition()
        {


            locator = CrossGeolocator.Current;

            position = await locator.GetPositionAsync(timeoutMilliseconds: 10000);


            return position;


        }

    }
}
