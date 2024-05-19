using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_Flow_Simulator
{
    partial class TrafficSim
    {
        class Vehicle
        {
            //  Stores information about a vehicle eg speed, memory
            //  Will contain where the vehicle is going to AND how
            //  Will contain logic on working out appropriate speed
            //  May contain logic for checking when a junction is free?

            //  Fixed values


            float maxAcceleration = 3;      //  m/s
            float maxDeceleration = 8;      //  m/s

            float vehicleLength = 3.5f;     //   metres
            float vehicleWidth = 1.5f;
            Vector2f textureScale;

            List<Road> route;

            protected Sprite carSprite;




            //  Frequent changing
            MemoryForVehicle memory;

            float currentPosRelativeToRoad;
            float currentSpeed;
            Junction previousJunction;
            int distThroughRoute = 0;       //  Need to update upon changing road
            Road currentRoad;

            public Vehicle(List<Road> route, Junction previousJunction, float currentSpeed = 0) //  First route entry is the starting route of the car
            {
                this.route = route;
                this.currentSpeed = currentSpeed;
                this.previousJunction = previousJunction;

                this.memory = new MemoryForVehicle();
                this.textureScale = CalculateScale();
                this.carSprite = GenerateSprite();
            }

            public Sprite GenerateSprite()
            {
                return new Sprite()
                {
                    Texture = carTexture,
                    Scale = this.GetTextureScale(),
                    Origin = this.GetScreenCarSize() / 2,

                };
            }

            Vector2f CalculateScale()
            {
                float Xscale = this.vehicleLength * 1f / (carTexture.Size.X * metresPerPixel);  //  Scale down to correct number of pixels
                float Yscale = this.vehicleWidth * 1f / (carTexture.Size.Y * metresPerPixel);
                return new Vector2f(Xscale, Yscale);
            }

            public float CalculateCurrentPositionAlongRoad()
            {
                float deltaPos = GetCurrentSpeed() / fps;
                float newPos = currentPosRelativeToRoad - deltaPos; //  Moving forward is subtracting from our pos relative to road
                this.currentPosRelativeToRoad = newPos;
                return newPos;
            }
            float GetCurrentSpeed()
            {
                return currentSpeed + GetCurrentAcceleration();
            }

            float GetCurrentAcceleration()
            {
                float targetSpeed = GetTargetSpeed();
                if (currentSpeed < 0.9 * targetSpeed)
                {
                    return maxAcceleration;
                }
                else if (currentSpeed > targetSpeed)
                {
                    return maxDeceleration;
                }
                return 0;

            }

            float GetTargetSpeed()
            {
                return memory.GetSpeedlimit();
            }


            public Junction GetPreviousJunction()
            {
                if (previousJunction != null)
                {
                    return previousJunction;
                }
                else
                {
                    Console.WriteLine("Previous Junction requested without it being set yet");
                    throw new Exception();
                }

            }

            public float GetCurrentPosRelRoad()
            {
                return this.currentPosRelativeToRoad;
            }

            public Road GetNextRoad()
            {
                Road nextRoad = route[distThroughRoute];
                return nextRoad;
            }

            public void SwitchToNewRoad(Road newRoad, float offset)
            {
                if (newRoad.GetType() == typeof(JunctionPath))
                {
                    Console.WriteLine("Passing through junction, therefore no change in dist through route");
                }
                else
                {
                    distThroughRoute++;

                    Console.WriteLine("Entering new road, so change in dist through route");
                }
                this.currentPosRelativeToRoad = newRoad.length - offset;
                this.currentRoad = newRoad;
            }

            public List<Vehicle> FindNearbyCars(float searchDistBehind, float searchDistAhead)
            {
                //  Limit of search will be max of distBehind and Ahead, then we just add an offset on the lower one so it starts closer to the limit+


                //  TO DO:
                //  Make structs for visibleJunctions (Contains dist from origin and a junction)
                //  Make struct (or class) for road segment with offset

                //  END

                //  Handle starting case as it more complicated
                //  Find which junction is ahead and which is behind

                //  Add both to seen junctions along with a current distance (If current dist is within our limit)

                //  Check if we fail to reach both junctions if so add the relevant road segment to a list of roadSegments

                //  Start main loop
                //  Loop through all visible junctions and find the nearest one to origin
                //  Don't need to check if beyond max dist as we will only add if within limit

                //  The one we find will be the junction we visit next
                //  When we visit it check through all neighbour junctions (by looping through roads)
                    //  If neighbour junctions are already visited then search through visibileRoadSegments to find the road you used to reach that
                    //  When you find the road: (If not throw error)
                        //  Check if road is fully covered ie is there overlap (If so add to fullyVisibileRoads and remove from roadSegments)
                        //  If not fully covered, add our current roadSegment to the visibleRoadSegments



                throw new NotImplementedException();
            }

            public void EnteringJunction(Junction newJunction)
            {
                this.previousJunction = newJunction;
            }

            public Vector2f GetTextureScale()
            {
                return this.textureScale;
            }

            public Vector2f GetScreenCarSize()
            {
                Vector2f size = new Vector2f(vehicleLength / metresPerPixel, vehicleWidth / metresPerPixel);
                return size;
            }

            public Sprite GetSprite()
            {
                return this.carSprite;
            }

        }
    }
}
