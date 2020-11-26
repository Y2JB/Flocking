using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flocking
{
    public class Bird
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        //float directionX, directionY;
        //public Vector2 Direction { get; set; }

        public float DirectionX { get; set; }
        public float DirectionY { get; set; }
        

        public Bird(int canvasWidth, int canvasHeight)
        {
            Random rnd = new Random();
            PositionX = (float) rnd.Next(0, canvasWidth);
            PositionY = (float) rnd.Next(0, canvasHeight);

            DirectionX = (float) rnd.NextDouble(); 
            DirectionY = (float) rnd.NextDouble(); 
        }

        public float Speed()
        {
            Vector2 direction = new Vector2(DirectionX, DirectionY);
            return direction.Length();
        }


        // Speed will naturally vary in flocking behavior, but real animals can't go arbitrarily fast.
        public void LimitSpeed()
        {
            const float speedLimit = 15.0f;

            float speed = Speed();

            if (speed > speedLimit)
            {
                // Cap the speed 
                DirectionX = (DirectionX / speed) * speedLimit;
                DirectionY = (DirectionY / speed) * speedLimit;
            }
        }
    }
}
