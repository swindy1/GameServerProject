using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Core;
namespace GameServer.Logic
{
    class HandleConnMsg
    {
        /// <summary>
        /// 心跳协议处理
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="protoBase"></param>
        public void MsgHeartBeat(Conn conn,ProtocolBase protoBase)
        {
            conn.lastTickTime = ServTime.GetTimeStamp();
            Console.WriteLine("更新心跳时间:"+conn.GetAddress());
        }
    }
}
