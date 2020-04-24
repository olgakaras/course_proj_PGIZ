using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.newObjects;
using Template.Game.GameObjects.Objects;

namespace Template.Game.Animations
{
    public class RotateSceneAnimation : Animation
    {
        public RotateSceneAnimation(List<PositionalObject> targetObjects) : base(targetObjects)
        {
            Parameters.Add("target_pos", Vector4.Zero);
            Parameters.Add("target_angle", 0.0f);
            Parameters.Add("angle", 0.0f);
            Parameters.Add("radius", 0.0f);
        }

        public override void Animate()
        {
            float yaw = (float)Parameters["target_angle"];
            Vector4 targetPos = GetNewPos((Vector4)Parameters["target_pos"], ref yaw);

            Parameters["target_pos"] = targetPos;
            Parameters["target_angle"] = yaw;

            float radius = (float)Parameters["radius"];
            if (Equals(targetPos.X, (float)Math.Cos(PositionalObject.PI) * radius, 0.01f))
            {
                EndAnimation("rotate");
            }
            float delta = (float)Parameters["angle"];
            TargetObjects.ForEach(w => {
                w.Position = ((RotatiableObject)w).GetNewHorizontalPosition(delta, out yaw);
                w.Yaw = yaw;
            });
        }

        private Vector4 GetNewPos(Vector4 initialPos, ref float yaw)
        {
            float delta = (float)Parameters["angle"];
            float radius = (float)Parameters["radius"];

            return GetNewPos(initialPos, delta, radius, ref yaw);
        }

        private Vector4 GetNewPos(Vector4 initialPos, float delta, float radius, ref float yaw)
        {
            yaw += delta;
            Vector4 newPos = initialPos;
            newPos.X = (float)(Math.Cos(-yaw) * radius);
            newPos.Z = (float)(Math.Sin(-yaw) * radius);

            return newPos;
        }

        
    }
}
