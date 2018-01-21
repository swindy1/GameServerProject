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


        /// <summary>
        /// 注册协议处理
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="protoBase"></param>
        public void MsgRegister(Conn conn,ProtocolBase protoBase)
        {
            //开始读取的位置
            int start = 0;
            ProtocolBytes protoBytes = (ProtocolBytes)protoBase;
            string protoName = protoBytes.GetString(start, ref start);
            string id = protoBytes.GetString(start, ref start);
            string pw = protoBytes.GetString(start, ref start);


            //构建返回协议
            protoBytes = new ProtocolBytes();
            protoBytes.AddString(protoName);
            //注册成功返回0，失败返回-1
            if(DataHelper.Instance.Register(id,pw))
            {
                protoBytes.AddInt(0);
            }
            else
            {
                protoBytes.AddInt(-1);
            }

            //创建角色
            DataHelper.Instance.CreatePlayer(id);

            //返回协议给客户端
            conn.Send(protoBytes);
        }


        /// <summary>
        /// 登陆消息处理
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="protoBase"></param>
        public void MsgLogin(Conn conn,ProtocolBase protoBase)
        {
            //开始读取的位置
            int start = 0;
            ProtocolBytes protoBytes = (ProtocolBytes)protoBase;
            string protoName = protoBytes.GetString(start, ref start);
            string id = protoBytes.GetString(start, ref start);
            string pw = protoBytes.GetString(start, ref start);

            //构建返回协议
            protoBytes = new ProtocolBytes();
            protoBytes.AddString(protoName);

            //验证密码
            if(!DataHelper.Instance.LoginCheck(id,pw))
            {
                protoBytes.AddInt(-1);
                conn.Send(protoBytes);
                return;
            }

            //将在线的该玩家踢下线
            ProtocolBytes protoLogout = new ProtocolBytes();
            protoLogout.AddString("Logout");
            //如果踢下线失败
            if(!Player.KickOff(id,protoLogout))
            {
                protoBytes.AddInt(-1);
                conn.Send(protoBytes);
                return;
            }

            //获取玩家数据
            PlayerData playerData = DataHelper.Instance.GetPlayerData(id);
            //获取数据失败
            if(playerData==null)
            {
                protoBytes.AddInt(-1);
                conn.Send(protoBytes);
                return;
            }

            //conn与player互有
            conn.player = new Player(id, conn);
            conn.player.data = playerData;

            //触发登陆事件
            ServNet.Instance.handlePlayerEvent.OnLogin(conn.player);

            //登陆成功返回
            protoBytes.AddInt(1);
            conn.Send(protoBytes);
            return;

        }


        /// <summary>
        /// 登出消息处理，正常登出返回0
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="protoBase"></param>
        public void MsgLogout(Conn conn,ProtocolBase protoBase)
        {
            ProtocolBytes protoBytes = new ProtocolBytes();
            protoBytes.AddString("Logout");
            protoBytes.AddInt(0);


            //存在两种下线情况
            if(conn.player==null)
            {
                conn.Send(protoBytes);
                conn.Close();
                
            }
            else
            {
                //Logout方法调用Close，必须先Send
                conn.Send(protoBytes);
                conn.player.Logout();

            }
            
        }
    }
}
