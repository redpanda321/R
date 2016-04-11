using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Threading;
using System.Drawing;
using System.Net;
using System.IO;
using System.Data.Entity;

using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

using RestSharp;
using RestSharp.Deserializers;

using Newtonsoft.Json;

using R.Models;

namespace R
{
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        public static string PROPERTYAPI = "https://www.realtor.ca/api/Listing.svc/PropertySearch_Post";
        
        /// <summary>
        /// 
        /// </summary>
        public static RestClient client = new RestClient(PROPERTYAPI);

        /// <summary>
        /// RestRe
        /// </summary>
        public static RestRequest request = new RestRequest();

        /// <summary>
        /// 
        /// </summary>
        public static RContext  db = new RContext();
        /// <summary>
        /// 
        /// </summary>
        public static Random  random = new Random();


        /// <summary>
        /// Get Api Results(String Format)
        /// </summary>
        /// <param name="longitudeMin"></param>
        /// <param name="longitudeMax"></param>
        /// <param name="latitudeMin"></param>
        /// <param name="latitudeMax"></param>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        public static string GetProperty(string longitudeMin,string longitudeMax,string latitudeMin,string latitudeMax,string longitude,string latitude, string currentPage) {


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
            request.AddParameter("BathRange", "0-0");
            request.AddParameter("LongitudeMin", longitudeMin);
            request.AddParameter("LongitudeMax", longitudeMax);
            request.AddParameter("LatitudeMin", latitudeMin);
            request.AddParameter("LatitudeMax", latitudeMax);
            request.AddParameter("SortOrder", "A");
            request.AddParameter("SortBy", "1");
            request.AddParameter("viewState", "g");
            request.AddParameter("Longitude", longitude);
            request.AddParameter("Latitude", latitude);
            request.AddParameter("CurrentPage", currentPage);


            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json charset=utf-8"; };
            client.AddHandler("application/json", new DynamicJsonDeserializer());

