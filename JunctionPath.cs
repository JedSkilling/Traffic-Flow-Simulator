using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_Flow_Simulator
{
    partial class TrafficSim
    {
        class JunctionPath : Road
        {   //  Junction path is also only one way - Should we add a base carRoute class the Road and JunctionPath inherit from?

            //  Stores a specific route through a junction, and the logic needed to take it
            //  Will contain a list of other JunctionPaths that need to be free for this to be usable

            //  How to store start and end road?
            Road startRoad;
            Road endRoad;

            public JunctionPath(WorldPathFollowed routeOfRoad, Road startRoad, Road endRoad, float? length, List<Vehicle> vehiclesOnRoad = null) : base(routeOfRoad, length, vehiclesOnRoad)
            {
                this.startRoad = startRoad;
                this.endRoad = endRoad;
            }

            public void ProcessVehicles()
            {
                //  COPIED FROM ROAD PROCESSVEHICLES

                

                foreach (Vehicle car in vehiclesOnThisRoad)
                {
                    float newPos = car.CalculateCurrentPositionAlongRoad();
                    if (newPos > length)                             //  DEBUG/ERROR HANDLING
                    {
                        Console.WriteLine("New pos has been behind the road");
                        //  Had problems with over deceleration causing speed to go briefly negative, fixed by preventing going from pos to neg speed in one tick
                        //  Also could fix by being more careful in max deceleration
                        throw new Exception();
                    }

                    if (newPos < 0)
                    {
                        //  Car will always be headed to next road
                        float posOffset = -newPos;
                        endRoad.AddCar(car, posOffset);
                        this.RemoveCar(car);

                        //  Now we need to put the car on the new road




                    }

                }
                UpdateVehiclesOnThisRoad();
                // END OF COPIED FROM ROAD PROCESSVEHICLES
            }


            public bool checkRoads(Road start, Road end)
            {
                if (this.startRoad == start)
                {
                    if (this.endRoad == end)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
