using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

using OpenQA.Selenium.Support.UI;

namespace R
{
    public class Program
    {
        public static void Main(string[] args)
        {

            IWebDriver driver = new FirefoxDriver();

            driver.Navigate().GoToUrl("https://www.realtor.ca/Residential/Map.aspx#CultureId=1&ApplicationId=1&RecordsPerPage=9&MaximumResults=9&PropertySearchTypeId=1&TransactionTypeId=2&StoreyRange=0-0&BedRange=0-0&BathRange=0-0&LongitudeMin=-114.13666876853941&LongitudeMax=-114.12212046684263&LatitudeMin=51.03741800312255&LatitudeMax=51.04181646583393&SortOrder=A&SortBy=1&viewState=g&Longitude=-114.13389&Latitude=51.039&CurrentPage=1");

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until((d) => { return d.Title.ToLower().StartsWith("Property Search"); });

            System.Console.WriteLine("title is :" + driver.Title);


            driver.Quit();

          
           
        }
    }
}
