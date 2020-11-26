using System;
using System.Collections.Generic;
using System.Text;

namespace Flocking
{
    public class FlockingSimulation
    {
        int canvasWidth;
        int canvasHeight;

        const int visualRange = 75;

        public Bird[] Birds { get; private set; }

        public FlockingSimulation(int canvasWidth, int canvasHeight, int birdCount)
        {
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;

            Birds = new Bird[birdCount];

            for(int i=0; i < birdCount; i++)
            {
                Birds[i] = new Bird(canvasWidth, canvasHeight);
            }
        }


        float distance(Bird bird1, Bird bird2)
        {
            return (float) Math.Sqrt(
                (bird1.PositionX - bird2.PositionX) * (bird1.PositionX - bird2.PositionX) +
                (bird1.PositionY - bird2.PositionY) * (bird1.PositionY - bird2.PositionY));
        }


        // Constrain a bird to within the canvas. If it gets too close to an edge, reverse its direction over a number of frames.
        void KeepWithinBounds(Bird boid)
        {
            // The margin is the distance from the edge of the canvas that a bird will start to turn
            const float margin = 200.0f;
            const float turnFactor = 1.0f;

            if (boid.PositionX < margin)
            {
                boid.DirectionX += turnFactor;
            }
            if (boid.PositionX > canvasWidth - margin)
            {
                boid.DirectionX -= turnFactor;
            }
            if (boid.PositionY < margin)
            {
                boid.DirectionY += turnFactor;
            }
            if (boid.PositionY > canvasHeight- margin)
            {
                boid.DirectionY -= turnFactor;
            }
        }


        // Find the center of mass of the other birds and adjust velocity slightly to point towards the center of mass.
        void FlyTowardsCenter(Bird bird)
        {
            // Adjust velocity by this %
            const float centeringFactor = 0.005f; 

            float centerX = 0.0f;
            float centerY = 0.0f;
            int numNeighbors = 0;
            
            foreach (Bird otherBoid in Birds)
            {
                if(bird == otherBoid)
                {
                    continue;
                }

                if (distance(bird, otherBoid) < visualRange)
                {
                    centerX += otherBoid.PositionX;
                    centerY += otherBoid.PositionY;
                    numNeighbors += 1;
                }
            }

            if (numNeighbors > 0)
            {
                centerX = centerX / numNeighbors;
                centerY = centerY / numNeighbors;

                bird.DirectionX += (centerX - bird.PositionX) * centeringFactor;
                bird.DirectionY += (centerY - bird.PositionY) * centeringFactor;
            }
        }



        // Find the average velocity (speed and direction) of the surrounding birds and adjust velocity slightly to match.
        void MatchVelocity(Bird boid)
        {
            const float matchingFactor = 0.05f; // Adjust by this % of average velocity

            float avgDX = 0;
            float avgDY = 0;
            int numNeighbors = 0;

            foreach (Bird otherBoid in Birds)
            {
                if (boid == otherBoid)
                {
                    continue;
                }


                if (distance(boid, otherBoid) < visualRange)
                {
                    avgDX += otherBoid.DirectionX;
                    avgDY += otherBoid.DirectionY;
                    numNeighbors += 1;
                }
            }

            if (numNeighbors > 0)
            {
                avgDX = avgDX / numNeighbors;
                avgDY = avgDY / numNeighbors;

                boid.DirectionX += (avgDX - boid.DirectionX) * matchingFactor;
                boid.DirectionY += (avgDY - boid.DirectionY) * matchingFactor;
            }
        }

        // Move away from other boids that are too close to avoid colliding
        void AvoidOtherBirds(Bird boid)
        {
            const float minDistance = 20.0f; // The distance to stay away from other boids
            const float avoidFactor = 0.05f; // Adjust velocity by this %
            float moveX = 0.0f;
            float moveY = 0.0f;
            foreach (Bird otherBoid in Birds)
            {
                if (boid != otherBoid)
                {
                    if (distance(boid, otherBoid) < minDistance)
                    {
                        moveX += boid.PositionX - otherBoid.PositionX;
                        moveY += boid.PositionY - otherBoid.PositionY;
                    }
                }
            }

            boid.DirectionX += moveX * avoidFactor;
            boid.DirectionY += moveY * avoidFactor;
        }

        public void Update()
        {
            foreach(Bird bird in Birds)
            {
                FlyTowardsCenter(bird);
                AvoidOtherBirds(bird);
                MatchVelocity(bird);
                KeepWithinBounds(bird);

                bird.LimitSpeed();

                // Birds fly forwards
                bird.PositionX += bird.DirectionX;
                bird.PositionY += bird.DirectionY;
            }
        }
    }
}
