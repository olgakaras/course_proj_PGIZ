using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Game.GameObjects.Objects
{
    class LiftPlatform : RotatiableObject
    {
        //private static readonly float LIFT_SPEED;

        public float LiftSpeed { get; set; }
        public Vector2 MinMax { get; set; }
        public int Up { get; set; }

        public LiftPlatform(Vector4 initialPosition, float radius, Vector2 minMax) : base(initialPosition, radius)
        {
            MinMax = minMax;
        }

        public Vector4 GetNewVerticalPosition(float speed)
        {
            Vector4 newPosition = Position;
            newPosition.Y += speed;

            return newPosition;
        }
    }
}
