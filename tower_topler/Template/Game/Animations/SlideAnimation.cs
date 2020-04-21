using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.Animations
{
    public class SlideAnimation : Animation
    {
        public SlideAnimation(DrawableObject targetObject) : base(targetObject)
        {
            Parameters.Add("targetPosition", null);
            Parameters.Add("offset", null);
        }

        public override void Animate()
        {
            if ((Vector4)Parameters["targetPosition"] == null && (Vector4)Parameters["offset"] == null)
                return;
            if (TargetObject.Position == (Vector4)Parameters["targetPosition"])
            {
                EndAnimation("slide");
                ClearHandlers();
                return;
            }
            TargetObject.Position += (Vector4)Parameters["offset"];
        }
    }
}
