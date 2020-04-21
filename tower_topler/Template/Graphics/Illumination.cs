using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer11 = SharpDX.Direct3D11.Buffer;

namespace Template.Graphics
{
    public class Illumination
    {
        public const int MaxLight = 8;

        [StructLayout(LayoutKind.Sequential)]
        public struct IlluminationDescription
        {
            public Vector4 eyePosition;
            public Vector4 globalAmbient;
            public LightSource.LightSourceDescription light0;
            public LightSource.LightSourceDescription light1;
            public LightSource.LightSourceDescription light2;
            public LightSource.LightSourceDescription light3;
            public LightSource.LightSourceDescription light4;
            public LightSource.LightSourceDescription light5;
            public LightSource.LightSourceDescription light6;
            public LightSource.LightSourceDescription light7;
        }

        private IlluminationDescription _illuminationProperties;
        public IlluminationDescription IlluminationProperties { get => _illuminationProperties; }

        private List<LightSource> _lightSources;

        public Vector4 EyePosition { get => _illuminationProperties.eyePosition; set => _illuminationProperties.eyePosition = value; }

        public Vector4 GlobalAmbient { get => _illuminationProperties.globalAmbient; set => _illuminationProperties.globalAmbient = value; }

        public LightSource this[int index]
        {
            get => _lightSources[index];
            set {
                if (index >= 0 && index < MaxLight)
                {
                    if (_lightSources.Count == index) _lightSources.Add(value);
                    else if (index < _lightSources.Count) _lightSources[index] = value;
                    if (0 == index) _illuminationProperties.light0 = _lightSources[index].LightSourceProperties;
                    else if (1 == index) _illuminationProperties.light1 = _lightSources[index].LightSourceProperties;
                    else if (2 == index) _illuminationProperties.light2 = _lightSources[index].LightSourceProperties;
                    else if (3 == index) _illuminationProperties.light3 = _lightSources[index].LightSourceProperties;
                    else if (4 == index) _illuminationProperties.light4 = _lightSources[index].LightSourceProperties;
                    else if (5 == index) _illuminationProperties.light5 = _lightSources[index].LightSourceProperties;
                    else if (6 == index) _illuminationProperties.light6 = _lightSources[index].LightSourceProperties;
                    else if (7 == index) _illuminationProperties.light7 = _lightSources[index].LightSourceProperties;
                }
            }
        }

        public Illumination(Vector4 eyePosition, Vector4 globalAmbient, LightSource[] lightSources)
        {
            _lightSources = new List<LightSource>(MaxLight);
            _illuminationProperties.eyePosition = eyePosition;
            _illuminationProperties.globalAmbient = globalAmbient;
            for (int i = 0; i < MaxLight; ++i)
                if (i < lightSources.Length) this[i] = lightSources[i];
                else this[i] = new LightSource();
        }
    }
}
