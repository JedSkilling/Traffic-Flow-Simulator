using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_Flow_Simulator
{
    partial class TrafficSim
    {
        class MemoryForVehicle
        {
            //  Where the actual memory is stored
            float speedlimit;

            public MemoryForVehicle()
            {
                speedlimit = 5f; //   default is 80mph?
            }


            public float GetSpeedlimit()
            {
                return speedlimit;
            }
        }
    }
}
