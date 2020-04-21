using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Template.Graphics
{
    public class Material
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MaterialDescription // For pixel shader (b0)
        {
            public Vector4 emissive;
            public Vector4 ambient;
            public Vector4 diffuse;
            public Vector4 specular;
            public float specularPower;
            public int textured;
            public Vector2 mPadding; // Padding to 16 byte boundary
        }

        private string _name;
        public string Name { get => _name; set => _name = value; }

        private MaterialDescription _materialProperties;
        public MaterialDescription MaterialProperties { get => _materialProperties; }

        public Vector4 Emissive { get => _materialProperties.emissive; set => _materialProperties.emissive = value; }
        public Vector4 Ambient { get => _materialProperties.ambient; set => _materialProperties.ambient = value; }
        public Vector4 Diffuse { get => _materialProperties.diffuse; set => _materialProperties.diffuse = value; }
        public Vector4 Specular { get => _materialProperties.specular; set => _materialProperties.specular = value; }
        public float SpecularPower { get => _materialProperties.specularPower; set => _materialProperties.specularPower = value; }
        public bool Textured { get => (0 != _materialProperties.textured); set => _materialProperties.textured = (value ? 1 : 0); }

        private Texture _texture;
        public Texture Texture { get => _texture; }

        private int _index;
        public int Index { get => _index; set => _index = value; }

        public Material()
        {
            _materialProperties.emissive = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            _materialProperties.ambient = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            _materialProperties.diffuse = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            _materialProperties.specular = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            _materialProperties.specularPower = 32.0f;
            _materialProperties.textured = 0;
            _texture = null;
        }

        public Material(string name, Vector4 emissive, Vector4 ambient, Vector4 diffuse, Vector4 specular, float specularPower, bool textured, Texture texture)
        {
            _name = name;
            _materialProperties.emissive = emissive;
            _materialProperties.ambient = ambient;
            _materialProperties.diffuse = diffuse;
            _materialProperties.specular = specular;
            _materialProperties.specularPower = specularPower;
            _materialProperties.textured = (textured ? 1 : 0);
            _texture = texture;
        }
    }
}
