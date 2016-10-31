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
using R.Common.Models;

using Microsoft.Extensions.Configuration;

using MongoDB.Driver;
using SharpRepository.MongoDbRepository;
using MongoDB.Bson;
using SharpRepository.Repository.Caching;
using SharpRepository.Caching.Redis;
using System.Data.Entity.Validation;
using System.Data.Entity.Migrations;

namespace R
{
    public class Program
    {
        static Program()
        {
            var builder = new ConfigurationBuilder().SetBasePath(System.IO.Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();

            Configuration = builder.Build();

           ApplicationDbContext.ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];

           Database.SetInitializer<ApplicationDbContext>(new CreateDatabaseIfNotExists<ApplicationDbContext>());
           Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, R.Migrations.Configuration>());

        }
        public static IConfigurationRoot Configuration { get; set; }

        public static System.Threading.Thread[] threads = new Thread[1024];

        /// <summary>
        /// Restful API
        /// </summary>
        public static string PROPERTYAPI = "https://64.26.131.68/api/Listing.svc/PropertySearch_Post";

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
        public static string GetProperty(string longitudeMin, string longitudeMax, string latitudeMin, string latitudeMax, string longitude, string latitude, string currentPage)
        {
            string content = "";

            try
            {
                var client = new RestClient(PROPERTYAPI);

               //  client.Proxy =  new WebProxy("http://127.0.0.1:8580");

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
                request.AddParameter("ZoomLevel", "11");

                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json charset=utf-8"; };
                client.AddHandler("application/json", new DynamicJsonDeserializer());
                content = client.Execute(request).Content;

                System.Console.WriteLine("content:" + content);
                System.Console.WriteLine("currentPage:" + currentPage);

            }
            catch (Exception e) {

                System.Console.WriteLine(e.ToString());
            }

            return content;
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
        /// <returns></returns>
        public static int GetPagingNumber(string longitudeMin, string longitudeMax, string latitudeMin, string latitudeMax, string longitude, string latitude)
        {

            string content = GetProperty(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude, "1");
            
            JObject o = JObject.Parse(content);
            int total = (int)o["Paging"]["TotalRecords"];

            int n1 = total / 9;
            int n2 = 0;
            if (total % 9 != 0)
                n2 = 1;

            int num = n1 + n2;

            System.Console.WriteLine("num:" + num);

            return num;
        }
        /// <summary>
        /// Get All Cookies
        /// </summary>
        /// <returns></returns>
        public static CookieContainer GetCookies()
        {

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
                System.Net.Cookie cookie = new System.Net.Cookie(c.Name, c.Value, c.Path, c.Domain);
                //if(c.Expiry.Value != null)
                // cookie.Expires = c.Expiry.Value;
                cookieContainer.Add(cookie);
            }

            // driver.Quit();

            return cookieContainer;

        }

