using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

using OpenQA.Selenium.Support.UI;

using System.Threading;
using System.Drawing;
using System.Net;
using RestSharp;
using RestSharp.Deserializers;

using Newtonsoft.Json;
using System.IO;
using R.Models;

namespace R
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /* 
             IWebDriver driver = new FirefoxDriver();
             driver.Manage().Window.Position = new Point(-2000, 0);

             driver.Navigate().GoToUrl("https://www.realtor.ca");
             driver.Navigate().GoToUrl("https://www.realtor.ca/Residential/Map.aspx#CultureId=1&ApplicationId=1&RecordsPerPage=9&MaximumResults=9&PropertySearchTypeId=1&TransactionTypeId=2&StoreyRange=0-0&BedRange=0-0&BathRange=0-0&LongitudeMin=-114.13666876853941&LongitudeMax=-114.12212046684263&LatitudeMin=51.03741800312255&LatitudeMax=51.04181646583393&SortOrder=A&SortBy=1&viewState=g&Longitude=-114.13389&Latitude=51.039&CurrentPage=1");
             System.Console.WriteLine("title is :" + driver.Title);


             List<OpenQA.Selenium.Cookie> cookies = driver.Manage().Cookies.AllCookies.ToList();
             CookieContainer cookieContainer = new CookieContainer();
             foreach (var c in cookies)
             {
                 System.Console.WriteLine("name:{0} value:{1}", c.Name, c.Value);
                 System.Net.Cookie cookie = new System.Net.Cookie(c.Name,c.Value,c.Path,c.Domain);
                 //if(c.Expiry.Value != null)
                 // cookie.Expires = c.Expiry.Value;
                 cookieContainer.Add(cookie);
             }
             */

            var db = new RContext();

            var client = new RestClient("https://www.realtor.ca/api/Listing.svc/PropertySearch_Post");
            //client.CookieContainer = cookieContainer;
          
            var request = new RestRequest();

            request.Method = Method.POST;

            request.AddHeader("Host", "www.realtor.ca");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");
            request.AddHeader("Accept", "*/*"); //application/json
            request.AddHeader("Accept-Language", "en-US,en;q=0.5");
            request.AddHeader("Accept-Encoding", "gzip, deflate, br");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            request.AddHeader("Referer", "https://www.realtor.ca");


            request.AddParameter("CultureId", "1");
            request.AddParameter("ApplicationId", "1");
            request.AddParameter("RecordsPerPage", "9");
            request.AddParameter("MaximumResults", "9");
            request.AddParameter("PropertySearchTypeId", "1");
            request.AddParameter("TransactionTypeId", "2");
            request.AddParameter("StoreyRange", "0-0");
            request.AddParameter("BedRange", "0-0");
            request.AddParameter("BathRange","0-0" );
            request.AddParameter("LongitudeMin", "-114.145241108551");
            request.AddParameter("LongitudeMax", "-114.1139558226013");
            request.AddParameter("LatitudeMin", "51.03625085626268");
            request.AddParameter("LatitudeMax", "51.04243032849644");
            request.AddParameter("SortOrder", "A");
            request.AddParameter("SortBy", "1");
            request.AddParameter("viewState","g");
            request.AddParameter("Longitude", "-114.13389");
            request.AddParameter("Latitude", "51.039");
            request.AddParameter("CurrentPage", "1");

            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json charset=utf-8"; };

            client.AddHandler("application/json", new DynamicJsonDeserializer());

            dynamic response = client.Execute(request);

            Results results = JsonConvert.DeserializeObject<Results>(response.Content);

            foreach(var  r in results.results )
            {
                System.Console.WriteLine("Id:"+ r.Id);
                System.Console.WriteLine("MlsNumber:"+r.MlsNumber);
      

            }
            

            System.Console.WriteLine("content is:" + response.Content);

            File.WriteAllText("c:\\1.json", response.Content);
            

           // dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Content);
            
            // WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(50));
            // wait.Until((d) => { return d.Title.ToLower().StartsWith("Property Search"); });


            //driver.Quit();

          
           
        }
    }
}
