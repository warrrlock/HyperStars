using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WesleyDavies
{
    /// <summary>
    /// A collection of Structs to make code more readable.
    /// </summary>
    public class Structs
    {
        public struct Direction
        {
            public readonly Vector3 DirectionVector;

            public Direction(Vector3 directionVector)
            {
                DirectionVector = directionVector;
            }
        }

        public class Directions
        {
            public readonly static Direction Up = new(Vector3.up);
            public readonly static Direction Down = new(Vector3.down);
            public readonly static Direction Left = new(Vector3.left);
            public readonly static Direction Right = new(Vector3.right);
            public readonly static Direction Forward = new(Vector3.forward);
            public readonly static Direction Backward = new(Vector3.back);
            public readonly static Direction Other = new(Vector3.zero);
        }

        public struct Side
        {
            public readonly Vector3 DirectionToSide;

            public Side(Vector3 directionToSide)
            {
                DirectionToSide = directionToSide;
            }
        }

        public class Sides
        {

        }
    }
}