        public static void SavePins(List<Pin> pins)
        {

            if (pins == null) return;
            if (pins.Count <= 0) return;

            try
            {
                var repo = new MongoDbRepository<Pin,string>(Configuration["Data:MongoDbConnection:ConnectionString"],new StandardCachingStrategy<Pin, string>(new RedisCachingProvider(Configuration["Data:MongoDbConnection:ConnectionString"], 6379,false)));
               // var repo = new MongoDbRepository<Pin, string>(Configuration["Data:MongoDbConnection:ConnectionString"], new StandardCachingStrategy<Pin, string>());


                foreach (var p in pins)
                {



                    if (p != null)
                    {

                        Pin dbPin = new Pin();
                       
                        dbPin = repo.Find(x=>x.key == p.key);
                        if (dbPin == null)
                        {
                            repo.Add(p);

                        }
                        else
                        {

                            repo.Update(p);
                        }

                    }


                }

                repo.Dispose();


                var server = new MongoClient(Configuration["Data:MongoDbConnection:ConnectionString"]).GetServer();
                var db = server.GetDatabase("Pin");
                var dbPins = db.GetCollection<Pin>("Pin");
                BsonDocument keys = new BsonDocument();
                keys.Add("key", 1);
                IMongoIndexKeys indexKeys = new IndexKeysDocument(keys);
                IndexOptionsDocument indexOptions = new IndexOptionsDocument();
                dbPins.CreateIndex(indexKeys,indexOptions);


            }
            catch (Exception e) {

                System.Console.WriteLine(e.ToString());   


             }

            #region EF6
            /*
            ApplicationDbContext db = new ApplicationDbContext();

            foreach (var p in pins)
            {


                try
                {

                    if (p != null)
                    {

                        Pin dbPin = new Pin();

                        dbPin = db.Pins.FirstOrDefault(dp => dp.key == p.key);

                        if (dbPin == null)
                        {

                            db.Pins.Add(p);

                        }
                        else
                        {
                        
                          //  db.Entry(dbPin).Entity.latitude = p.latitude;
                          //  db.Entry(dbPin).Entity.longitude = p.longitude;
                          //  db.Entry(dbPin).Entity.propertyId = p.propertyId;
                          //  db.Entry(dbPin).CurrentValues.SetValues(p);

                        }
                    }


                }
                catch (Exception e)
                {


                    System.Console.WriteLine(e);
                }

            }


            db.SaveChanges();
          */
            #endregion EF6

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="results"></param>
        public static void SaveResults(List<Result> results)
        {

            if (results == null) return;
            if (results.Count <= 0) return;

            #region EF6


            ApplicationDbContext dbx = new ApplicationDbContext();

            foreach (var rr in results)
            {

                //Result history
                #region result history
                try
                {
                    XResultHistory resultHistory1 = new XResultHistory();
                    resultHistory1.ResultDateTime = DateTime.Now;
                    resultHistory1.ResultId = rr.Id;
                    resultHistory1.Id = (int)DateTime.Now.Ticks;

                    string price1 = rr.Property.Price.Replace('$', ' ');
                    if (rr.Property.Price.IndexOf(',') >= 0)
                    {
                        string[] price = price1.Split(',');
                        price1 = price[0] + price[1];
                    }



                    resultHistory1.Price = Convert.ToSingle(price1);

                    dbx.XResultHistories.Add(resultHistory1);
                   // dbx.SaveChanges();

                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.ToString());


                }

                /*
                catch (DbEntityValidationException  e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }

                    //System.Console.WriteLine(e.ToString());


                }

                */
                #endregion result history


                //Related Objects

                Result dbResult = new Result();


                try
                {


                    dbResult = dbx.Results.Include(r => r.Building).Include(r => r.Land).Include(r => r.AlternateURL)
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
                catch (Exception e)
                {


                    System.Console.WriteLine(e.ToString());
                }



                if (dbResult == null)
                {
                    dbx.Results.Add(rr);
                }
                else
                {

                    #region  dbResult


                    try
                    {


                        dbx.Entry(dbResult).CurrentValues.SetValues(rr);

                        var dbBuilding = dbResult.Building;
                        var dbLand = dbResult.Land;
                        var dbAlternateURL = dbResult.AlternateURL;
                        var dbProperty = dbResult.Property;
                        var dbProAddress = dbResult.Property.Address;


                        if (rr.Building != null)
                        {
                            if (dbBuilding != null)
                            {

                                dbx.Entry(dbBuilding).Entity.BathroomTotal = rr.Building.BathroomTotal;
                            }
                            else
                            {
                                dbx.Buildings.Attach(rr.Building);
                                dbResult.Building = rr.Building;

                            }
                        }

                        if (rr.Land != null)
                        {
                            if (dbLand != null)
                            {


                                dbx.Entry(dbLand).Entity.LandscapeFeatures = rr.Land.LandscapeFeatures;
                            }
                            else
                            {

                                dbx.Lands.Attach(rr.Land);
                                dbResult.Land = rr.Land;

                            }
                        }


                        if (rr.AlternateURL != null)
                        {
                            if (dbAlternateURL != null)
                            {


                                dbx.Entry(dbAlternateURL).Entity.BrochureLink = rr.AlternateURL.BrochureLink;
                            }
                            else
                            {
                                try
                                {
                                    dbx.AlternateURLs.Attach(rr.AlternateURL);
                                    dbResult.AlternateURL = rr.AlternateURL;
                                }
                                catch { }
                            }
                        }


                        if (rr.Property != null)
                        {
                            if (dbProperty != null)
                            {


                                dbx.Entry(dbProperty).Entity.Price = rr.Property.Price;

                            }
                            else
                            {
                                dbx.Properties.Attach(rr.Property);
                                dbResult.Property = rr.Property;
                            }
                        }


                        if (rr.Property.Address != null)
                        {
                            if (dbProAddress != null)
                            {


                                dbx.Entry(dbProAddress).Entity.AddressText = rr.Property.Address.AddressText;
                            }
                            else
                            {

                                dbx.Address2s.Attach(rr.Property.Address);
                                dbResult.Property.Address = rr.Property.Address;
                            }
                        }


                        if (rr.Property.Parking != null && rr.Property.Parking.Count > 0)
                        {
                            foreach (var p in rr.Property.Parking.ToList())
                            {

                                if (dbProperty.Parking.ToList() != null)
                                {

                                    foreach (var dbProParking in dbProperty.Parking.ToList())
                                    {



                                        dbx.Entry(dbProParking).Entity.Spaces = p.Spaces;
                                    }

                                }
                                else
                                {

                                    dbx.Parkings.Attach(p);
                                    dbProperty.Parking.Add(p);


                                }
                            }
                        }

                        if (rr.Property.Photo != null && rr.Property.Photo.Count > 0)
                        {
                            foreach (var p in rr.Property.Photo.ToList())
                            {

                                if (dbProperty.Photo.ToList() != null)
                                {
                                    foreach (var dbPhoto in dbProperty.Photo.ToList())
                                    {


                                        dbx.Entry(dbPhoto).Entity.HighResPath = p.HighResPath;
                                    }

                                }
                                else
                                {
                                    dbx.Photoes.Attach(p);
                                    dbProperty.Photo.Add(p);
                                }
                            }
                        }

                        if (rr.Individual != null && rr.Individual.Count > 0)
                        {
                            foreach (var i in rr.Individual.ToList())
                            {
                                var dbIndividual = dbResult.Individual.SingleOrDefault(In => In.IndividualID == i.IndividualID);
                                if (dbIndividual != null)
                                {


                                    dbx.Entry(dbIndividual).Entity.LastName = i.LastName;

                                    if (i.Phones != null && i.Phones.Count > 0)
                                    {
                                        foreach (var p in i.Phones.ToList())
                                        {


                                            if (dbIndividual.Phones.ToList() != null)
                                            {
                                                foreach (var dbIndividualPhone in dbIndividual.Phones.ToList())
                                                {

                                                    dbx.Entry(dbIndividualPhone).Entity.PhoneNumber = p.PhoneNumber;
                                                }
                                            }
                                            else
                                            {
                                                dbx.Phone2s.Attach(p);
                                                dbIndividual.Phones.Add(p);
                                            }


                                        }
                                    }


                                    if (i.Emails != null && i.Emails.Count > 0)
                                    {
                                        foreach (var e in i.Emails.ToList())
                                        {
                                            if (dbIndividual.Emails.ToList() != null)
                                            {
                                                foreach (var dbIndividualEmail in dbIndividual.Emails.ToList())
                                                {


                                                    dbx.Entry(dbIndividualEmail).Entity.ContactId = e.ContactId;
                                                }
                                            }
                                            else
                                            {
                                                dbx.Email2s.Attach(e);
                                                dbIndividual.Emails.Add(e);
                                            }

                                        }
                                    }

                                    if (i.Websites != null && i.Websites.Count > 1)
                                    {
                                        foreach (var w in i.Websites.ToList())
                                        {
                                            if (dbIndividual.Websites.ToList() != null)
                                            {
                                                foreach (var dbIndividualWebsite in dbIndividual.Websites.ToList())
                                                {


                                                    dbx.Entry(dbIndividualWebsite).Entity.Website = w.Website;
                                                }
                                            }
                                            else
                                            {
                                                dbx.Website2s.Attach(w);
                                                dbIndividual.Websites.Add(w);
                                            }


                                        }
                                    }

                                    var dbOrganization = dbIndividual.Organization;
                                    if (i.Organization != null)
                                    {
                                        if (dbOrganization != null)
                                        {



                                            dbx.Entry(dbOrganization).Entity.Name = i.Name;


                                            var dbOrganizationAddress = dbIndividual.Organization.Address;

                                            if (i.Organization.Address != null)
                                            {
                                                if (dbOrganizationAddress != null)
                                                {



                                                    dbx.Entry(dbOrganizationAddress).Entity.AddressText = i.Organization.Address.AddressText;
                                                }
                                                else
                                                {

                                                    dbx.Addresses.Attach(i.Organization.Address);
                                                    dbOrganization.Address = i.Organization.Address;

                                                }
                                            }

                                            if (i.Organization.Phones != null && i.Organization.Phones.Count > 0)
                                            {
                                                foreach (var p in i.Organization.Phones.ToList())
                                                {

                                                    if (dbOrganization.Phones.ToList() != null)
                                                    {
                                                        foreach (var dbOrganizationPhone in dbOrganization.Phones.ToList())
                                                        {


                                                            dbx.Entry(dbOrganizationPhone).Entity.PhoneNumber = p.PhoneNumber;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        dbx.Phones.Attach(p);
                                                        dbOrganization.Phones.Add(p);
                                                    }
                                                }
                                            }

                                            if (i.Organization.Emails != null && i.Organization.Emails.Count > 0)
                                            {
                                                foreach (var e in i.Organization.Emails.ToList())
                                                {
                                                    if (dbOrganization.Emails.ToList() != null)
                                                    {
                                                        foreach (var dbOrganizationEmail in dbOrganization.Emails.ToList())
                                                        {


                                                            dbx.Entry(dbOrganizationEmail).Entity.ContactId = e.ContactId;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        dbx.Emails.Attach(e);
                                                        dbOrganization.Emails.Add(e);
                                                    }
                                                }

                                            }

                                            if (i.Organization.Websites != null && i.Organization.Websites.Count > 0)
                                            {
                                                foreach (var w in i.Organization.Websites.ToList())
                                                {
                                                    if (dbOrganization.Websites.ToList() != null)
                                                    {
                                                        foreach (var dbOrganizationWebsite in dbOrganization.Websites.ToList())
                                                        {


                                                            dbx.Entry(dbOrganizationWebsite).Entity.Website = w.Website;

                                                        }
                                                    }
                                                    else
                                                    {
                                                        dbx.webSite1s.Attach(w);
                                                        dbOrganization.Websites.Add(w);
                                                    }
                                                }
                                            }

                                        }
                                        else
                                        {

                                            dbx.Organizations.Attach(i.Organization);
                                            dbIndividual.Organization = i.Organization;

                                        }
                                    }
                                }
                                else
                                {


                                    dbx.Individuals.Attach(i);
                                    dbResult.Individual.Add(i);

                                }



                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e.ToString());

                    }


                    #endregion dbResult

                }
               // dbx.SaveChanges();



               


            }


            //dbx.SaveChangesAsync();
            dbx.SaveChanges();
            dbx.Dispose();

            #endregion EF6



            #region  mongodb

            var repo1 = new MongoDbRepository<ResultHistory, string>(Configuration["Data:MongoDbConnection:ConnectionString"],new StandardCachingStrategy<ResultHistory,string>( new RedisCachingProvider(Configuration["Data:RedisConnection:ConnectionString"], 6379,false)));
             var repo = new MongoDbRepository<Result, string>(Configuration["Data:MongoDbConnection:ConnectionString"], new StandardCachingStrategy<Result, string>(new RedisCachingProvider(Configuration["Data:RedisConnection:ConnectionString"], 6379,false)));

          //  var repo1 = new MongoDbRepository<ResultHistory, string>(Configuration["Data:MongoDbConnection:ConnectionString"], new StandardCachingStrategy<ResultHistory, string>());
          //  var repo = new MongoDbRepository<Result, string>(Configuration["Data:MongoDbConnection:ConnectionString"], new StandardCachingStrategy<Result, string>());


            foreach (var r in results)
            {

                ResultHistory resultHistory = new ResultHistory();
                Result dbResult = new Result();
         
                try {


                    if (r != null)
                    {


                        //Result

                        string price1 = r.Property.Price.Replace('$', ' ');
                        if (r.Property.Price.IndexOf(',') >= 0)
                        {
                            string[] price = price1.Split(',');
                            price1 = "";
                            for (int i = 0; i < price.Count(); i++)
                            {

                                price1 = price1 + price[i];

                            }

                        }
                        
                        r.Property.Price = price1;


                        try
                        {

                            dbResult = repo.Find(x => x.MlsNumber == r.MlsNumber);
                            if (dbResult == null)
                            {
                                repo.Add(r);

                            }
                            else
                            {
                                repo.Update(r);
                            }
                        }
                        catch(Exception e) {

                            System.Console.WriteLine(e.ToString());
                        }

                        //ResultHistory

                        try
                        {

                            resultHistory.ResultDateTime = DateTime.Now;
                            resultHistory.ResultId = r.Id;
                            resultHistory.Result = r;
                            resultHistory.Price = Convert.ToSingle(price1);

                            repo1.Add(resultHistory);

                        }
                        catch (Exception e) {

                            System.Console.WriteLine(e.ToString());
                        }
                    }
                    


                }
                catch (Exception e)
                {

                    System.Console.WriteLine(e.ToString());

                }



            }

            repo.Dispose();
            repo1.Dispose();

            //creare index
            var server = new MongoClient(Configuration["Data:MongoDbConnection:ConnectionString"]).GetServer();
            var db = server.GetDatabase("Result");
            var dbResults = db.GetCollection<Result>("Result");

            var db1 = server.GetDatabase("ResultHistory");
            var dbResultHistory = db1.GetCollection<ResultHistory>("ResultHistory");



            BsonDocument keys = new BsonDocument();
            keys.Add("MlsNumber", 1);
            keys.Add("Property.Address.Longitude", 1);
            keys.Add("Property.Address.Lantitude", 1);
            IMongoIndexKeys indexKeys = new IndexKeysDocument(keys);
            IndexOptionsDocument indexOptions = new IndexOptionsDocument();

            dbResults.CreateIndex(indexKeys, indexOptions);

            keys.Clear();
            keys.Add("Result.MlsNumber", 1);
            indexKeys = new IndexKeysDocument(keys);
            
            dbResultHistory.CreateIndex(indexKeys, indexOptions);

            repo.Dispose();
            repo1.Dispose();

            #endregion



        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="longitudeMin"></param>
        /// <param name="longitudeMax"></param>
        /// <param name="latitudeMin"></param>
        /// <param name="latitudeMax"></param>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void ProcessThing(string longitudeMin, string longitudeMax, string latitudeMin, string latitudeMax, string longitude, string latitude, int from, int to)
        {


            if (from > 0 & to > 0)
            {

                if (to > from)
                {


                    for (int i = from; i <= to; i++)
                    {
                       
                        //Sleep 1~5 s
                       // Random random = new Random();
                       // var span = random.Next(1000, 5000);
                       // Thread.Sleep(span);


                        //Get data
                        string content = GetProperty(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude, i.ToString());
                        Results results = JsonConvert.DeserializeObject<Results>(content);
                        Pins pins = JsonConvert.DeserializeObject<Pins>(content);

                        //Save pins data
                        try
                        {
                          //  if (pins.pins != null & pins.pins.Count > 0)
                          //      SavePins(pins.pins);
                        }
                        catch(Exception e) {
                            System.Console.WriteLine(e.ToString());
                        }

                        //Save results data

                        try
                        {
                            if (results.results != null & results.results.Count > 0)
                                SaveResults(results.results);
                        }
                        catch (Exception e) { System.Console.WriteLine(e.ToString()); }



                    }


                }

            }


        }
        /// <summary>
        /// Craweler
        /// </summary>
        public static void ThingsTodo()
        {

            #region  data
            //s
            /*
            string longitudeMin = "-114.145241108551";
            string longitudeMax = "-114.1139558226013";
            string latitudeMin = "51.03625085626268";
            string latitudeMax = "51.04243032849644";
            string longitude = "-114.13389";
            string latitude = "51.039";
            */

            /*
             //all  
            String longitudeMax=180
            String latitudeMin=-67.91660022479132
            String latitudeMax=82.48055792406726
            String longitude=-113.914628
            String latitude=50.897983

            
             "Position": {

              "LongitudeMax": "180",
              "LongitudeMin": "-67.91660022479132",
              "LatitudeMax": "82.48055792406726",
              "LatitudeMin": "50.051219586625",
              "Longitude": "-113.914628",
              "Latitude": "50.897983"
            }

            
            */


            /*
            String longitudeMin=”-115.90212547926488”;
            String longitudeMax=”-110.82094872145238”;
            String latitudeMin=”50.051219586625”;
            String latitudeMax=”51.523515704948416”;
            String longitude=”-113.914628”;
            String latitude=”50.897983”;

            "Position": {

            "LongitudeMin": "-115.90212547926488",
            "LongitudeMax": "-110.82094872145238",
            "LatitudeMin": "50.051219586625",
            "LatitudeMax": "51.523515704948416",
            "Longitude": "-113.914628",
            "Latitude": "50.897983"
            }
            */
            #endregion

            String longitudeMin = Configuration["Geo:Position:LongitudeMin"];
            String longitudeMax = Configuration["Geo:Position:LongitudeMax"];
            String latitudeMin = Configuration["Geo:Position:LatitudeMin"];
            String latitudeMax = Configuration["Geo:Position:LatitudeMax"];
            String longitude = Configuration["Geo:Position:Longitude"];
            String latitude = Configuration["Geo:Position:Latitude"];

            //client.CookieContainer = GetCookies();

            int factor = Convert.ToInt32(Configuration["Task:Factor"]);

            //Random random = new Random();
            //factor = random.Next(260, 360);
               
            
            int num = GetPagingNumber(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude);

            int n = 0;
            int m = 0;

            Dictionary<int, int> dic = new Dictionary<int, int>();


            if (num > 0 & factor > 0)
            {
                n = num / factor;
                m = num % factor;

                if (n == 0 || ((n == 1) & (m == 0)))
                {

                    threads = new Thread[1];
                    threads[0] = new Thread(() => ProcessThing(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude, 1, num));
                    threads[0].IsBackground = true;
                    threads[0].Start();

                }
                else if ((n == 1) & (m > 0))
                {

                    threads = new Thread[2];
                    dic.Add(1, factor);
                    dic.Add(factor+1, num);


                    for (int i = 0; i < dic.Count; i++)
                    {
                        var entry = dic.ElementAt(i);
                        threads[i] = new Thread(() => ProcessThing(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude, entry.Key, entry.Value));
                        threads[i].IsBackground = true;
                        threads[i].Start();
                    }



                }
                else if (n > 1)
                {

                    
                    threads = new Thread[n + 1];
                    dic.Add(1, factor);

                    for (int i = 2; i <= n; i++)
                    {

                        dic.Add(1 + (i - 1) * factor, i * factor);

                    }

                    if (m > 0)
                    dic.Add(n * factor + 1, num);

                    
                    for (int j = 0; j < dic.Count; j++)
                    {
                        var entry = dic.ElementAt(j);
                        threads[j] = new Thread(() => ProcessThing(longitudeMin, longitudeMax, latitudeMin, latitudeMax, longitude, latitude, entry.Key, entry.Value));
                        threads[j].IsBackground = true;
                        threads[j].Start();
                    }
                    
                }



            }


            bool live = true ;

            do
            {

                live = false ;

                for (int k = 0; k < threads.Count(); k++)
                {

                    if (threads[k].IsAlive)
                        live = true;                
                }

                Thread.Sleep(1);

            } while (live);


        }

        public static void ThingsTodoTask()
        {


            var DailyTime = Configuration["Task:Timer"];
            var timeParts = DailyTime.Split(new char[1] { ':' });


            while (true)
            {
                var dateNow = DateTime.Now;
                var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));

                TimeSpan ts;
                if (date > dateNow)
                {
                    ts = date - dateNow;
                }
                else
                {

                    date = date.AddDays(1);
                    ts = date - dateNow;
                }

                System.Console.WriteLine("dateNow:" + dateNow.ToString());
                System.Console.WriteLine("date:" + date.ToString());
                System.Console.WriteLine("Ts:" + ts.ToString());

                Task.Delay(ts).Wait();
                ThingsTodo();


            }

        }

        public static void Main(string[] args)
        {

            if( Configuration["Task:True"] == "false")
            { 
               ThingsTodo();

            }else if (Configuration["Task:True"] == "true")
             {  
               ThingsTodoTask();
            }
        }
    }
}