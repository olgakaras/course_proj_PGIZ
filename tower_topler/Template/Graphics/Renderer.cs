using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Device11 = SharpDX.Direct3D11.Device;
using Buffer11 = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D;
using System.Runtime.InteropServices;
using Template.Graphics;

namespace Template
{
    /// <summary>
    /// Renderer is holder of Shader's program, input layout.
    /// </summary>
    public class Renderer : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PerObjectConstantBuffer // For vertex shader (b0)
        {
            public Matrix worldMatrix;
            public Matrix worldViewMatrix;
            public Matrix inverseTransposeMatrix;
            public Matrix worldViewProjectionMatrix;
            public float time;
            public int timeScaling;
            public Vector2 oPadding; // Padding to 16 byte boundary
        }

        /// <summary>DirectX3DGraphics object, witch holds DirectX3D objects.</summary>
        /// See <see cref="DirectX3DGraphics"/>.
        private DirectX3DGraphics _directX3DGraphics;

        /// <summary>Vertex Shader's program.</summary>
        private VertexShader _vertexShader;

        /// <summary>Pixel Shader's program.</summary>
        private PixelShader _pixelShader;

        /// <summary>Input signature of vertex shader program.</summary>
        private ShaderSignature _shaderSignature;

        /// <summary>Input layout of vertex shader program.</summary>
        private InputLayout _inputLayout;

        /// <summary>Constant buffer with per object data (matrices).</summary>
        private PerObjectConstantBuffer _perObjectConstantBuffer;

        /// <summary>Constant buffer DirectX object with per object data (matrices).</summary>
        private Buffer11 _perObjectConstantBufferObject;

        private Buffer11 _materialConstantBuffer;

        private Buffer11 _illuminationConstantBuffer;

