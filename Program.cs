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
        private struct VisibleJunction  //  Used in FindNearbyCars function
        {
            public Junction junction { get; set; }
            public float distFromOrigin { get; set; }
        }

        private struct RoadSegment
        {
            public Road road { get; set; }
            public Junction referenceJunction { get; set; }
            public float offsetFromJunc { get; set; }
            public float lengthOfSelection { get; set; }
        }



        static RenderWindow window;
        static float fps = 60;

        //  Creating colours
        static Color backgroundColour = new Color(30, 57, 73);
        static Color junctionColour = new Color(0, 255, 98);
        static Color roadColour = new Color(0, 255, 215);
        static Color carColour = new Color(187, 0, 255);    //  Created from texture so this isn't used

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
                    //  Fills in the junctionSprites list to be drawn + starts filling in carSprites (from junction paths)
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
                    
                    carDrawables.AddRange(currRoad.CreateCarDrawables());
                    
                    
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

        static List<Vehicle> FindNearbyCars(float distFromReferenceJunction, Junction referenceJunction, Road startRoad, float searchDistToRefJunc, float searchDistAwayFromRefJunc, Vehicle? startVehicle = null)  //  Counts junctions as having zero distance
        {
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
                Console.WriteLine("Search offset greater than road we started on")
            }
            List<VisibleJunction> visibleJunctions = new List<VisibleJunction>();
            List<RoadSegment> roadSegments = new List<RoadSegment>();
            List<Road> fullyVisibleRoads = new List<Road>();


            //  Handle starting case as it more complicated
            Junction[] connectedJunctions = startRoad.GetStartAndEndJunction();
            Junction otherJunction = null;
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
            float distToRefJuncFromSearchStart = 




            //  Find which junction is ahead and which is behind

            //  Add both to seen junctions along with a current distance (If current dist is within our limit, distance will be adjusted for searchDistBehind and Ahead)

            //  Check if we fail to reach both junctions, if so add the relevant road segment to a list of roadSegments

            //  Start main loop
            //  Loop through all visible junctions and find the nearest one to origin
            //  Don't need to check if beyond max dist as we will only add if within limit

            //  The one we find will be the junction we visit next

            //  Add all cars currently in that junction - We will not count this in the distance currently for simplicity
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
