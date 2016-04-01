using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R.Models
{
    public class House {

        public int HouseId { get; set; }

        public string ImageUrl { get; set; }

        public string Url { get; set; }

        public string Address { get; set; }

        public string PostCode { get; set; }
        
        public string City { get; set; }

        public string Province { get; set; }

        public float Price { get; set; }

        public float Bedroom { get; set; }

        public float Bedroom1 { get; set; }

        public float Bedroom2 { get; set; }

        public float Bashroom { get; set; }

        public string Type { get; set; }

        public string ListingID { get; set; }


    }


    public class HousePrice {

       public int HousePriceId { get; set; }

       public string Date { get; set; }

       public int HouseId { get; set; }  


    }


}
