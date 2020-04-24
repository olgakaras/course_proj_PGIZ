using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.interfaces;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.GameObjects.Objects
{
    public class RotatiableObject : DrawableObject
    {
        public float Radius { get; set; }
        public int Horizontal { get; set; }
        public RotatiableObject(Vector4 initialPosition, float radius) : base(initialPosition)
        {
            Horizontal = 1;
            Radius = radius;
        }

        public Vector4 GetNewHorizontalPosition(float speed, out float yaw)
        {
            yaw = Yaw + speed * Horizontal;
            Vector4 newPosition = Position;
            newPosition.X = (float)(Math.Cos(-yaw) * Radius);
            newPosition.Z = (float)(Math.Sin(-yaw) * Radius);

            return newPosition;
        }

        public Vector4 GetNewHorizontalPosition(float angle)
        {
            Vector4 newPosition = Position;
            newPosition.X = (float)(Math.Cos(-angle) * Radius);
            newPosition.Z = (float)(Math.Sin(-angle) * Radius);

            return newPosition;
        }
    }
}
