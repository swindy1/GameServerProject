using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Core;
using GameServer.Logic;

namespace GameServer.Test
{
    class HandleMsgTest
    {
        public static void ExecuteHandleMsgTest()
        {
            //打开数据库连接
            DataHelper dataHelper = DataHelper.Instance;
            DataHelper.Instance.Connect();

            Conn conn = new Conn();
            ProtocolBytes protocolBytes = new ProtocolBytes();
            protocolBytes.AddString("Register");
            protocolBytes.AddString("wii");
            protocolBytes.AddString("1234");

            //处理注册消息
            ServNet.Instance.HandleMsg(conn, protocolBytes);

            protocolBytes = new ProtocolBytes();
            protocolBytes.AddString("Login");
            protocolBytes.AddString("wii");
            protocolBytes.AddString("1234");
            //处理登陆消息
            ServNet.Instance.HandleMsg(conn, protocolBytes);
        }
    }
}