            return  client.Execute(request).Content;

        }

        /// <summary>
        /// Get Pages Number
        /// </summary>
        /// <param name="longitudeMin"></param>
        /// <param name="longitudeMax"></param>
        /// <param name="latitudeMin"></param>
        /// <param name="latitudeMax"></param>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        public static int GetPagingNumber(string longitudeMin, string longitudeMax, string latitudeMin, string latitudeMax, string longitude, string latitude)
        {

            Paging paging  =  JsonConvert.DeserializeObject<Paging>( GetProperty( longitudeMin,  longitudeMax,  latitudeMin,  latitudeMax,  longitude,  latitude, "1") );

            return paging.TotalPages;

        }
        /// <summary>
        /// Get All Cookies
        /// </summary>
        /// <returns></returns>
        public static CookieContainer GetCookies() {

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

            driver.Quit();

            return cookieContainer;

        }


        public static void SaveResults(List<Result> results)
        {
            foreach (var rr in results)
            {

                //Related Objects
                var dbResult = db.Results.Include(r => r.Building).Include(r => r.Land).Include(r => r.RelativeDetailsURL)
                 .Include(r => r.Property).Include(r => r.Property.Address).Include(r => r.Property.Parking.Select(p => p.Name))
                 .Include(r => r.Property.Photo.Select(p => p.SequenceId))
                 .Include(r => r.Individual.Select(v => v.IndividualID))
                  .Include(r => r.Individual.Select(v => v.Organization))
                 .Include(r => r.Individual.Select(v => v.Organization.Address))
                 .Include(r => r.Individual.Select(v => v.Organization.Phones.Select(p => p.PhoneNumber)))
                 .Include(r => r.Individual.Select(v => v.Organization.Emails.Select(e => e.ContactId)))
                 .Include(r => r.Individual.Select(v => v.Organization.Websites.Select(w => w.Website)))
                 .Include(r => r.Individual.Select(v => v.Phones.Select(p => p.PhoneNumber)))
                 .Include(r => r.Individual.Select(v => v.Websites.Select(w => w.Website)))
                 .Include(r => r.Individual.Select(v => v.Emails.Select(e => e.ContactId))).Single(r => r.Id == rr.Id);

                if (dbResult == null)
                {
                    db.Results.Add(rr);
                }
                else
                {

                    db.Entry(dbResult).CurrentValues.SetValues(rr);

                    var dbBuilding = dbResult.Building;
                    var dbLand = dbResult.Land;
                    var dbRelativeDetailsURL = dbResult.RelativeDetailsURL;
                    var dbProperty = dbResult.Property;
                    var dbProAddress = dbResult.Property.Address;

                    /*
                    if (dbBuilding.Id == rr.Building.Id)
                    {
                        db.Entry(dbBuilding).CurrentValues.SetValues(rr.Building);
                    }
                    else {
                        db.Buildings.Attach(rr.Building);
                        dbBuilding = rr.Building;
                    }
                    */

                    db.Entry(dbBuilding).CurrentValues.SetValues(rr.Building);
                    db.Entry(dbLand).CurrentValues.SetValues(rr.Land);
                    db.Entry(dbRelativeDetailsURL).CurrentValues.SetValues(rr.RelativeDetailsURL);
                    db.Entry(dbProperty).CurrentValues.SetValues(rr.Property);
                    db.Entry(dbProAddress).CurrentValues.SetValues(rr.Property.Address);

                    foreach (var p in rr.Property.Parking.ToList())
                    {

                        var dbProParking = dbProperty.Parking.SingleOrDefault(pa => pa.Id == p.Id);
                        if (dbProParking != null)
                            db.Entry(dbProParking).CurrentValues.SetValues(p);
                        else
                            dbProperty.Parking.Add(p);
                    }
                    foreach (var p in rr.Property.Photo.ToList())
                    {

                        var dbPhoto = dbProperty.Photo.SingleOrDefault(ph => ph.Id == p.Id);
                        if (dbPhoto != null)
                            db.Entry(dbPhoto).CurrentValues.SetValues(p);
                        else
                            dbProperty.Photo.Add(p);
                    }

                    foreach (var i in rr.Individual.ToList())
                    {
                        var dbIndividual = dbResult.Individual.SingleOrDefault(In => In.Id == i.Id);
                        if (dbIndividual != null)
                        {
                            db.Entry(dbIndividual).CurrentValues.SetValues(i);


                            foreach (var p in i.Phones.ToList())
                            {
                                var dbIndividualPhone = dbIndividual.Phones.SingleOrDefault(ph => ph.Id == p.Id);
                                if (dbIndividualPhone != null)
                                {
                                    db.Entry(dbIndividualPhone).CurrentValues.SetValues(p);
                                }
                                else
                                {
                                    dbIndividual.Phones.Add(p);
                                }


                            }

                            foreach (var e in i.Emails.ToList())
                            {
                                var dbIndividualEmail = dbIndividual.Emails.SingleOrDefault(em => em.Id == e.Id);
                                if (dbIndividualEmail != null)
                                {
                                    db.Entry(dbIndividualEmail).CurrentValues.SetValues(e);
                                }
                                else
                                {
                                    dbIndividual.Emails.Add(e);
                                }

                            }

                            foreach (var w in i.Websites.ToList())
                            {
                                var dbIndividualWebsite = dbIndividual.Websites.SingleOrDefault(we => we.Id == w.Id);
                                if (dbIndividualWebsite != null)
                                {
                                    db.Entry(dbIndividualWebsite).CurrentValues.SetValues(w);
                                }
                                else
                                {
                                    dbIndividual.Websites.Add(w);
                                }


                            }




                            var dbOrganization = dbIndividual.Organization;
                            db.Entry(dbOrganization).CurrentValues.SetValues(i.Organization);




                            var dbOrganizationAddress = dbIndividual.Organization.Address;
                            db.Entry(dbOrganizationAddress).CurrentValues.SetValues(i.Organization.Address);

                            foreach (var p in i.Organization.Phones.ToList())
                            {
                                var dbOrganizationPhone = dbOrganization.Phones.SingleOrDefault(ph => ph.Id == p.Id);
                                if (dbOrganizationPhone != null)
                                {
                                    db.Entry(dbOrganizationPhone).CurrentValues.SetValues(p);
                                }
                                else
                                {
                                    dbOrganization.Phones.Add(p);
                                }
                            }

                            foreach (var e in i.Organization.Emails.ToList())
                            {
                                var dbOrganizationEmail = dbOrganization.Emails.SingleOrDefault(em => em.Id == e.Id);
                                if (dbOrganizationEmail != null)
                                {
                                    db.Entry(dbOrganizationEmail).CurrentValues.SetValues(e);
                                }
                                else
                                {
                                    dbOrganization.Emails.Add(e);
                                }
                            }

                            foreach (var w in i.Organization.Websites.ToList())
                            {
                                var dbOrganizationWebsite = dbOrganization.Websites.SingleOrDefault(we => we.Id == w.Id);
                                if (dbOrganizationWebsite != null)
                                {
                                    db.Entry(dbOrganizationWebsite).CurrentValues.SetValues(w);
                                }
                                else
                                {
                                    dbOrganization.Websites.Add(w);
                                }
                            }


                        }



                    }

                }

            }




        }


        public static void Main(string[] args)
        {
           
            string longitudeMin = "-114.145241108551";
            string longitudeMax = "-114.1139558226013";
            string latitudeMin = "51.03625085626268";
            string latitudeMax = "51.04243032849644";
            string longitude ="-114.13389";
            string latitude = "51.039";
           

            int num = GetPagingNumber(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude);

            if (num > 0)
            {
                for (int i = 1; i <= num; i++)
                {

                    //Sleep 1~10 s
                    var span = random.Next(1000, 10000);
                    Thread.Sleep(span);
                    
                    //Get Data
                    string content = GetProperty(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude, i.ToString());
                    Results results =   JsonConvert.DeserializeObject<Results>(content);
                    Pins pins =  JsonConvert.DeserializeObject<Pins>(content);

                    //Save Data
                    SaveResults(results.results);


                    
                }

            }






        }
    }
}
