using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace R.Web.Services
{
    public class ResultBehavior:WebSocketBehavior
    {
        public ResultBehavior()
        {
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);

            Console.WriteLine("Result:"+e.Data);

      

        }

    }
}
