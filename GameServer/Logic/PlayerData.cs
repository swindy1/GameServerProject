using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Logic
{

    [Serializable]
    class PlayerData
    {
        public int score = 0;

        public PlayerData()
        {
            score = 100;
        }
    }
}
