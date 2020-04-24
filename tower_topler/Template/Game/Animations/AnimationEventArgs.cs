using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.newObjects;

namespace Template.Game.Animations
{
    public class AnimationEventArgs
    {
        public List<PositionalObject> AnimationObjects { get; set; }
        public string AnimationType { get; set; }

        public AnimationEventArgs(List<PositionalObject> animationObjects, string animationType)
        {
            AnimationObjects = animationObjects;
            AnimationType = animationType;
        }
    }
}
