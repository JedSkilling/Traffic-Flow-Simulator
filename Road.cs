using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Traffic_Flow_Simulator.TrafficSim;

namespace Traffic_Flow_Simulator
{
    partial class TrafficSim
    {
        class Road
        {
            //  Stores the cars and info they need when moving along the road
            public float length;
            protected WorldPathFollowed routeOfRoad;
            protected Junction? junctionStart = null;
            protected Junction? junctionEnd = null;
            protected List<Vehicle> vehiclesOnThisRoad = new List<Vehicle>();

            protected List<Vehicle> vehiclesToRemove = new List<Vehicle>();
            protected List<Vehicle> vehiclesToAdd = new List<Vehicle>();



            public Road(WorldPathFollowed routeOfRoad, float? length = null, List<Vehicle> vehiclesOnRoad = null)
            {
                if (vehiclesOnRoad != null)
                {
                    foreach (Vehicle newCar in vehiclesOnRoad)
                    {
                        this.AddCar(newCar, 0f);
                    }
                }
                else
                {
                    vehiclesOnRoad = new List<Vehicle>();
                }

                this.routeOfRoad = routeOfRoad;



                if (length == null)
                {
                    this.length = metresPerPixel * routeOfRoad.GetWorldSpaceLength();
                }
                else
                {
                    this.length = (float)length;
                }

            }

            public void AddNewJunction(Junction newJunction)    //  May need to add explicity definition of start or end
            {
                //  Need to know if this is the start or end junction
                //  Currently achieved by picking which one is closest
                if (junctionStart != null && junctionEnd != null)
                {
                    Console.WriteLine("Trying to add new junction to full road");
                    throw new Exception();
                }



                Vector2f juncToRoadStart = newJunction.GetWorldPos() - this.routeOfRoad.GetStartPos();
                float distToStart = Magnitude(juncToRoadStart);
                Vector2f juncToRoadEnd = newJunction.GetWorldPos() - this.routeOfRoad.GetEndPos();
                float distToEnd = Magnitude(juncToRoadEnd);

                if (distToStart < distToEnd)
                {
                    if (junctionStart == null)
                    {
                        junctionStart = newJunction;
                    }
                    else
                    {
                        Console.WriteLine("Junction start is already claimed, but we tried to put another junction there");
                        throw new Exception();
                    }
                }
                else if (distToStart >= distToEnd)
                {
                    if (junctionEnd == null)
                    {
                        junctionEnd = newJunction;
                    }
                    else
                    {
                        Console.WriteLine("Junction end is already claimed, but we tried to put another junction there");
                        throw new Exception();
                    }
                }

            }
            public void ProcessVehicles()
            {
                //  To avoid changing vehicles on this road we only remove vehicles at the end

                foreach (Vehicle car in vehiclesOnThisRoad)
                {
                    float newPos = car.CalculateCurrentPositionAlongRoad();
                    if (newPos > length)                             //  DEBUG/ERROR HANDLING
                    {
                        Console.WriteLine("New pos has been behind the road");
                        throw new Exception();
                    }

                    if (newPos < 0)
                    {
                        //  Need to get where the car is headed
                        Junction targetJunction;
                        if (car.GetPreviousJunction() == junctionStart)
                        {
                            targetJunction = junctionEnd;
                        }
                        else if (car.GetPreviousJunction() == junctionEnd)
                        {
                            targetJunction = junctionStart;
                        }
                        else
                        {
                            Console.WriteLine("Cars previous junction was not connected to the road it is currently on");
                            throw new Exception();
                        }

                        //  Now we need to put the car on the new junction
                        float posOffset = -newPos;
                        targetJunction.AddCar(car, posOffset, this);



                    }

                }
                UpdateVehiclesOnThisRoad();

            }

            public List<Vehicle> FindNearCars(float searchStart, Junction startReference, float searchDistBehind, float searchDistAhead)
            {






            }
            protected void UpdateVehiclesOnThisRoad()
            {
                foreach (Vehicle toRemove in vehiclesToRemove)
                {
                    this.vehiclesOnThisRoad.Remove(toRemove);
                }
                vehiclesToRemove.Clear();
                foreach (Vehicle toAdd in vehiclesToAdd)
                {
                    this.vehiclesOnThisRoad.Add(toAdd);
                }
                vehiclesToAdd.Clear();

            }

            public void RemoveCar(Vehicle car)
            {
                vehiclesToRemove.Add(car);
            }

            public void AddCar(Vehicle car, float offset)   //  offset is with reference to start of road
            {
                car.SwitchToNewRoad(this, offset);

                if (vehiclesOnThisRoad.Contains(car) || vehiclesToAdd.Contains(car))   //  debug/error detection
                {
                    Console.WriteLine("Car is already on road it tried to join");
                    throw new Exception();
                }

                vehiclesToAdd.Add(car);
            }

            public List<Drawable> GetRouteRenderList()
            {
                return routeOfRoad.GetListToRender();
            }

            public List<Drawable> CreateCarDrawables()
            {
                List<Drawable> carDrawables = new List<Drawable>();
                foreach (Vehicle car in this.vehiclesOnThisRoad)
                {
                    Sprite carSprite = car.GetSprite();



                    float lengthRelStart;
                    bool headedToEnd;
                    if (car.GetPreviousJunction() == junctionStart)
                    {
                        lengthRelStart = this.length - car.GetCurrentPosRelRoad();
                        headedToEnd = true;
                    }
                    else if (car.GetPreviousJunction() == junctionEnd)
                    {
                        lengthRelStart = car.GetCurrentPosRelRoad();
                        headedToEnd = false;
                    }
                    else
                    {
                        Console.WriteLine("Car's previous junction not on road (whilst trying to draw)");
                        throw new Exception();
                    }

                    float[] newPosAndRot = routeOfRoad.GetCoordAndRotOfCar(lengthRelStart, headedToEnd);   //  Won't work if I move to bezier curves for roads
                    carSprite.Position = new Vector2f(newPosAndRot[0], newPosAndRot[1]);
                    carSprite.Rotation = newPosAndRot[2];
                    carDrawables.Add(carSprite);


                }
                return carDrawables;
            }

        }
    }
}
