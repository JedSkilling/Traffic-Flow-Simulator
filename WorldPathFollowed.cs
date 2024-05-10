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
        class WorldPathFollowed
        {
            //  Stores the visual appearence of a given road
            Vector2f startWorldPos;
            Vector2f endWorldPos;
            float WorldSpaceLength;

            List<Vector2f> controlPoints = new List<Vector2f>();
            //  Draw a line between startWorldPos, through controlPoints then to endWorldPos
            List<Drawable> linesToRender = new List<Drawable>();


            public WorldPathFollowed(Vector2f startPos, Vector2f endPos, List<Vector2f> controlPoints = null)
            {
                this.startWorldPos = startPos;
                this.endWorldPos = endPos;
                if (controlPoints != null)
                {
                    this.controlPoints = controlPoints;
                }
                this.WorldSpaceLength = CalculateLength();

                this.linesToRender = CalculateLinesToRender();
            }

            float CalculateLength()
            {
                float length = 0;
                Vector2f previousPoint = this.startWorldPos;
                Vector2f roadPath;
                for (int i = 0; i < controlPoints.Count; i++)
                {
                    roadPath = controlPoints[i] - previousPoint;
                    length += Magnitude(roadPath);
                    previousPoint = controlPoints[i];

                }
                roadPath = this.endWorldPos - previousPoint;
                length += Magnitude(roadPath);
                return length;
            }

            List<Drawable> CalculateLinesToRender()
            {
                List<Drawable> lineList = new List<Drawable>();
                Vector2f previousPoint = this.startWorldPos;
                Vector2f roadPath;
                Drawable lineSegment;
                for (int i = 0; i < controlPoints.Count; i++)
                {

                    lineSegment = Line(previousPoint, controlPoints[i], defaultColor: roadColour);
                    lineList.Add(lineSegment);

                    previousPoint = controlPoints[i];

                }
                lineSegment = Line(previousPoint, this.endWorldPos, defaultColor: roadColour);
                lineList.Add(lineSegment);
                return lineList;
            }

            public float[] GetCoordAndRotOfCar(float distFromStart, bool headedToEnd)   //  Should probably be inputting car or the car sprite here
            {
                //  distFromStart is in metres but we need it in pixels
                float pixelDistFromStart = distFromStart / metresPerPixel;
                //  First step is find the control points we are between
                float percentDistFromLastPoint = -1;    //  Default value should never be used
                float length = 0;
                Vector2f previousPoint = this.startWorldPos;
                Vector2f nextPoint = new Vector2f(0, 0);

                Vector2f roadPath;
                bool found = false;
                for (int i = 0; i < controlPoints.Count && found == false; i++)
                {
                    roadPath = controlPoints[i] - previousPoint;
                    length += Magnitude(roadPath);

                    if (length > pixelDistFromStart)
                    {
                        //  To get dist from the last control point we have to remove the magnitude of that line segment again
                        percentDistFromLastPoint = 1 + (pixelDistFromStart - length) / Magnitude(roadPath);
                        nextPoint = controlPoints[i];
                        found = true;

                    }
                    else
                    {
                        previousPoint = controlPoints[i];
                    }
                }
                if (found == false)
                {
                    roadPath = this.endWorldPos - previousPoint;
                    length += Magnitude(roadPath);
                    if (length >= pixelDistFromStart)
                    {
                        //  To get dist from the last control point we have to remove the magnitude of that line segment again
                        percentDistFromLastPoint = 1 + (pixelDistFromStart - length) / Magnitude(roadPath);
                        nextPoint = this.endWorldPos;
                        found = true;
                    }
                }

                if (found == false)
                {
                    Console.WriteLine("Didn't find car in world path");
                    throw new Exception();
                }


                Vector2f carWorldPos = lerp(previousPoint, nextPoint, percentDistFromLastPoint);



                //  Now we need to add an offset to put it to the side - Need to find which side of road is correct though
                //  previousPoint is closer to start - if we are headed from start we should be on left side (facing from previous to next point)
                //  If headed from end then on the right side and car should face towards the previous point

                Vector2f offsetFromRoad;
                float carRotation = GetDegreesBetweenPoints(new Vector2f(1, 0), nextPoint - previousPoint);
                Vector2f roadSegUnitVector = Normalise(nextPoint - carWorldPos);
                if (headedToEnd)
                {
                    offsetFromRoad = RotateByAngle(roadSegUnitVector, 90);
                }
                else
                {
                    offsetFromRoad = RotateByAngle(roadSegUnitVector, -90);
                    carRotation += 180;
                }
                offsetFromRoad *= (2 / metresPerPixel);    //  We want a 2 metre offset
                carWorldPos += offsetFromRoad;
                float[] output = new float[] { carWorldPos.X, carWorldPos.Y, carRotation };
                return output;


            }

            public List<Drawable> GetListToRender()
            {
                return linesToRender;
            }

            public float GetWorldSpaceLength()
            {
                return this.WorldSpaceLength;
            }

            public Vector2f GetStartPos()
            {
                return this.startWorldPos;
            }

            public Vector2f GetEndPos()
            {
                return this.endWorldPos;
            }
        }
    }
}
