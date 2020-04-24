using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.Animations;
using Template.Game.gameObjects.newObjects;

namespace Template.Game
{
    public abstract class Animation
    {
        public delegate void AnimationHandler(object sender, AnimationEventArgs args);
        public event AnimationHandler AnimationEnded;
        public List<PositionalObject> TargetObjects { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public object this[string parameter]
        {
            get => Parameters[parameter];
            set => Parameters[parameter] = value;
        }

        public Animation(List<PositionalObject> targetObjects)
        {
            TargetObjects = targetObjects;
            Parameters = new Dictionary<string, object>();
        }

        public abstract void Animate();

        protected void EndAnimation(string type)
        {
            AnimationEnded?.Invoke(this, new AnimationEventArgs(TargetObjects, type));
        }

        protected void ClearHandlers()
        {
            AnimationEnded = delegate { };
        }

        protected bool Equals(float left, float right, float accuracy)
        {
            return Math.Abs(left - right) <= accuracy;
        }
    }
}
