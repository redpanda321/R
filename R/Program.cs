using HtmlAgilityPack;
using R.Models;
using R.SimpleCrawler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R
{
    class Program
    {

        /// <summary>
        /// 蜘蛛爬虫的设置
        /// </summary>
        public static  readonly CrawlSettings Settings = new CrawlSettings();

        public  RContext Db = new RContext();

        public static BloomFilter<string> Filter;


        #region 程序开始处理
        /// <summary>
        /// 程序开始处理
        /// </summary>
        public static void ProcessStart()
        {

           
            Filter = new BloomFilter<string>(200000);
            #region 设置种子地址
            // 设置种子地址 
           
            Settings.SeedsAddress.Add("https://www.realtor.ca/Residential/Map.aspx#CultureId=1&ApplicationId=1&RecordsPerPage=9&MaximumResults=9&PropertySearchTypeId=1&TransactionTypeId=2&StoreyRange=0-0&BedRange=0-0&BathRange=0-0&LongitudeMin=-114.151184883728&LongitudeMax=-114.11814006866453&LatitudeMin=51.03494537104224&LatitudeMax=51.04247417551822&SortOrder=A&SortBy=1&viewState=g&Longitude=-114.13389&Latitude=51.039&CurrentPage=1");
            Settings.SeedsAddress.Add("https://www.realtor.ca/Residential/Single-Family/16524192/203-1900-25a-SW-Richmond-Calgary-Alberta-t3e1y5-Richmond");

            #endregion

            #region 设置 URL 关键字
            Settings.HrefKeywords.Add("a/");
            Settings.HrefKeywords.Add("b/");
            Settings.HrefKeywords.Add("c/");
            Settings.HrefKeywords.Add("d/");

            Settings.HrefKeywords.Add("e/");
            Settings.HrefKeywords.Add("f/");
            Settings.HrefKeywords.Add("g/");
            Settings.HrefKeywords.Add("h/");


            Settings.HrefKeywords.Add("i/");
            Settings.HrefKeywords.Add("j/");
            Settings.HrefKeywords.Add("k/");
            Settings.HrefKeywords.Add("l/");


            Settings.HrefKeywords.Add("m/");
            Settings.HrefKeywords.Add("n/");
            Settings.HrefKeywords.Add("o/");
            Settings.HrefKeywords.Add("p/");

            Settings.HrefKeywords.Add("q/");
            Settings.HrefKeywords.Add("r/");
            Settings.HrefKeywords.Add("s/");
            Settings.HrefKeywords.Add("t/");


            Settings.HrefKeywords.Add("u/");
            Settings.HrefKeywords.Add("v/");
            Settings.HrefKeywords.Add("w/");
            Settings.HrefKeywords.Add("x/");

            Settings.HrefKeywords.Add("y/");
            Settings.HrefKeywords.Add("z/");
            #endregion


            // 设置爬取线程个数
            Settings.ThreadCount = 1;

            // 设置爬取深度
            Settings.Depth = 55;

            // 设置爬取时忽略的 Link，通过后缀名的方式，可以添加多个
             Settings.EscapeLinks.Add(".jpg");

            // 设置自动限速，1~5 秒随机间隔的自动限速
            Settings.AutoSpeedLimit = false;

            // 设置都是锁定域名,去除二级域名后，判断域名是否相等，相等则认为是同一个站点
            Settings.LockHost = false;


            // 设置请求的 User-Agent HTTP 标头的值
            // settings.UserAgent 已提供默认值，如有特殊需求则自行设置

            // 设置请求页面的超时时间，默认值 15000 毫秒
            // settings.Timeout 按照自己的要求确定超时时间



            Settings.RegularFilterExpressions.Add(@"http://([w]{3}.)+[realtor]+.ca/");
            var master = new CrawlMaster(Settings);
            master.AddUrlEvent += MasterAddUrlEvent;
            master.DataReceivedEvent += MasterDataReceivedEvent;

            master.Crawl();

        }

        #endregion


        #region 解析HTML



        /// <summary>
        /// The master add url event.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static  bool MasterAddUrlEvent(AddUrlEventArgs args)
        {
            if (!Filter.Contains(args.Url))
            {
                Filter.Add(args.Url);
                Console.WriteLine(args.Url);

                

                return true;
            }

            return false; // 返回 false 代表：不添加到队列中
        }



        /// <summary>
        /// The master data received event.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public static void MasterDataReceivedEvent(SimpleCrawler.DataReceivedEventArgs args)
        {
            // 在此处解析页面，可以用类似于 HtmlAgilityPack（页面解析组件）的东东、也可以用正则表达式、还可以自己进行字符串分析

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(args.Html);


            string fileName = string.Format( DateTime.Now.Millisecond + ".html", "c:\\temp");
            doc.Save(fileName);


            /*

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//title");
            string title = node.InnerText;

            HtmlNode node2 = doc.DocumentNode.SelectSingleNode("//*[@id='post-date']");
            string time = node2.InnerText;

            HtmlNode node3 = doc.DocumentNode.SelectSingleNode("//*[@id='topics']/div/div[3]/a[1]");
            string author = node3.InnerText;

            HtmlNode node6 = doc.DocumentNode.SelectSingleNode("//*[@id='blogTitle']/h2");
            string motto = node6.InnerText;
            */


            

        }
        #endregion




         public static void Main(string[] args)
        {

            ProcessStart();
        }
    }
}
