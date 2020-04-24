using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Game.Animations
{
    public class VerticalSlideAnimation : Animation
    {
        private PositionalObject target;
        public VerticalSlideAnimation(List<PositionalObject> targetObjects) : base(targetObjects) 
        {
            target = TargetObjects[0];
            Parameters.Add("target_pos", 0f);
            Parameters.Add("speed", 0f);
            Parameters.Add("direction", 0);
        }
        public override void Animate()
        {
            if (Equals(target.Position.Y, (float)Parameters["target_pos"], 0.1f))
            {
                EndAnimation("slide");
            }
            target.Position += new Vector4(0, (float)Parameters["speed"] * (int)Parameters["direction"], 0, 0);
        }
    }
}