        /// <summary>Create Renderer instance and compile shader's programs.</summary>
        /// <param name="directX3DGraphics">Inctance of <see cref="DirectX3DGraphics"/>.</param>
        public Renderer(DirectX3DGraphics directX3DGraphics)
        {
            _directX3DGraphics = directX3DGraphics;
            Device11 device = _directX3DGraphics.Device;
            DeviceContext deviceContext = _directX3DGraphics.DeviceContext;

            // Compile Vertex and Pixel shaders
            CompilationResult vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shaders\\vertex.hlsl", "vertexShader", "vs_5_0");
            _vertexShader = new VertexShader(device, vertexShaderByteCode);
            CompilationResult pixelShaderByteCode = ShaderBytecode.CompileFromFile("Shaders\\pixel.hlsl", "pixelShader", "ps_5_0");
            _pixelShader = new PixelShader(device, pixelShaderByteCode);

            // Input elements.
            InputElement[] inputElements = new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 32, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 48, 0),
                new InputElement("TEXCOORD", 1, Format.R32G32_Float, 56, 0)
            };

            // Layout from VertexShader input signature
            _shaderSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            _inputLayout = new InputLayout(device, _shaderSignature, inputElements);

            Utilities.Dispose(ref vertexShaderByteCode);
            Utilities.Dispose(ref pixelShaderByteCode);

            // Prepare All the stages
            deviceContext.InputAssembler.InputLayout = _inputLayout;
            deviceContext.VertexShader.Set(_vertexShader);
            deviceContext.PixelShader.Set(_pixelShader);
        }

        /// <summary>Set fields of PerObjectConstantBuffer</summary>
        /// <param name="constants"></param>
        public void SetPerObjectConstants(float time, int timeScaling)
        {
            _perObjectConstantBuffer.time = time;
            _perObjectConstantBuffer.timeScaling = timeScaling;
        }

        /// <summary>Create constant buffers.</summary>
        public void CreateConstantBuffers()
        {
            Device11 device = _directX3DGraphics.Device;
            DeviceContext deviceContext = _directX3DGraphics.DeviceContext;

            _perObjectConstantBufferObject = new Buffer11(
                device,
                Utilities.SizeOf<PerObjectConstantBuffer>(),
                ResourceUsage.Dynamic,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.Write,
                ResourceOptionFlags.None,
                0);
            //deviceContext.VertexShader.SetConstantBuffer(0, _perObjectConstantBufferObject);

            _materialConstantBuffer = new Buffer11(
                device,
                Utilities.SizeOf<Material.MaterialDescription>(),
                ResourceUsage.Dynamic,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.Write,
                ResourceOptionFlags.None,
                0);
            //deviceContext.PixelShader.SetConstantBuffer(0, _materialConstantBuffer);

            _illuminationConstantBuffer = new Buffer11(
                device,
                Utilities.SizeOf<Illumination.IlluminationDescription>(),
                ResourceUsage.Dynamic,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.Write,
                ResourceOptionFlags.None,
                0);
            //deviceContext.PixelShader.SetConstantBuffer(1, _illuminationConstantBuffer);
            //deviceContext.PixelShader.SetConstantBuffers(0, 2, new Buffer11[] { _materialConstantBuffer, _illuminationConstantBuffer });
        }

        /// <summary>Begin render - clear buffers.</summary>
        public void BeginRender()
        {
            // Clear depth and stencil buffer and render view
            _directX3DGraphics.ClearBuffers(Color.LightBlue);
        }

        /// <summary>Render 3D object.</summary>
        /// <param name="index">Index of object in internal list.</param>
        /// <param name="world">World transformation matrix.</param>
        /// <param name="view">View transformation matrix.</param>
        /// <param name="projection">Projection transformation matrix.</param>
        public void UpdatePerObjectConstantBuffer(int index, Matrix world, Matrix view, Matrix projection)
        {
            DeviceContext deviceContext = _directX3DGraphics.DeviceContext;
            _perObjectConstantBuffer.worldMatrix = world;
            _perObjectConstantBuffer.inverseTransposeMatrix = Matrix.Invert(world);
            _perObjectConstantBuffer.worldMatrix.Transpose();
            _perObjectConstantBuffer.worldViewMatrix = Matrix.Multiply(world, view);
            _perObjectConstantBuffer.worldViewProjectionMatrix = Matrix.Multiply(_perObjectConstantBuffer.worldViewMatrix, projection);
            _perObjectConstantBuffer.worldViewMatrix.Transpose();
            _perObjectConstantBuffer.worldViewProjectionMatrix.Transpose();
            //deviceContext.UpdateSubresource<PerObjectConstantBuffer>(ref _perObjectConstantBuffer, _perObjectConstantBufferObject);
            DataStream dataStream;
            deviceContext.MapSubresource(_perObjectConstantBufferObject, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out dataStream);
            dataStream.Write(_perObjectConstantBuffer);
            deviceContext.UnmapSubresource(_perObjectConstantBufferObject, 0);
            deviceContext.VertexShader.SetConstantBuffer(0, _perObjectConstantBufferObject);
        }

        public void SetWhiteTexture(Texture whiteTexture)
        {
            DeviceContext deviceContext = _directX3DGraphics.DeviceContext;
            deviceContext.PixelShader.SetShaderResource(0, whiteTexture.ShaderResourceView);
            deviceContext.PixelShader.SetSampler(0, whiteTexture.SamplerState);
        }

        public void UpdateMaterialProperties(Material material)
        {
            DeviceContext deviceContext = _directX3DGraphics.DeviceContext;
            Material.MaterialDescription materialDescription = material.MaterialProperties;
            //deviceContext.UpdateSubresource<Material.MaterialDescription>(ref materialDescription, _materialConstantBuffer);
            DataStream dataStream;
            deviceContext.MapSubresource(_materialConstantBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out dataStream);
            dataStream.Write(material.MaterialProperties);
            deviceContext.UnmapSubresource(_materialConstantBuffer, 0);
            deviceContext.PixelShader.SetConstantBuffer(0, _materialConstantBuffer);

            deviceContext.PixelShader.SetShaderResource(1, material.Texture.ShaderResourceView);
            deviceContext.PixelShader.SetSampler(1, material.Texture.SamplerState);
        }

        public void UpdateIlluminationProperties(Illumination illumination)
        {
            DeviceContext deviceContext = _directX3DGraphics.DeviceContext;
            //deviceContext.UpdateSubresource<Illumination.IlluminationDescription>(ref illuminationDescription, _illuminationConstantBuffer);
            DataStream dataStream;
            deviceContext.MapSubresource(_illuminationConstantBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out dataStream);
            dataStream.Write(illumination.IlluminationProperties);
            deviceContext.UnmapSubresource(_illuminationConstantBuffer, 0);
            deviceContext.PixelShader.SetConstantBuffer(1, _illuminationConstantBuffer);
        }

        /// <summary>Present frame buffer.</summary>
        public void EndRender()
        {
            _directX3DGraphics.SwapChain.Present(1, PresentFlags.Restart); //(0, PresentFlags.None); $$$$$$$$ Vert sync!!!
        }

        /// <summary>Release of all used resourses.</summary>
        public void Dispose()
        {
            Utilities.Dispose(ref _illuminationConstantBuffer);
            Utilities.Dispose(ref _materialConstantBuffer);
            Utilities.Dispose(ref _perObjectConstantBufferObject);
            Utilities.Dispose(ref _inputLayout);
            Utilities.Dispose(ref _shaderSignature);
            Utilities.Dispose(ref _pixelShader);
            Utilities.Dispose(ref _vertexShader);
        }
    }
}
