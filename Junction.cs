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
        enum JunctionType   //  Will contain a number for each type of junction
        {
            straightRoad,
            threeWayJunctionNoPriority,
            Debug

        }
        class Junction
        {

            //  Stores the location and paths through a given junction
            //  Will return which junction paths are free (and how busy others are?)
            //  Will store what the signal we'd need to see to know a car was planning on going this way
            Vector2f worldPos;
            List<Road> connectedRoads;
            List<JunctionPath> allJunctionPaths = new List<JunctionPath>();

            Shape juncShape = new CircleShape(25f)              //  Don't need to regenerate the sprite should move this to the Junction class
            {
                Position = new Vector2f(0, 0),
                Origin = new Vector2f(25, 25),
                FillColor = Color.Transparent,
                OutlineColor = junctionColour,
                OutlineThickness = 1,
            };

            public Junction(Vector2f worldPos, List<Road> connectedRoads, JunctionType type)
            {
                this.worldPos = worldPos;
                this.connectedRoads = connectedRoads;
                foreach (Road connectedRoad in this.connectedRoads)
                {
                    connectedRoad.AddNewJunction(this);
                }
                CreateJunctionPaths(type);
            }

            public void ProcessVehicles()
            {
                foreach (JunctionPath currPath in allJunctionPaths)
                {
                    currPath.ProcessVehicles();
                }
            }

            public void CreateJunctionPaths(JunctionType type)
            {
                if (type == JunctionType.straightRoad)
                {
                    WorldPathFollowed junctionWorldPath = new WorldPathFollowed(this.worldPos, this.worldPos);
                    /*JunctionPath pathOne = new JunctionPath(junctionWorldPath, connectedRoads[0], connectedRoads[1], length: 2);
                    allJunctionPaths.Add(pathOne);
                    JunctionPath pathTwo = new JunctionPath( junctionWorldPath, connectedRoads[1], connectedRoads[0], length: 2);
                    allJunctionPaths.Add(pathTwo);*/

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (i != j)
                            {
                                allJunctionPaths.Add(
                                    new JunctionPath(
                                    junctionWorldPath,
                                    connectedRoads[i],
                                    connectedRoads[j],
                                    length: 2
                                    ));
                            }
                        }
                    }
                }
                else if (type == JunctionType.threeWayJunctionNoPriority)
                {
                    WorldPathFollowed junctionWorldPath = new WorldPathFollowed(this.worldPos, this.worldPos);
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (i != j)
                            {
                                allJunctionPaths.Add(
                                    new JunctionPath(
                                    junctionWorldPath,
                                    connectedRoads[i],
                                    connectedRoads[j],
                                    length: 2
                                    ));
                            }
                        }
                    }
                }
                else if (type == JunctionType.Debug)
                {

                }
                else
                {
                    Console.WriteLine("Junction type not supported or invalid type");
                    throw new Exception();
                }
            }

            public void AddCar(Vehicle carToAdd, float positionOffset, Road sourceRoad)
            {
                //  Take in a car and put it on the correct junction path
                if (!connectedRoads.Contains(sourceRoad))
                {
                    Console.WriteLine("Junction did not contain the road a car came from");
                    throw new Exception();
                }
                foreach (JunctionPath potentialPath in allJunctionPaths)
                {
                    //  Will need to move this at some point
                    if (potentialPath.checkRoads(sourceRoad, carToAdd.GetNextRoad()))    //  Would the car not have already chosen this before entering the junction?
                    {
                        //  code runs when junction path matches intended route
                        carToAdd.EnteringJunction(this);    //  Car has now entered a new junction
                        SwitchCarToNewRoad(carToAdd, sourceRoad, potentialPath, positionOffset);


                    }
                }


            }

            public List<Vehicle> GetVehiclesInJunction()
            {
                List<Vehicle> vehicles = new List<Vehicle>();
                foreach(JunctionPath currPath in allJunctionPaths)
                {
                    vehicles.AddRange(currPath.GetVehicles());
                }
                return vehicles;
            }
            public Vector2f GetWorldPos()
            {
                return this.worldPos;
            }

            public List<Road> GetConnectedRoads()
            {
                return connectedRoads;
            }

            public Drawable GetDrawable()
            {
                this.juncShape.Position = this.worldPos;
                return juncShape;
            }
        }
    }
}
