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
            try
            {
                //客户端连接套接字
                Socket socket = listenSocket.EndAccept(ias);
                int index = GetIndex();
                if (index < 0)
                {
                    socket.Close();
                    Console.WriteLine("连接已满");
                    return;
                }
                //分配连接
                Conn conn = conns[index];
                conn.Init(socket);

                string address = conn.GetAddress();
                Console.WriteLine("The Client Address is:" + address);
                //异步接收数据
                socket.BeginReceive(conn.readBuffer, conn.bufferCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);


                listenSocket.BeginAccept(AcceptCb, null);
            }
            catch(Exception e)
            {
                Console.WriteLine("AcceptCb失败："+e.Message);
            }

        }


        //回调
        public void ReceiveCb(IAsyncResult ias)
        {
            Conn conn = (Conn)ias.AsyncState;
            lock(conn)
            {
                try
                {
                    int count = conn.socket.EndReceive(ias);
                    //接收到消息长度为0表示已经断开连接
                    if(count<=0)
                    {
                        Console.WriteLine(conn.GetAddress()+"断开连接");
                        conn.Close();
                        return;
                    }

                    //缓冲区消息总长度增加
                    conn.bufferCount = conn.bufferCount + count;
                    //处理消息，分发
                    ProcessData(conn);

                    //继续接受
                    conn.socket.BeginReceive(conn.readBuffer, conn.bufferCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);

                }
                catch(Exception e)
                {
                    Console.WriteLine(conn.GetAddress()+"接收消息失败:"+e.Message);
                }
            }
        }


        //处理数据
        public void ProcessData(Conn conn)
        {
            //缓冲区的数据长度小于4字节，，不是完整的消息
            if(conn.bufferCount<sizeof(Int32))
            {
                return;
            }

            //复制消息长度到lenBytes
            Array.Copy(conn.readBuffer, conn.lenBytes, sizeof(Int32));
            //获取数组内容获取消息长度
            conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0);

            //处理消息
            string msg = System.Text.Encoding.UTF8.GetString(conn.readBuffer,0,conn.msgLength);
            Console.WriteLine("收到消息："+msg);


            //清除已经处理的消息
            //未处理的消息长度
            int count = conn.bufferCount - (sizeof(Int32) + conn.msgLength);
            Array.Copy(conn.readBuffer, sizeof(Int32) + conn.msgLength, conn.readBuffer, 0, conn.bufferCount - count);

            //重新设置bufferCount
            conn.bufferCount = count;

            //递归调用
            if(conn.bufferCount>0)
            {
                ProcessData(conn);
            }

        }


        //发送消息
        public void Send(Conn conn,string msg)
        {
            //msg转为Byte数组
            byte[] msgBytes = System.Text.Encoding.UTF8.GetBytes(msg);
            //根据msgBytes数组的长度创建lenBytes数组
            byte[] lenBytes = BitConverter.GetBytes(msgBytes.Length);
            //拼接两个数组
            byte[] sendMsg = lenBytes.Concat(msgBytes).ToArray();

            try
            {
                //发送,无回调函数，不关心发送结果
                conn.socket.BeginSend(sendMsg, 0, sendMsg.Length, SocketFlags.None, null, null);
            }
            catch(Exception e)
            {
                Console.WriteLine("发送消息失败："+conn.GetAddress()+e.Message);
            }
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
