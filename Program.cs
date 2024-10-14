using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Traffic_Flow_Simulator
{
    partial class TrafficSim
    {
        private struct JunctionWithDij  //  Used in FindNearbyCars function
        {
            public Junction baseJunction { get; set; }
            public float distFromOrigin { get; set; }
            public float dijDist { get; set; }
        }

        private struct RoadSegment
        {
            public Road baseRoad { get; set; }
            public Junction referenceJunction { get; set; }
            public float offsetFromRefJunc { get; set; }
            public float lengthOfSelection { get; set; }
        }
        //  Debug settings
        static bool displayAllCarInfo = true;
        //  End of debug settings

        static RenderWindow window;
        static Font font = new Font("..\\..\\..\\fonts\\arial.ttf");

        static float fps = 60;
        

        //  Creating colours
        static Color backgroundColour = new Color(30, 57, 73);
        static Color junctionColour = new Color(0, 255, 98);
        static Color roadColour = new Color(0, 255, 215);

        //static Color carColour = new Color(187, 0, 255);    //  Created from texture so this isn't used

        //static Texture junctionTexture = new Texture("..\\..\\..\\textures\\junctionTexture - Not transparent.png");

        static Texture carTexture = new Texture("..\\..\\..\\textures\\carArt-Draft02.png");

        //  World to Real conversion ratios
        static float metresPerPixel = 0.1f;
        static void Main(string[] args)
        {
            

            //  Creates window
            window = new RenderWindow(new VideoMode(1000, 1000), "Traffic Flow Simulator");
            window.SetFramerateLimit(60);

            //  All roads:
            List<Road> roads = new List<Road>();
            List<Junction> junctions = new List<Junction>();

            

            //  Rendering variables
            List<Drawable> junctionDrawables = new List<Drawable>();  //  Junction sprites will be drawn first
            List<Drawable> roadDrawables = new List<Drawable>();
            List<Drawable> carDrawables = new List<Drawable>();

            
            


            //  Initialising for simple case (two junctions connected by two roads)
            #region Simple test case 01
            

            roads.Add(
                new Road(
                    new WorldPathFollowed(
                        new Vector2f(200, 200),
                        new Vector2f(800, 400),
                        new List<Vector2f> { new Vector2f(700, 250)}
                        )
                    )
                );

            roads.Add(
                new Road(
                    new WorldPathFollowed(
                        new Vector2f(800, 400),
                        new Vector2f(200, 200),
                        new List<Vector2f> { new Vector2f(200, 700) }
                        )
                    )
                );

            roads.Add(
                new Road(
                    new WorldPathFollowed(
                        new Vector2f(800, 400),
                        new Vector2f(200, 200),
                        new List<Vector2f> { new Vector2f(300, 500) }
                        )
                    )
                );

            junctions.Add(
                new Junction(new Vector2f(200, 200), new List<Road> { roads[0], roads[1], roads[2] }, JunctionType.threeWayJunctionNoPriority)
                );

            junctions.Add(
                new Junction(new Vector2f(800, 400), new List<Road> { roads[0], roads[1], roads[2] }, JunctionType.threeWayJunctionNoPriority)
                );

            roads[0].AddCar(
                new Vehicle(
                    route: new List<Road> { roads[0], roads[1], roads[0], roads[2] },
                    previousJunction: junctions[1]
                    ),
                0
                );
            roads[1].AddCar(
                new Vehicle(
                    route: new List<Road> { roads[1], roads[2], roads[0], roads[1] },
                    previousJunction: junctions[1]
                    ),
                5
                );

            #endregion
            //  End of ititialising

            //  On event triggers
            window.Closed += (_, __) => Quit();
            window.KeyPressed += (S, E) => OnKey(E, true);
            window.Resized += (S, E) => OnResize(E);        //  Only updates view upon finishing changes

            //  Starts displaying window
            window.SetActive();

            int tick = 1;
            while (window.IsOpen)
            {
                //  Code to be ran each tick goes here
                //  Process cars on road
                foreach (Road currRoad in roads)
                {
                    currRoad.ProcessVehicles();
                }
                foreach(Junction currJunction in junctions)
                {
                    currJunction.ProcessVehicles();
                }
                
                //  Drawing stuff

                //  Debug/Temp
                
                

                //  End of debug

                foreach (Junction junctionToDraw in junctions)
                {
                    //  Fills in the junctionSprites list to be drawn
                    //  Cars are NOT drawn whilst in junctions currently - should change this

                    //  Each junction is currently a ring of set size
                    
                    /*Sprite juncSprite = new Sprite(junctionTexture)   Useful for when/if I want to switch to textures, may be required for cars?
                    {
                        Position = junctionToDraw.GetWorldPos(),
                    };*/
                    junctionDrawables.Add(junctionToDraw.GetDrawable());

                }

                foreach(Road currRoad in roads)
                {
                    //  Draw the main road as a base


                    roadDrawables.AddRange(currRoad.GetRouteRenderList());
                    //  Loop through cars present and draw these
                    
                    carDrawables.AddRange(currRoad.CreateCarDrawables(displayAllCarInfo));
                    
                    
                }

                //  Window controller stuff
                window.DispatchEvents();
                window.Clear(backgroundColour);

                //  Actual rendering
                foreach (Drawable currSprite in junctionDrawables)
                {
                    window.Draw(currSprite);
                }
                
                foreach (Drawable currSprite in roadDrawables)
                {
                    window.Draw(currSprite);
                }
                foreach(Drawable currSprite in carDrawables)
                {
                    window.Draw(currSprite);
                }
                //  Debug

                //  End of debug

                window.Display();
                tick++;
                //  End of tick
                junctionDrawables = new List<Drawable>();
                roadDrawables = new List<Drawable>();
                carDrawables = new List<Drawable>();

            }
        }

        static List<Vehicle> FindNearbyCars(float distFromReferenceJunction, Junction referenceJunction, Road startRoad, float searchDistToRefJunc/*Behind car*/, float searchDistAwayFromRefJunc/*In front of car*/, Vehicle? startVehicle = null)  //  Counts junctions as having zero distance
        {
            //  Principles this tries to maintain:
            //      Avoid sudden jumps in visible cars when going through junctions
            //      Cars won't be as aware of cars behind them compared to infront
            //      Time complexity should not depend on total number of cars
            //  Not currently accounted for:
            //      Any thing in the road affecting visibility eg corners
            //  searchDistToRefJunc will be BEHIND a car (if refJunction is got from previousJunction)


            //  Limit of search will be max of distBehind and Ahead, then we just add an offset on the lower one so it starts closer to the limit


            //  TO DO:
            //  Make structs for visibleJunctions (Contains dist from origin and a junction)    -   DONE
            //  Make struct -class?- for road segment (with offset)                             -   DONE
            //  Implement main code
            //  Add a GetCarsFromDistAndOffset function to Road


            //  END
            if(distFromReferenceJunction > startRoad.length)
            {
                Console.WriteLine("Search offset greater than length of road we started on");
                throw new Exception();
            }
            else if(distFromReferenceJunction < 0)
            {
                Console.WriteLine("Search offset below zero");
                throw new Exception();
            }
            List<JunctionWithDij> visibleJunctions = new List<JunctionWithDij>();
            List<RoadSegment> roadSegments = new List<RoadSegment>();
            List<Vehicle> vehicles = new List<Vehicle>(); //    Will primarily be generated from the roadSegment list, but JunctionPaths add directly to this as we don't count their distances

            //List<Road> fullyVisibleRoads = new List<Road>();  -   Rolled into roadSegments now


            //  Handle starting case as it more complicated
            //  Find which junction is ahead and which is behind
            Junction[] connectedJunctions = startRoad.GetStartAndEndJunction();
            Junction otherJunction = null;
            //  Other junction is ahead of our car, reference junction is behind
            if(connectedJunctions[0] == referenceJunction)
            {
                otherJunction = connectedJunctions[1];
            }
            else if(connectedJunctions[1] == referenceJunction)
            {
                otherJunction = connectedJunctions[0];
            }
            else
            {
                Console.WriteLine("FindNearbyCars reference junction was not connected to startRoad");
                throw new Exception();
            }
            float currentRoadLength = startRoad.length;

            //  Calculate actual distance : FSS means From Search Start
            float distToRefJuncFSS = -distFromReferenceJunction;    //  Negative as this is the distance behind
            float distToOtherJuncFSS = currentRoadLength - distFromReferenceJunction;

            //  Calculate the "distance" that we use to allow for a different search dist behind and ahead (By offsetting the start of the search to the lower one)
            //  We want a difference search distance for looking ahead and behind to model drivers paying more attention ahead of them
            //  We want to put them in the same (adjusted) application of Dijkstra because tracking overlap between them is easier
            //  a dij (dijkstra) prefix means that this is the adjusted "distance" rather than the actual distance
            //  This does mean we need to store the actual distance as well, but we just use the dijDistances as our heuristic

            float maxSearchDist = MathF.Max(-distToRefJuncFSS, distToOtherJuncFSS);


            //  Calculate our offsets
            float dijDistToRefJunc = MathF.Max(searchDistToRefJunc, searchDistAwayFromRefJunc) - searchDistToRefJunc; 
            float dijDistToOtherJunc = MathF.Max(searchDistToRefJunc, searchDistAwayFromRefJunc) - searchDistAwayFromRefJunc;
            //  Add in the dists we need to travel
            dijDistToRefJunc += -distToRefJuncFSS;
            dijDistToOtherJunc += distToOtherJuncFSS;

            //  Add both to seen junctions along with a current distance (If current dist is within our limit, distance will be adjusted for searchDistBehind and Ahead)



            bool failedToReachRefJunc = false;
            bool failedToReachOtherJunc = false;
            if (dijDistToRefJunc < maxSearchDist)
            {
                visibleJunctions.Add(new JunctionWithDij
                {
                    baseJunction = referenceJunction,
                    distFromOrigin = distToRefJuncFSS,
                    dijDist = dijDistToRefJunc 
                });
            }
            else
            {
                //  Generate correct roadSegment
                failedToReachRefJunc = true;
            }
            if(dijDistToOtherJunc < maxSearchDist)
            {
                visibleJunctions.Add(new JunctionWithDij
                {
                    baseJunction = otherJunction,
                    distFromOrigin = distToOtherJuncFSS,
                    dijDist = dijDistToOtherJunc
                });
            }
            else
            {
                //  Generate correct roadSegment
                failedToReachOtherJunc = true;
            }
            //  If we fail to reach the junctions or not we still add this road segment
            
            roadSegments.Add(new RoadSegment    //  Our starting road
            {
                baseRoad = startRoad,
                referenceJunction = referenceJunction,
                offsetFromRefJunc = distToRefJuncFSS-searchDistToRefJunc,
                lengthOfSelection = searchDistToRefJunc+searchDistAwayFromRefJunc

            });

            

            //  Start main loop
            bool searching = true;
            while (searching)
            {
                //  Don't need to check if beyond max dist as we will only add (to visibleJunctions) if within limit
                if (visibleJunctions.Count != 0)
                {
                    //  Find closest visible junction
                    JunctionWithDij closestVisJunction = visibleJunctions[0];
                    for (int i = 1; i < visibleJunctions.Count; i++)
                    {
                        JunctionWithDij nextVisJunction = visibleJunctions[i];
                        if(nextVisJunction.dijDist < closestVisJunction.dijDist)
                        {
                            closestVisJunction = nextVisJunction;
                        }
                    }
                    //  Add all cars currently in that junction - We will not count this in the distance currently for simplicity
                    vehicles.AddRange(closestVisJunction.baseJunction.GetVehiclesInJunction());


                    //  Start looping through neighbouring roads
                    List<Road> connectedRoads = closestVisJunction.baseJunction.GetConnectedRoads();
                    foreach (Road currRoad in connectedRoads)
                    {
                        //  This bit is complicated - needs all the edge case handling here
                    }


                }
                else
                {
                    searching = false;
                }
                
            }
            
            

            //  The one we find will be the junction we visit next

            
            //  When we visit it check through all neighbour junctions (by looping through connected roads)
            //  If neighbour junctions are already visited then search through visibileRoadSegments to find the road you used to reach that
            //  When you find the road: (If not throw error)
            //  Check if road is fully covered ie is there overlap (If so add to fullyVisibileRoads and remove from roadSegments)
            //  If not fully covered, add our current roadSegment to the visibleRoadSegments (Now there will be two for that bit of road)
            //  If not already visited calculate a dist to that junction and add this to visibleJunctions
            //  Also add the relevant road segment for each road you loop through
            //  Repeat the main loop

            //  Loop through all the roadSegments we've made and add them to a list of nearby cars

            throw new NotImplementedException();
        }

        static void SwitchCarToNewRoad(Vehicle car, Road oldRoad, Road newRoad, float overshoot)
        {

            oldRoad.RemoveCar(car);
            newRoad.AddCar(car, overshoot); //  Also moves car into the new road

        }

        static void OnResize(SizeEventArgs sizeEventArg)
        {
            FloatRect visibleArea = new FloatRect(0, 0, sizeEventArg.Width, sizeEventArg.Height);
            window.SetView(new View(visibleArea));
        }

        static void OnKey(KeyEventArgs E, bool isPressed)
        {
            if (E.Code == Keyboard.Key.Escape)
            {
                Quit();
            }
        }

        static void Quit()
        {
            window.Close();
            Environment.Exit(0);
        }

    }
}
