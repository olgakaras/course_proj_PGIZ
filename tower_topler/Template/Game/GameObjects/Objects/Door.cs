using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Game.GameObjects.Objects
{
    public class Door : RotatiableObject
    {
        public Door OtherDoor { get; set; }
        public Door(Vector4 initialPosition, float radius) : base(initialPosition, radius)
        {
        }
    }
}
