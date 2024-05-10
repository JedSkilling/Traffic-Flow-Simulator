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
        static RectangleShape Line(Vector2f startPoint, Vector2f endPoint, Color? defaultColor = null, float thickness = 2)
        {
            if (defaultColor == null)
            {
                defaultColor = Color.White;
            }

            RectangleShape shape = new RectangleShape();
            shape.Size = new Vector2f(Magnitude(endPoint - startPoint), thickness);
            shape.Position = startPoint;
            shape.FillColor = (Color)defaultColor;
            float theta = GetDegreesBetweenPoints(startPoint, endPoint);
            shape.Rotation = theta;
            return shape;
        }

        static float GetDegreesBetweenPoints(Vector2f startPoint, Vector2f endPoint, Vector2f? midPoint = null)    //  Default is reference to origin
        {
            if (midPoint != null)
            {
                startPoint -= (Vector2f)midPoint;
                endPoint -= (Vector2f)midPoint;
            }
            float theta;
            if (endPoint.X - startPoint.X >= 0)
            {
                if (endPoint.Y - startPoint.Y >= 0)
                {
                    theta = MathF.Atan((endPoint.Y - startPoint.Y) / (endPoint.X - startPoint.X)) * (180 / MathF.PI);
                }
                else
                {
                    theta = MathF.Atan((endPoint.Y - startPoint.Y) / (endPoint.X - startPoint.X)) * (180 / MathF.PI) + 360;
                }
            }
            else
            {
                theta = MathF.Atan((endPoint.Y - startPoint.Y) / (endPoint.X - startPoint.X)) * (180 / MathF.PI) + 180;
            }
            return theta;
        }

        static void SwitchCarToNewRoad(Vehicle car, Road oldRoad, Road newRoad, float overshoot)
        {

            oldRoad.RemoveCar(car);
            newRoad.AddCar(car, overshoot); //  Also moves car into the new road

        }

        static float Magnitude(Vector2f input)
        {
            float m = 0;
            m = MathF.Sqrt(input.X * input.X + input.Y * input.Y);
            return m;

        }

        static Vector2f Normalise(Vector2f input)
        {
            Vector2f output = input / Magnitude(input);
            return output;
        }

        static Vector2f RotateByAngle(Vector2f input, float degrees)
        {
            float radians = degrees * MathF.PI / 180f;
            Vector2f output = new Vector2f(input.X * MathF.Cos(radians) - input.Y * MathF.Sin(radians), input.X * MathF.Sin(radians) + input.Y * MathF.Cos(radians));
            return output;
        }
        static Vector2f lerp(Vector2f A, Vector2f B, float t)
        {
            Vector2f C = A + (B - A) * t;
            return C;
        }
    }
}
