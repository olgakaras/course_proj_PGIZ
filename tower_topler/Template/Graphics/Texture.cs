using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.WIC;

namespace Template.Graphics
{
    public class Texture : IDisposable
    {
        private Texture2D _textureObject;
        public Texture2D TextureObject { get => _textureObject; }

        private ShaderResourceView _shaderResourceView;
        public ShaderResourceView ShaderResourceView { get => _shaderResourceView; }

        private int _width;
        public int Width { get => _width; }

        private int _height;
        public int Height { get => _height; }

        private string _name;
        public string Name { get => _name; }

        private SamplerState _samplerState;
        public SamplerState SamplerState { get => _samplerState; }

        private int _index;
        public int Index { get => _index; set => _index = value; }

        public Texture(Texture2D textureObject, ShaderResourceView shaderResourceView, int width, int height, string name, SamplerState samplerState)
        {
            _textureObject = textureObject;
            _shaderResourceView = shaderResourceView;
            _width = width;
            _height = height;
            _name = name;
            _samplerState = samplerState;
        }

        public void Dispose()
        {
            Utilities.Dispose(ref _shaderResourceView);
            Utilities.Dispose(ref _textureObject);
        }
    }
}
