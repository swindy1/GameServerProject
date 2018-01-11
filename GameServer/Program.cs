using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Core;
using GameServer.Logic;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            DataHelper dataHelper = DataHelper.Instance;
            string strTest=@"02";
            DataHelper.Instance.Connect();
            bool te = dataHelper.IsRegister(strTest);
            Console.WriteLine(strTest);
            Console.WriteLine(te);
        }
    }
}
