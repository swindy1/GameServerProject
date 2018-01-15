using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GameServer.Core
{
    //单例模式
    class ServNet
    {
        //监听套接字
        public Socket listenSocket;

        public Conn[] conns;

        public int maxCount=50;

        public static ServNet instance = new ServNet();

        private ServNet()
        {

        }

        public static ServNet Instance
        {
            get { return instance; }
        }


        //开启服务器
        public void Start(string host,int port)
        {
            //创建连接池
            conns = new Conn[maxCount];
            for(int i=0;i<maxCount;i++)
            {
                conns[i] = new Conn();
            }

            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint iPEndPoint = new IPEndPoint(ip, port);

            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //绑定ip
            listenSocket.Bind(iPEndPoint);

            //设置最大连接数
            listenSocket.Listen(maxCount);

            listenSocket.BeginAccept(AcceptCb,null);

            Console.WriteLine("服务器启动");

        }

        //Accept回调
        public void AcceptCb(IAsyncResult ias)
        {

        }

        //获取连接索引
        public int GetIndex()
        {
            if (conns == null)
                return -1;

            for(int i=0;i<maxCount;i++)
            {
                if (conns[i] == null)
                    return -2;
                else if (conns[i].isUse == false)
                    return i;
                
            }

            return -3;
        }


        //关闭连接前保存数据
        public void Close()
        {
            for(int i=0;i<maxCount;i++)
            {
                Conn conn = conns[i];
                if (conn == null)
                    continue;
                if (conn.isUse == false)
                    continue;
                //至少存在三个线程竞争
                lock (conn)
                    {
                        conn.Close();
            
                    };
            }
        }

    }
}
