using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enmity.Terrain
{
    internal class TerrainConfig
    {
        public const float TickRate = 10f; // 10 ticks per second
        public const float ChunkTickPeriod = 1f / TickRate;
    }
}
