using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.Animations
{
    public class RotationAnimation : Animation
    {
        private string type;
        public RotationAnimation(DrawableObject targetObject, string type) : base(targetObject) 
        {
            this.type = type;
            Parameters.Add("initialRotation", null);
            Parameters.Add("targetRotation", null);
            Parameters.Add("offset", null);
        }

        public override void Animate()
        {
            switch(type)
            {
                case "yawRotation":
                    YawAnimation();
                    break;
            }
        }

        public void YawAnimation()
        {
            float target = (float)Parameters["targetRotation"];
            float offset = (float)Parameters["offset"];
            SetRightParameters(ref target, ref offset);
            if (Equals(TargetObject.Yaw, target, 0.03f))
            {
                TargetObject.Yaw = GetRightYaw(target);
                EndAnimation("yawRotation");
                ClearHandlers();
                return;
            }
            TargetObject.Yaw += offset;
        }

        private bool Equals(float x1, float x2, float precission)
        {
            return Math.Abs(x1 - x2) <= precission;
        }

        private void SetRightParameters(ref float target, ref float offset)
        {
            if (target - (float)Parameters["initialRotation"] > PositionalObject.PI)
            {
                target = -(target - PositionalObject.PI);
                offset = -offset;
            }
            else if(target - (float)Parameters["initialRotation"] < -PositionalObject.PI)
            {
                target = PositionalObject.TWO_PI;
            }
            else if (target - (float)Parameters["initialRotation"] < 0)
            {
                offset = -offset;
            }
        }

        private float GetRightYaw(float actual)
        {
            if (Equals(actual, -PositionalObject.HALF_PI, 0.03f)) return PositionalObject.PI + PositionalObject.HALF_PI;
            if (Equals(actual, PositionalObject.TWO_PI, 0.03f)) return 0;

            return actual;
        }
    }
}
