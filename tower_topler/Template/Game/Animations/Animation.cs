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
        public DrawableObject TargetObject { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public Animation(DrawableObject targetObject)
        {
            TargetObject = targetObject;
            Parameters = new Dictionary<string, object>();
        }

        public abstract void Animate();

        protected void EndAnimation(string type)
        {
            AnimationEnded?.Invoke(this, new AnimationEventArgs(TargetObject, type));
        }

        protected void ClearHandlers()
        {
            AnimationEnded = delegate { };
        }
    }
}
