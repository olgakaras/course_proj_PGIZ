using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Template.Graphics
{
    public class LightSource
    {
        public enum LightType
        {
            DirectionalLight = 0,
            PointLight = 1,
            SpotLight = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LightSourceDescription
        {
            public Vector4 position;
            public Vector4 direction;
            public Vector4 color;
            public int lightType;
            public float spotAngle;
            public float constantAttenuation;
            public float linearAttenuation;
            public float quadraticAttenuation;
            public int enabled;
            public Vector2 lPadding; // Padding to 16 byte boundary
        }

        private LightSourceDescription _lightSourceProperties;
        public LightSourceDescription LightSourceProperties { get => _lightSourceProperties; }

        public int LightSourceType { get => _lightSourceProperties.lightType; set => _lightSourceProperties.lightType = value; }
        public Vector4 Position { get => _lightSourceProperties.position; set => _lightSourceProperties.position = value; }
        public Vector4 Direction { get => _lightSourceProperties.direction; set => _lightSourceProperties.direction = value; }
        public Vector4 Color { get => _lightSourceProperties.color; set => _lightSourceProperties.color = value; }
        public float SpotAngle { get => _lightSourceProperties.spotAngle; set => _lightSourceProperties.spotAngle = value; }
        public float ConstantAttenuation { get => _lightSourceProperties.constantAttenuation; set => _lightSourceProperties.constantAttenuation = value; }
        public float LinearAttenuation { get => _lightSourceProperties.linearAttenuation; set => _lightSourceProperties.linearAttenuation = value; }
        public float QuadraticAttenuation { get => _lightSourceProperties.quadraticAttenuation; set => _lightSourceProperties.quadraticAttenuation = value; }
        public int Enabled { get => _lightSourceProperties.enabled; set => _lightSourceProperties.enabled = value; }

        private int _index;
        public int Index { get => _index; set => _index = value; }

        public LightSource()
        {
            _lightSourceProperties.lightType = (int)LightType.PointLight;
            _lightSourceProperties.position = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            _lightSourceProperties.direction = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
            _lightSourceProperties.color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            _lightSourceProperties.spotAngle = PositionalObject.HALF_PI;
            _lightSourceProperties.constantAttenuation = 1.0f;
            _lightSourceProperties.linearAttenuation = 0.2f;
            _lightSourceProperties.quadraticAttenuation = 0.1f;
            _lightSourceProperties.enabled = 0;
        }

        public LightSource(LightType lightType, Vector4 position, Vector4 direction, Vector4 color, float spotAngle,
            float constantAttenuation, float linearAttenuation, float quadraticAttenuation, int enabled)
        {
            _lightSourceProperties.lightType = (int)lightType;
            _lightSourceProperties.position = position;
            _lightSourceProperties.direction = direction;
            _lightSourceProperties.color = color;
            _lightSourceProperties.spotAngle = spotAngle;
            _lightSourceProperties.constantAttenuation = constantAttenuation;
            _lightSourceProperties.linearAttenuation = linearAttenuation;
            _lightSourceProperties.quadraticAttenuation = quadraticAttenuation;
            _lightSourceProperties.enabled = enabled;
        }
    }
}
