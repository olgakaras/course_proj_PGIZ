using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Template
{
    public abstract class PositionalObject
    {
        public static readonly float PI = (float)Math.PI;
        public static readonly float TWO_PI = (float)Math.PI * 2.0f;
        public static readonly float HALF_PI = (float)Math.PI / 2.0f;

        protected Vector4 position;
        public virtual Vector4 Position { get => position; set => position = value; }
        public virtual float Yaw { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public PositionalObject(Vector4 initialPosition)
        {
            position = initialPosition;
        }

        public virtual void MoveTo(float x, float y, float z)
        {
            position.X = x;
            position.Y = y;
            position.Z = z;
        }

        public Matrix GetWorldMatrix()
        {
            return Matrix.Multiply(Matrix.RotationYawPitchRoll(Yaw, Pitch, Roll), Matrix.Translation((Vector3)position));
        }

        public virtual void YawBy(float deltaYaw)
        {
            Yaw += deltaYaw;
            if (Yaw > PI) Yaw -= TWO_PI;
            else if (Yaw < -PI) Yaw += TWO_PI;
        }

        public virtual void PitchBy(float deltaPitch)
        {
            Pitch += deltaPitch;
            if (Pitch > PI) Pitch -= TWO_PI;
            else if (Pitch < -PI) Pitch += TWO_PI;
        }

        /// <summary>Rotate around 0Z axis by deltaRoll (x - to left, y - to up, z - to back), rad.</summary>
        /// <param name="deltaRoll">Angle, rad.</param>
        public virtual void RollBy(float deltaRoll)
        {
            Roll += deltaRoll;
            if (Roll > PI) Roll -= TWO_PI;
            else if (Roll < -PI) Roll += TWO_PI;
        }

        public virtual void MoveByDirection(float speed, string direction)
        {
            switch (direction)
            {
                case "forward": position.X += speed; break;
                case "back": position.X -= speed; break;
                case "left": position.Z += speed; break;
                case "right": position.Z -= speed; break;
            }
        }

        /// <summary>Move forward or backward (depends of moveBy sign: positive - forward). Yaw = 0 - look to -Z.</summary>
        /// <param name="moveBy">Amount of movement.</param>
        public void MoveForwardBy(float moveBy)
        {
            position.X -= moveBy * (float)Math.Sin(Yaw);
            position.Z -= moveBy * (float)Math.Cos(Yaw);
        }

        public void MoveUpBy(float moveBy)
        {
            position.Y += moveBy;
        }

        /// <summary>Move to right or to left (depends of moveBy sign: positive - to right).</summary>
        /// <param name="moveBy">Amount of movement.</param>
        public void MoveRightBy(float moveBy)
        {
            position.X -= moveBy * (float)Math.Cos(Yaw);
            position.Z += moveBy * (float)Math.Sin(Yaw);
        }
    }
}
