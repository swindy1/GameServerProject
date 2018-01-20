using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using GameServer.Logic;
using System.Reflection;

namespace GameServer.Core
{
    //单例模式
    class ServNet
    {
        //监听套接字
        public Socket listenSocket;

        public Conn[] conns;

        public int maxCount=50;
        //主定时器
        public System.Timers.Timer timer = new System.Timers.Timer(1000);
        //心跳时间,180秒执行一次心跳
        public long heartBeatTime = 180;

        //应用协议,待定
        public ProtocolBase proto;
        //角色事件处理对象
        public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();
        //连接消息处理对象
        public HandleConnMsg handleConnMsg = new HandleConnMsg();
        //角色消息处理对象
        public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();

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
            //定时器
            timer.Elapsed += new System.Timers.ElapsedEventHandler(HanderMainTimer);
            timer.AutoReset = false;
            timer.Enabled = true;

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


       //主定时器
       public void HanderMainTimer(object sender,System.Timers.ElapsedEventArgs e)
        {
            //处理心跳
            HeartBeat();
            timer.Start();
        }
       
        //处理心跳
        public void HeartBeat()
        {
            Console.WriteLine("主定时器开始运行");

            //当前时间戳
            long timeNow = ServTime.GetTimeStamp();

            for(int i=0;i<conns.Length;i++)
            {
                if(conns[i].isUse)
                {
                    //超过心跳时间
                    if(timeNow-conns[i].lastTickTime>heartBeatTime)
                    {
                        Console.WriteLine("心跳引起连接断开"+conns[i].GetAddress());
                        lock(conns[i])
                        {
                            conns[i].Close();
                        }
                    }
                }
            }


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
            //string msg = System.Text.Encoding.UTF8.GetString(conn.readBuffer,0,conn.msgLength);
            //Console.WriteLine("收到消息："+msg);

            ProtocolBase protocol = proto.Decode(conn.readBuffer,sizeof(Int32),conn.msgLength);
            HandleMsg(conn, protocol);



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


        //处理消息
        public void HandleMsg(Conn conn,ProtocolBase protoBase)
        {
            /*
            //这里调用的是ProtocolBytes的GetName
            string name = protoBase.GetName();
            if(name=="HeartBeat")
            {
                Console.WriteLine("收到消息"+name);
                conn.lastTickTime = ServTime.GetTimeStamp();
            }
            Send(conn,protoBase);
           */


            string name = protoBase.GetName();
            string methodName = "Msg" + name;

            //连接消息处理
            if(conn.player==null||name=="HeartBeat"||name=="Logout")
            {
                //根据方法名称获取方法属性
                MethodInfo methodInfo = handleConnMsg.GetType().GetMethod(methodName);
                if(methodInfo==null)
                {
                    Console.WriteLine("HandMsg没有连接处理方法");
                        return;
                }
                //方法参数
                object[] objs = new object[] { conn, protoBase };
                //调用方法
                methodInfo.Invoke(handleConnMsg, objs);
                Console.WriteLine("处理连接消息");
            }
            //角色消息处理
            else
            {
                //根据方法名称获取方法属性
                MethodInfo methodInfo = handlePlayerEvent.GetType().GetMethod(methodName);
                if (methodInfo == null)
                {
                    Console.WriteLine("HandMsg没有处理角色方法");
                    return;
                }
                //方法参数
                object[] objs = new object[] { conn, protoBase };
                //调用方法
                methodInfo.Invoke(handlePlayerEvent, objs);
                Console.WriteLine("处理角色消息");
            }

        }



        //发送消息
        public void Send(Conn conn,ProtocolBase protocol)
        {
            //msg转为Byte数组
            byte[] msgBytes = protocol.Encode();
            //根据msgBytes数组的长度创建lenBytes数组
            byte[] lenBytes = BitConverter.GetBytes(msgBytes.Length);
            //拼接两个数组
            byte[] sendMsg = lenBytes.Concat(msgBytes).ToArray();

            try
            {
                conn.socket.BeginSend(sendMsg, 0, sendMsg.Length, SocketFlags.None, null, null);
            }
            catch(Exception e)
            {
                Console.WriteLine("发送消息失败"+e.Message);
            }
        }


        //消息广播
        public void Broadcast(ProtocolBase protocol)
        {
            for(int i=0;i<conns.Length; i++)
            {
                if (conns[i] == null)
                    continue;
                if (conns[i].isUse == false)
                    continue;
                Send(conns[i], protocol);
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
