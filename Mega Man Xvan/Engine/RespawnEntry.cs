using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
    internal class RespawnEntry
    {
        private Player player; // Bomberman que será respawnado
        private MMXFloat time; // Tempo que deverá se esperar para que o respawn ocorra

        /// <summary>
        /// Cria uma nova entrada de respawn
        /// </summary>
        /// <param name="bomberman">Bomberman que será respawnado</param>
        /// <param name="time">Tempo que deverá se esperar para que o respawn ocorra</param>
        public RespawnEntry(Player player, MMXFloat time)
        {
            this.player = player;
            this.time = time;
        }

        /// <summary>
        /// Bomberman que será respawnado
        /// </summary>
        public Player Player
        {
            get
            {
                return player;
            }
        }

        /// <summary>
        /// Tempo que deverá se esperar para que o respawn ocorra
        /// </summary>
        public MMXFloat Time
        {
            get
            {
                return time;
            }
        }
    }
}
