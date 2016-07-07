using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using WebSocketSharp;
using WebSocketSharp.Server;

using R.Common.Models;

using SharpRepository.MongoDbRepository;
using SharpRepository.Caching.Redis;

using Newtonsoft.Json;
using SharpRepository.Repository.Caching;

namespace R.Web.Services
{
    public class ResultBehavior:WebSocketBehavior
    {

        public IConfigurationRoot m_Configuration { get; set; }

        public MongoDbRepository<Result,string> m_MongoDbRepositoryResult { get; set; }

        public Position m_Position { get; set; }

        public List<Result> m_Results { get; set; }

        public ResultBehavior()
        {
            try { 
           //Configuration
            var builder = new ConfigurationBuilder().SetBasePath(System.IO.Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            m_Configuration = builder.Build();


               
            //MongoDBRepository
            m_MongoDbRepositoryResult = new MongoDbRepository<Result, string>(m_Configuration["ConnectionStrings:MongoDbConnection:ConnectionString"], new StandardCachingStrategy<Result, string>(new RedisCachingProvider(m_Configuration["ConnectionStrings:RedisConnection:ConnectionString"], 6379, false)));

            } catch(Exception exception) {

                Console.WriteLine(exception.ToString());

            }



}

protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            try
            {
                Console.WriteLine("Result:" + e.Data);
                Position p = JsonConvert.DeserializeObject<Position>(e.Data);

                if (p != null)
                {

                 m_Results =   m_MongoDbRepositoryResult.FindAll( x => Convert.ToDouble(x.Property.Address.Latitude) <= Convert.ToDouble(p.LatitudeMax)
                                                      &&  Convert.ToDouble(x.Property.Address.Latitude) >= Convert.ToDouble(p.LatitudeMin)
                                                      &&　 Convert.ToDouble(x.Property.Address.Longitude) <= Convert.ToDouble(p.LongitudeMax)
                                                      &&  Convert.ToDouble(x.Property.Address.Longitude) >= Convert.ToDouble(p.LongitudeMin)
                                                      ).ToList();

                }


                if (m_Results != null) {

                    Send(  JsonConvert.SerializeObject(m_Results));
                }


            }
            catch(Exception exception) {

                Console.WriteLine(exception.ToString());

            }


        }

    }
}
