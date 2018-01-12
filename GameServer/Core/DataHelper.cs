using System;
using System.Collections.Generic;
using System.Text;
using MySql;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using GameServer.Logic;

namespace GameServer.Core
{
    //单例模式
    class DataHelper
    {

        private static DataHelper instance = new DataHelper();

        private DataHelper()
        {

        }

        public static DataHelper Instance
        {
            get
            {
                return instance;
            }
        }


        //唯一连接对象
        MySqlConnection sqlConn;

        public void Connect()
        {
            string DbAndDS = "Database=gamedata;DataSource=127.0.0.1;";
            string IdAndPw = "User Id=root;Password=570128;port=3306";
            string connStr = DbAndDS + IdAndPw;

            sqlConn = new MySqlConnection(connStr);
            try
            {
                sqlConn.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("DataBaseConnect" + e.Message);
            }
        }



        //判断字符串是否安全，防止sql注入
        //非法返回false,安全返回true
        public bool IsSafeStr(string str)
        {
            //加@强制不转义,在里面的转义字符无效
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\{|\}|%|@|\*|!|\']");
        }


        //是否可以注册，可以注册返回true,不可以注册返回false
        public bool IsRegister(string id)
        {

            //防sql注入,包含不安全代码不可注册
            if (!IsSafeStr(id))
                return false;


            //接下来查询id是否已经存在
            string cmdStr = string.Format("select * from user where id='{0}'", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                //如果已经存在，则该id不能使用
                return !hasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("ISRegister:" + e.Message);
                return false;
            }

        }



        public bool Register(string id, string password)
        {
            //如果不可以注册
            if (!IsRegister(id))
                return false;

            //如果包含非法字符
            if (!IsSafeStr(password))
                return false;

            string cmdStr = string.Format("insert into user values('{0}','{1}');", id, password);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

            try
            {
                //返回受影响的行数
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Register" + e.Message);
                return false;
            }
        }



        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CreatePlayer(string id)
        {
            PlayerData playerData = new PlayerData();

            IFormatter formatter = new BinaryFormatter();

            MemoryStream stream = new MemoryStream();

            try
            {
                //序列化到内存流
                formatter.Serialize(stream, playerData);
            }
            catch (Exception e)
            {
                Console.WriteLine("CreatePlayer:" + e.Message);
                return false;
            }

            //转为字节数组
            byte[] bytes = stream.ToArray();

            //写入数据库
            string cmdStr = string.Format("insert into player values('{0}',@data)", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

            //添加一个参数并设置参数类型
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            //添加该参数的值
            //cmd.Parameters["@data"].Value = bytes;
            cmd.Parameters[0].Value = bytes;

            //上面两句可以合为
            //cmd.Parameters.AddWithValue("@data", bytes);


            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("CreatePlayerMySql:" + e.Message);
                return false;
            }
        }



        /// <summary>
        /// 登陆校验
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool LoginCheck(string id,string password)
        {
            //id不安全返回false
            if (!IsSafeStr(id))
                return false;

            string cmdStr = string.Format("select * from user where id='{0}'and password={1}", id, password);
            //或者
            // string cmdStr="select * from user where id=@id',pw=@password";

            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                //数据流是否有数据
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return hasRows;

            }
            catch(Exception e)
            {
                Console.WriteLine("LoginCheck:"+e.Message);
                return false;
            }

        }



        /// <summary>
        /// 获取玩家数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PlayerData GetPlayerData(string id)
        {
            PlayerData playerData = null;

            if (!IsSafeStr(id))
                return playerData;

            string cmdStr = string.Format("select * from player where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr,sqlConn);
            byte[] buffer = null;
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                //如果不包含数据
                if (!dataReader.HasRows)
                {
                    dataReader.Close();
                    return playerData;
                }

                //读取第一行数据
                dataReader.Read();
                //获取要读取的数据长度
                long length = dataReader.GetBytes(1, 0, null, 0, 0);
                buffer = new byte[length];
                dataReader.GetBytes(1, 0, buffer, 0, (int)length);
                dataReader.Close();

                    
            }
            catch(Exception e)
            {
                Console.WriteLine("GetPlayerData:"+e.Message);
                return playerData;
            }



            //反序列化
            MemoryStream stream = new MemoryStream(buffer);

            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                playerData = (PlayerData)binaryFormatter.Deserialize(stream);
                return playerData;
            }
            catch(Exception e)
            {
                Console.WriteLine("GetPlayDataDesFormatter:"+e.Message);
                return playerData;
            }
            
            
        }



        /// <summary>
        /// 保存角色
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool SavePlayer(Player player)
        {
            string id = player.id;
            PlayerData data = player.data;

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();

            //序列化
            try
            {
                binaryFormatter.Serialize(stream, data);
            }
            catch(Exception e)
            {
                Console.WriteLine("SavePlayer:"+e.Message);
                return false;
            }

            byte[] bytes = stream.ToArray();

            string cmdStr = string.Format("update player set data=@data where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters["@data"].Value = bytes;
            try
            {
                cmd.ExecuteNonQuery();
                return true;

            }
            catch(Exception e)
            {
                Console.WriteLine("SavePlayerData:"+e.Message);
                return false;
            }

        }





    }
}
