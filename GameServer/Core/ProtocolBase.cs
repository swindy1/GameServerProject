using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    class ProtocolBase
    {
        //解码器，从readBuffer的start处开始解码length长度字节
        public virtual ProtocolBase Decode(byte[] readBuffer,int start,int length)
        {
            return new ProtocolBase();
        }

        //编码器，将数据编码为byte数组
        public virtual byte[] Encode()
        {
            return new byte[] { };
        }

        //获取协议名称
        public virtual string GetName()
        {
            return "";
        }

        //获取协议内容，用于调试
        public virtual string GetDesc()
        {
            return "";
        }
    }
}
