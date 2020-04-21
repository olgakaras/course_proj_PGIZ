using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;

namespace Template.Graphics
{
    class SamplerStates : IDisposable
    {
        private SamplerState _colored;
        public SamplerState Colored { get => _colored; }

        private SamplerState _textured;
        public SamplerState Textured { get => _textured; }

        public SamplerStates(DirectX3DGraphics directX3DGraphics)
        {
            SamplerStateDescription samplerStateDescription = new SamplerStateDescription
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                MipLodBias = 0.0f,
                MaximumAnisotropy = 1,
                ComparisonFunction = Comparison.Never,
                BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f),
                MinimumLod = 0,
                MaximumLod = float.MaxValue
            };
            _colored = new SamplerState(directX3DGraphics.Device, samplerStateDescription);

            samplerStateDescription = new SamplerStateDescription
            {
                Filter = Filter.Anisotropic,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                MipLodBias = 0.0f,
                MaximumAnisotropy = 2,
                ComparisonFunction = Comparison.Never,
                BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f),
                MinimumLod = 0,
                MaximumLod = float.MaxValue
            };
            _textured = new SamplerState(directX3DGraphics.Device, samplerStateDescription);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref _textured);
            Utilities.Dispose(ref _colored);
        }
    }
}
