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
using Newtonsoft.Json.Linq;

using R.Models;
using Microsoft.Framework.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace R
{
    public  class  Program
    {
        
        static Program()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, R.Migrations.Configuration>());

            ApplicationDbContext.ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];
        }

        public static  IConfigurationRoot Configuration { get; set; }

        public  void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ApplicationDbContext>();
        }

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
        public static ApplicationDbContext db = new ApplicationDbContext();
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

            client = new RestClient(PROPERTYAPI);
            request = new RestRequest();

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

            string content = client.Execute(request).Content;

            System.Console.WriteLine("content:" + content);

            return  content ;

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

            string content = GetProperty(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude, "1");
           

            JObject o = JObject.Parse(content);
            int num = (int)o["Paging"]["TotalPages"];
            System.Console.WriteLine("num:" + num);

            return num;

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

           // driver.Quit();

            return cookieContainer;

        }


        public static void SaveResults(List<Result> results)
        {

            ApplicationDbContext db = new ApplicationDbContext();

            foreach (var rr in results)
            {

                //Related Objects
               
                Result dbResult = new Result();

                try
                {

                    
                dbResult = db.Results.Include(r => r.Building).Include(r => r.Land).Include(r => r.AlternateURL)
                .Include(r => r.Property).Include(r => r.Property.Address).Include(r => r.Property.Parking)
                .Include(r => r.Property.Photo)
                .Include(r => r.Individual)
                 .Include(r => r.Individual.Select(v => v.Organization))
                .Include(r => r.Individual.Select(v => v.Organization.Address))
                .Include(r => r.Individual.Select(v => v.Organization.Phones))
                .Include(r => r.Individual.Select(v => v.Organization.Emails))
                .Include(r => r.Individual.Select(v => v.Organization.Websites))
                .Include(r => r.Individual.Select(v => v.Phones))
                .Include(r => r.Individual.Select(v => v.Websites))
                .Include(r => r.Individual.Select(v => v.Emails)).SingleOrDefault(r => r.Id == rr.Id);
                


                }
                catch (Exception e) {


                    System.Console.WriteLine(e.ToString());
                }



                if (dbResult == null)
                {
                    db.Results.Add(rr);
                }
                else
                {

                    db.Entry(dbResult).CurrentValues.SetValues(rr);

                    var dbBuilding = dbResult.Building;
                    var dbLand = dbResult.Land;
                    var dbAlternateURL = dbResult.AlternateURL;
                    var dbProperty = dbResult.Property;
                    var dbProAddress = dbResult.Property.Address;

                   
                    if (dbBuilding != null)
                    {
                        rr.Building.Id = dbBuilding.Id;

                        db.Entry(dbBuilding).CurrentValues.SetValues(rr.Building);
                    }else
                    {
                        db.Buildings.Attach(rr.Building);
                        dbResult.Building = rr.Building;

                    }

                    if (dbLand != null)
                    {
                        rr.Land.Id = dbLand.Id;
                        db.Entry(dbLand).CurrentValues.SetValues(rr.Land);
                    }
                    else {

                        db.Lands.Attach(rr.Land);
                        dbResult.Land = rr.Land;

                    }

                    if (dbAlternateURL != null)
                    {
                        rr.AlternateURL.Id = dbAlternateURL.Id;
                        db.Entry(dbAlternateURL).CurrentValues.SetValues(rr.AlternateURL);
                    }
                    else {

                        db.AlternateURLs.Attach(rr.AlternateURL);
                        dbResult.AlternateURL = rr.AlternateURL;
                    }

                    if (dbProperty != null)
                    {

                        rr.Property.Id = dbProperty.Id;
                        db.Entry(dbProperty).CurrentValues.SetValues(rr.Property);

                    }
                    else {
                        db.Properties.Attach(rr.Property);
                        dbResult.Property = rr.Property;


                    }


                    if (dbProAddress != null)
                    {
                        rr.Property.Address.Id = dbProAddress.Id;
                        db.Entry(dbProAddress).CurrentValues.SetValues(rr.Property.Address);
                    }
                    else {

                        db.Address2s.Attach(rr.Property.Address);
                        dbResult.Property.Address = rr.Property.Address;

                    }




                    foreach (var p in rr.Property.Parking.ToList())
                    {

                        if (dbProperty.Parking.ToList() != null)
                        {

                            foreach (var dbProParking in dbProperty.Parking.ToList())
                            {
                                p.Id = dbProParking.Id;

                                db.Entry(dbProParking).CurrentValues.SetValues(p);
                            }

                        }
                        else {

                            db.Parkings.Attach(p);
                            dbProperty.Parking.Add(p);
                            

                        }
                    }

                    foreach (var p in rr.Property.Photo.ToList())
                    {

                        if(dbProperty.Photo.ToList() != null)
                        { 
                            foreach ( var dbPhoto in dbProperty.Photo.ToList())
                            {
                                p.Id = dbPhoto.Id;             
                                db.Entry(dbPhoto).CurrentValues.SetValues(p);
                            }

                        }else
                        {
                            db.Photoes.Attach(p);
                            dbProperty.Photo.Add(p);
                        }
                    }

                    foreach (var i in rr.Individual.ToList())
                    {
                        var dbIndividual = dbResult.Individual.SingleOrDefault(In => In.IndividualID == i.IndividualID);
                        if (dbIndividual != null)
                        {

                            i.Id = dbIndividual.Id;
                            db.Entry(dbIndividual).CurrentValues.SetValues(i);

                            
                            foreach (var p in i.Phones.ToList())
                            {


                                if(dbIndividual.Phones.ToList() != null)
                                { 
                                   foreach( var dbIndividualPhone in dbIndividual.Phones.ToList())
                                   {
                                        p.Id = dbIndividualPhone.Id;

                                        db.Entry(dbIndividualPhone).CurrentValues.SetValues(p);
                                   }
                                }
                                else
                                {
                                    db.Phone2s.Attach(p);
                                    dbIndividual.Phones.Add(p);
                                }


                            }

                            foreach (var e in i.Emails.ToList())
                            {
                                if (dbIndividual.Emails.ToList() != null)
                                {
                                    foreach (var dbIndividualEmail in dbIndividual.Emails.ToList())
                                    {
                                        e.Id = dbIndividualEmail.Id;
                                        db.Entry(dbIndividualEmail).CurrentValues.SetValues(e);
                                    }
                                }
                                else
                                {
                                    db.Email2s.Attach(e);
                                    dbIndividual.Emails.Add(e);
                                }

                            }

                            foreach (var w in i.Websites.ToList())
                            {
                                if (dbIndividual.Websites.ToList() != null)
                                {
                                    foreach (var dbIndividualWebsite in dbIndividual.Websites.ToList())
                                    {
                                        w.Id = dbIndividualWebsite.Id;
                                        db.Entry(dbIndividualWebsite).CurrentValues.SetValues(w);
                                    }
                                }
                                else
                                {
                                    db.Website2s.Attach(w);
                                    dbIndividual.Websites.Add(w);
                                }


                            }
                            

                            var dbOrganization = dbIndividual.Organization;

                            if (dbOrganization != null)
                            {
                                i.Organization.Id = dbOrganization.Id;

                                db.Entry(dbOrganization).CurrentValues.SetValues(i.Organization);
                                

                                var dbOrganizationAddress = dbIndividual.Organization.Address;
                                if (dbOrganizationAddress != null)
                                {
                                    i.Organization.Address.Id = dbOrganizationAddress.Id;

                                    db.Entry(dbOrganizationAddress).CurrentValues.SetValues(i.Organization.Address);
                                }
                                else {

                                    db.Addresses.Attach(i.Organization.Address);
                                    dbOrganization.Address = i.Organization.Address;

                                }

                                foreach (var p in i.Organization.Phones.ToList())
                                {

                                    if( dbOrganization.Phones.ToList() != null)
                                    {  
                                      foreach( var dbOrganizationPhone in  dbOrganization.Phones.ToList())
                                        {
                                            p.Id = dbOrganizationPhone.Id; 
                                            db.Entry(dbOrganizationPhone).CurrentValues.SetValues(p);
                                        }
                                    }
                                    else
                                    {
                                        db.Phones.Attach(p);
                                        dbOrganization.Phones.Add(p);
                                    }
                                }

                                foreach (var e in i.Organization.Emails.ToList())
                                {
                                    if (dbOrganization.Emails.ToList() != null)
                                    {
                                        foreach (var dbOrganizationEmail in dbOrganization.Emails.ToList())
                                        {
                                            e.Id = dbOrganizationEmail.Id;
                                            db.Entry(dbOrganizationEmail).CurrentValues.SetValues(e);
                                        }
                                        }
                                    else
                                    {
                                        db.Emails.Attach(e);
                                        dbOrganization.Emails.Add(e);
                                    }
                                }

                                foreach (var w in i.Organization.Websites.ToList())
                                {
                                    if (dbOrganization.Websites.ToList() != null)
                                    {
                                        foreach (var dbOrganizationWebsite in dbOrganization.Websites.ToList())
                                        {
                                            w.Id = dbOrganizationWebsite.Id;
                                            db.Entry(dbOrganizationWebsite).CurrentValues.SetValues(w);

                                        }
                                    }
                                    else
                                    {
                                        db.webSite1s.Attach(w);
                                        dbOrganization.Websites.Add(w);
                                    }
                                }

                            }
                            else {

                                db.Organizations.Attach(i.Organization);
                                dbIndividual.Organization = i.Organization;

                            }

                        }
                        else {


                            db.Individuals.Attach(i);
                            dbResult.Individual.Add(i);

                        }



                    }

                }

            }


            db.SaveChanges();

        }


        public static void Main(string[] args)
        {
           
            string longitudeMin = "-114.145241108551";
            string longitudeMax = "-114.1139558226013";
            string latitudeMin = "51.03625085626268";
            string latitudeMax = "51.04243032849644";
            string longitude ="-114.13389";
            string latitude = "51.039";


           //client.CookieContainer = GetCookies();
            
           int num = GetPagingNumber(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude);

            if (num > 0)
            {
                for (int i = 1; i <= num; i++)
                {

                    //Sleep 1~10 s
                    var span = random.Next(1000, 10000);
                    Thread.Sleep(span);
                    
                    //Get Data
                     string   content = GetProperty(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude, i.ToString());
                     Results results =   JsonConvert.DeserializeObject<Results>(content);
                     Pins  pins =  JsonConvert.DeserializeObject<Pins>(content);

                    //Save Data
                    SaveResults(results.results);


                    
                }

            }


            

        }
    }
}
