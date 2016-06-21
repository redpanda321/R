using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace r.mobile.Util
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
        public static r.mobile.Models.Position GetPosition(double latitude, double longitude, double distance)
        {
            var position = new r.mobile.Models.Position();


            String longitudeMin = ( longitude - distance/(111 * Math.Cos(latitude)) ).ToString() ;
            String longitudeMax = (longitude + distance / (111 * Math.Cos(latitude))).ToString();
            String latitudeMin = (latitude - distance/111).ToString();
            String latitudeMax = (latitude + distance / 111).ToString();

            position.Longitude = longitude.ToString();
            position.Latitude = latitude.ToString();
            position.LongitudeMin = longitudeMin;
            position.LongitudeMax = longitudeMax;
            position.LatitudeMin = latitudeMin;
            position.LatitudeMax = latitudeMax;



            return position;

        }

    }
}
