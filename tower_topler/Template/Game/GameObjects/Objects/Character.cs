using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Game.gameObjects.newObjects;
using Template.Graphics;

namespace Template.Game.gameObjects.interfaces
{
    public class Character : DrawableObject 
    {
        protected static readonly float HORIZONTAL_SPEED = 0.01f;
        public static readonly float VERCTICAL_SPEED = 1.6f;
        public static readonly float GRAVITY = 0.1f;
        protected static readonly int HEALTH = 6;

        public override Vector4 Position 
        { 
            get => base.Position; 
            set 
            {
                base.Position = value;
                Ray = new Ray((Vector3)Position, new Vector3(0, -1, 0));
            } 
        }
        public bool IsFlying { get; set; }
        public Ray Ray { get; set; }

        public float Offset { get; set; }
        public bool IsActive { get; set; }
        public bool IsAlive { get; set; }
        public int Health { get; set; }

        public Vector2 Speed;
        public float VSpeed { get; set; }

        public int Horizontal { get; set; }
        public Character(Vector4 initialPosition) : base(initialPosition) 
        {
            Horizontal = 0;
            Speed = new Vector2(HORIZONTAL_SPEED, 0);
            VSpeed = 0;
            Health = HEALTH;
            IsAlive = true;
            IsActive = false;
            Ray = new Ray((Vector3)Position, new Vector3(0, -1, 0));
        }

        public virtual Vector4 GetNewHorizontalPosition()
        {
            return GetNewHorizontalPosition(Speed.X);
        }

        public virtual Vector4 GetNewVerticalPosition()
        {
            return GetNewVerticalPosition(Speed.Y);
        }

        public virtual Vector4 GetNewHorizontalPosition(float speed)
        {
            Vector4 newPosition = position;
            newPosition.Z += Horizontal * speed;
            return newPosition;
        } 

        public virtual Vector4 GetNewVerticalPosition(float speed)
        {
            Vector4 newPosition = position;
            newPosition.Y += speed;
            return newPosition;
        }

        public virtual void GetDamage(int damage)
        {
            Health = (Health - damage < 0) ? 0 : Health - damage;
            if (Health == 0)
            {
                IsAlive = false;
            }
        }
    }
}
