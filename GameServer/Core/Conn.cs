using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using GameServer.Logic;

namespace GameServer.Core
{
    class Conn
    {
        public const int BUFFER_SIZE = 1024;

        public Socket socket;

        public byte[] readBuffer =null;
        //缓冲区消息总长度
        public int bufferCount = 0;
        //消息长度
        public int msgLength = 0;
        //粘包分包
        public byte[] lenBytes = new byte[sizeof(UInt32)];
        //是否启用
        public bool isUse = false;

        public long lastTickTime = long.MinValue;
        //对应角色
        public Player player;


        public Conn()
        {
            readBuffer = new byte[BUFFER_SIZE];
        }

        public void Init(Socket socket)
        {
            this.socket = socket;
            //已经启用
            isUse = true;
            //该类作为对象池对象循环使用，每次初始化一些字段需要重新设置
            bufferCount = 0;
            //设置心跳时间
            lastTickTime = ServTime.GetTimeStamp();
        }


        //剩余的缓冲区大小
        public int BuffRemain()
        {
            return BUFFER_SIZE - bufferCount;
        }

        //获取客户端地址
        public string GetAddress()
        {
            if (!isUse)
                return "Conn对象尚未启用";
            else return socket.RemoteEndPoint.ToString();
        }

        public void Close()
        {
            if (!isUse)
                return;
            if(player!=null)
            {
                //确保玩家退出操作
                player.Logout();
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            //socket = null;
            isUse = false;
            
        }


        //发送协议
        public void Send(ProtocolBase protoBase)
        {
            ServNet.Instance.Send(this, protoBase);
        }
    }
}
