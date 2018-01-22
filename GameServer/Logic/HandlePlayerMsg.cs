using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Core;
namespace GameServer.Logic
{
    class HandlePlayerMsg
    {
        /// <summary>
        /// 获取分数消息处理
        /// </summary>
        /// <param name="player"></param>
        /// <param name="protoBase"></param>
        public void MsgGetScore(Player player,ProtocolBase protoBase)
        {
            /*
             //索引
             int start = 0;
             ProtocolBytes protoBytes = (ProtocolBytes)protoBase;
             //获取协议名称
             string protoName = protoBytes.GetString(start,ref start);
             //构建新的返回协议
             protoBytes = new ProtocolBytes();
             protoBytes.AddString(protoName);
             */
            ProtocolBytes protoBytes = (ProtocolBytes)protoBase;

            //添加分数
            protoBytes.AddInt(player.data.score);
            player.Send(protoBytes);
            Console.WriteLine("MsgGetScore:"+player.id+"|"+player.data.score);
        }


        /// <summary>
        /// 增加分数消息处理
        /// </summary>
        /// <param name="player"></param>
        /// <param name="protoBase"></param>
        public void MsgAddScore(Player player,ProtocolBase protoBase)
        {
            player.data.score += 1;
            Console.WriteLine("MsgAddScore:"+player.id+"|"+player.data.score);
        }
    }
}
