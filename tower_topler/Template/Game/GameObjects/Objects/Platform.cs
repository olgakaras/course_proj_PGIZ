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
    public class Platform : DrawableObject
    {
        public int Horizontal { get; set; }
        public Platform(Vector4 initialPosition) : base(initialPosition)
        {
            Horizontal = 1;
        }

        public Vector4 GetNewHorizontalPosition(float speed, float radius, out float yaw)
        {
            yaw = Yaw + speed * Horizontal;
            Vector4 newPosition = Position;
            newPosition.X = (float)(Math.Cos(-yaw) * radius);
            newPosition.Z = (float)(Math.Sin(-yaw) * radius);

            return newPosition;
        }
    }
}
