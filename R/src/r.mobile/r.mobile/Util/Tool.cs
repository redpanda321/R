using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R.Mobile.Util
{
 public class Tool
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static R.Mobile.Models.Position GetPosition(double latitude, double longitude, double distance)
        {
            var position = new R.Mobile.Models.Position();


            double longitudeMin = ( longitude - distance/(111 * Math.Cos(latitude)) ) ;
            double longitudeMax = (longitude + distance / (111 * Math.Cos(latitude)));
            double latitudeMin = (latitude - distance/111);
            double latitudeMax = (latitude + distance / 111);

            position.Longitude = longitude;
            position.Latitude = latitude;
            position.LongitudeMin = longitudeMin;
            position.LongitudeMax = longitudeMax;
            position.LatitudeMin = latitudeMin;
            position.LatitudeMax = latitudeMax;



            return position;

        }

    }
}
