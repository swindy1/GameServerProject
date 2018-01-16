using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    class ServTime
    {
        public static long GetTimeStamp()
        {
            //获取时间间隔,按协调通用时间
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0,0,0,0);
            return Convert.ToInt64(timeSpan.TotalSeconds);
        }
    }
}
