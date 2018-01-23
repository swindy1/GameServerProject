using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Core;
using GameServer.Logic;

namespace GameServer.Test
{
    class DataHelperTest
    {
        public static void ExecuteTest1()
        {
            DataHelper dataHelper = DataHelper.Instance;
            DataHelper.Instance.Connect();

            bool reg = dataHelper.Register("swindy", "1234");
            if (reg)
            {
                Console.WriteLine("注册成功");
            }
            else
                Console.WriteLine("注册失败");

            bool create = dataHelper.CreatePlayer("swindy");
            if (create)
                Console.WriteLine("创建玩家成功");
            else
                Console.WriteLine("创建玩家失败");

            //获取玩家数据
            PlayerData pd = dataHelper.GetPlayerData("swindy");
            if (pd != null)
            {
                Console.WriteLine("获取玩家数据成功：score=" + pd.score);
            }
            else
                Console.WriteLine("获取玩家数据失败");

            //更改玩家数据
            pd.score = 20;

            Player p = new Player("swindy",null);
            p.id = "swindy";
            p.data = pd;

            bool save = dataHelper.SavePlayer(p);
            if (save)
                Console.WriteLine("保存成功");
            else
                Console.WriteLine("保存失败");

            //登陆检测
            bool login = dataHelper.LoginCheck("swindy", "1234");
            if (login)
                Console.WriteLine("登陆成功");
            else
                Console.WriteLine("登陆失败");
        }
    }
}
