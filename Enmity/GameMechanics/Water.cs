using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enmity.GameMechanics
{
    internal class Water : Fluid
    {
        public override void Initialize()
        {
            Viscosity = 0.5f;
        }

        public override void Update()
        {

        }

        public override void Draw()
        {

        }
    }
}
