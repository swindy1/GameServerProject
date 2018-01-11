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

namespace GameServer.Core
{
    //单例模式
    class DataHelper
    {

        private static DataHelper instance=new DataHelper();

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
            catch(Exception e)
            {
                Console.WriteLine("DataBaseConnect"+e.Message);
            }
        }



        //判断字符串是否安全，防止sql注入
        //非法返回false,安全返回true
        public bool IsSafeStr(string str)
        {
            //加@强制不转义,在里面的转义字符无效
            return !Regex.IsMatch(str,@"[-|;|,|\/|\(|\)|\[|\]|\{|\}|%|@|\*|!|\']");
        }


        //是否可以注册，可以注册返回true,不可以注册返回false
        public bool IsRegister(string id)
        {

            //防sql注入,包含不安全代码不可注册
            if (!IsSafeStr(id))
                return false;


            //接下来查询id是否已经存在
            string cmdStr = string.Format("select * from user where id='{0}'",id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                //如果已经存在，则该id不能使用
                return !hasRows;
            }
            catch(Exception e)
            {
                Console.WriteLine("ISRegister:"+e.Message);
                return false;
            }

        }



        public bool Register(string id,string password)
        {
            //如果不可以注册
            if (!IsRegister(id))
                return false;

            //如果包含非法字符
            if (!IsSafeStr(password))
                return false;

            string cmdStr = string.Format("insert into user values(id='{0}',pw='{1}');", id, password);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);

            try
            { 
                //返回受影响的行数
                cmd.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("Register"+e.Message);
                return false;
            }
        }



    }
}
