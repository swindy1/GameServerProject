using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    /// <summary>
    /// 字节流协议
    /// </summary>
    class ProtocolBytes:ProtocolBase
    {
        //传输的字节流
        public Byte[] bytes;


        //解码器；返回ProtocolBase
        public override ProtocolBase Decode(byte[] readBuffer, int start, int length)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.bytes = new byte[length];
            //复制
            Array.Copy(readBuffer,start,bytes,0,length);
            return protocol;
        }

        //编码，返回byets
        public override byte[] Encode()
        {
            return bytes;
        }

        //协议名称
        public override string GetName()
        {
            return GetString(0);
        }

        //获取描述
        public override string GetDesc()
        {
            StringBuilder str = new StringBuilder("");
            if (bytes == null)
                return str.ToString();
            for(int i=0;i<bytes.Length; i++)
            {
                //byte数组中每一位的值为1或0，转为int类型显示
                int byt= (int)bytes[i];
                //添加到末尾
                str.Append(byt.ToString());
            }
            return str.ToString();
        }

        //将字符串添加到bytes中
        public void AddString(string str)
        {
            int len = str.Length;
            //4字节长度存储str长度
            byte[] lenBytes = BitConverter.GetBytes(len);
            //str内容
            byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);

            //第一次，初始化
            if (bytes == null)
                bytes = lenBytes.Concat(strBytes).ToArray();
            //非初始化,拼接到原数组末尾
            else
                bytes = bytes.Concat(lenBytes).Concat(strBytes).ToArray();


        }

        //从bytes中获取字符串
        public string GetString(int start,ref int end)
        {
            if (bytes == null)
                return "";
            if (bytes.Length < start+sizeof(Int32))
                return "";

            //从start开始获取4个字节
            int len = BitConverter.ToInt32(bytes,start);
            //长度校验
            if (bytes.Length < start + sizeof(Int32) + len)
                return "";

            string str = System.Text.Encoding.UTF8.GetString(bytes, start + sizeof(Int32), len);
            end = start + sizeof(Int32) + len;
            return str;
        }


        //重载版GetString
        public string GetString(int start)
        {
            int end = 0;
            return GetString(start, ref end);
        }


        public void AddInt(int num)
        {
            //返回4个字节的字节数组
            byte[] numBytes = BitConverter.GetBytes(num);

            if (bytes == null)
                bytes = numBytes;
            else
                bytes = bytes.Concat(numBytes).ToArray();

        }


        //获取Int数据
        public int GetInt(int start,ref int end)
        {
            if (bytes == null)
                return 0;
            if (bytes.Length < start + sizeof(Int32))
                return 0;
            //读取4个字节
            int num = BitConverter.ToInt32(bytes, start);
            end = start + sizeof(Int32);

            return num;
        }

        //重载版GetInt
        public int GetInt(int start)
        {
            int end = 0;
            return GetInt(start,ref end);
        }


        //添加float
        public void AddFloat(float num)
        {
            //读取float到8位字节数组
            byte[] numBytes = BitConverter.GetBytes(num);

            if (bytes == null)
                bytes = numBytes;
            else
                bytes = bytes.Concat(numBytes).ToArray();
        }


        //获取Float
        public float GetFloat(int start,ref int end)
        {
            if (bytes == null)
                return 0f;
            if (bytes.Length < start + sizeof(float))
                return 0f;
            //读取浮点数
            float num = BitConverter.ToSingle(bytes, start);
            end = start + sizeof(float);
            return num;
        }


        //重载GetFloat
        public float GetFloat(int start)
        {
            int end = 0;
            return GetFloat(start, ref end);
        }
    }
}
