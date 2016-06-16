using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace r.mobile
{
    public class App : Application
    {

        public TabbedPage m_Tabs; 

        public App()
        {

            
          
            m_Tabs = new TabbedPage();
           
            m_Tabs.Children.Add(new MapPage());
            //main page
            MainPage = m_Tabs;



        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
