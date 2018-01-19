using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Logic;

namespace GameServer.Core
{
    class Player
    {
        public string id;
        public Conn conn;

        //用户数据
        public PlayerData data;
        //临时数据
        public PlayerTempData tempData;


        public Player(string id,Conn conn)
        {
            this.id = id;
            this.conn = conn;
            tempData = new PlayerTempData();

        }

        //发送消息
        public void Send(ProtocolBase proto)
        {
            if (conn == null)
                return;
            if (conn.isUse == false)
                return;
            //封装
            ServNet.Instance.Send(conn, proto);
        }

        //下线
        public bool Logout()
        {
            //保存角色数据
            if (DataHelper.Instance.SavePlayer(this) == false)
                return false;
            //设置conn状态
            conn.player = null;
            conn.Close();
            return true;

        }

        //将玩家踢下线
        public static bool KickOff(string id,ProtocolBase proto)
        {
            Conn[] conns = ServNet.Instance.conns;
            for(int i=0;i<conns.Length;i++)
            {
                if (conns[i] == null)
                    continue;
                if (conns[i].isUse == false)
                    continue;
                if (conns[i].player == null)
                    continue;
                if (conns[i].player.id == id)
                {
                    //与连接的消息处理不在同一线程
                    lock(conns[i].player)
                    {
                        if (proto != null)
                            conns[i].player.Send(proto);
                        return conns[i].player.Logout();
                    }
                   
                }
            }
            return true;
        }

    }
}
