using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device11 = SharpDX.Direct3D11.Device;
using Buffer11 = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D;

namespace Template
{
    /// <summary>
    /// DirectX3DGraphics is holder of DirectX3D objects.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <listheader>DirectX3DGraphics holds:</listheader>
    /// <item>Device,</item>
    /// <item>SwapChain,</item>
    /// <item>Render context,</item>
    /// <item>Sampler,</item>
    /// <item>Buffers (depth, stencil, back),</item>
    /// <item>View objects (for render target, depth and stencil buffer).</item>
    /// </list>
    /// <para>And uses for manipulation of buffers.</para>
    /// <para>You need call Resize method before render.</para>
    /// </remarks>
    public class DirectX3DGraphics : IDisposable
    {
        /// <summary>Rendering mode: solid or wireframe.</summary>
        public enum RenderModes
        {
            Solid,
            Wireframe
        }

        /// <summary>Render form.</summary>
        private RenderForm _renderForm;
        /// <summary>Render form.</summary>
        public RenderForm RenderForm { get => _renderForm; }

        /// <summary>Sampling description: multi-sampling parameters.</summary>
        private SampleDescription _sampleDescription;
        /// <summary>Gets the sample description.</summary>
        /// <value>The sample description.</value>
        public SampleDescription SampleDescription { get => _sampleDescription; }

        /// <summary>
        /// Swap chain description: countVertices of buffers (back buffers), video mode, windowed or full screen, sampling, swap effect, usage.
        /// </summary>
        private SwapChainDescription _swapChainDescription;

        /// <summary>Rasterizer Stage description: fill mode, culling mode, multi-sampling, AA, depth clipping.</summary>
        private RasterizerStateDescription _rasterizerStateDescription;

        /// <summary>DirectX3D device.</summary>
        private Device11 _device;
        /// <summary>DirectX3D device.</summary>
        /// <value>DirectX3D device.</value>
        public Device11 Device { get => _device; }

        /// <summary>Swap chain.</summary>
        private SwapChain _swapChain;
        /// <summary>Swap Chain.</summary>
        public SwapChain SwapChain { get => _swapChain; }

        /// <summary>Device render context.</summary>
        private DeviceContext _deviceContext;
        /// <summary>Device render context.</summary>
        /// <value>Device render context.</value>
        public DeviceContext DeviceContext { get => _deviceContext; }

        /// <summary>Rasterizer State object.</summary>
        private RasterizerState _rasterizerState;

        /// <summary>Factory for DirectX3D objects.</summary>
        private Factory _factory;

        /// <summary>Back buffer of Swap Chain.</summary>
        private Texture2D _backBuffer;
        /// <summary>Back buffer of Swap Chain.</summary>
        /// <value>Back buffer of Swap Chain.</value>
        public Texture2D BackBuffer { get => _backBuffer; }

        /// <summary>Render Target for view.</summary>
        private RenderTargetView _renderView = null;

        /// <summary>Dept and Stencil buffer description.</summary>
        private Texture2DDescription _depthStencilBufferDescription;

        /// <summary>Depth and Stencil buffers.</summary>
        private Texture2D _depthStencilBuffer = null;

        /// <summary>View object for Depth and Stencil buffers.</summary>
        private DepthStencilView _depthStencilView = null;

        /// <summary>Rendering mode: solid or wireframe.</summary>
        private RenderModes _renderMode = RenderModes.Solid;
        /// <summary>Rendering mode: solid or wireframe.</summary>
        /// <value>Rendering mode: solid or wireframe.</value>
        public RenderModes RenderMode
        {
            get { return _renderMode; }
            set
            {
                if (value != _renderMode)
                {
                    _renderMode = value;
                    if (_renderMode == RenderModes.Solid)
                    {
                        Utilities.Dispose(ref _rasterizerState);
                        _rasterizerStateDescription.FillMode = FillMode.Solid;
                        _rasterizerState = new RasterizerState(_device, _rasterizerStateDescription);
                        _deviceContext.Rasterizer.State = _rasterizerState;
                    } else
                    {
                        Utilities.Dispose(ref _rasterizerState);
                        _rasterizerStateDescription.FillMode = FillMode.Wireframe;
                        _rasterizerState = new RasterizerState(_device, _rasterizerStateDescription);
                        _deviceContext.Rasterizer.State = _rasterizerState;
                    }
                }
            }
        }

        /// <summary>Is render form windowed or full screen.</summary>
        private bool _isFullScreen;
        /// <summary>Is render form windowed or full screen.</summary>
        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set
            {
                if (value != _isFullScreen)
                {
                    _isFullScreen = value;
                    _swapChain.SetFullscreenState(_isFullScreen, null);
                }
            }
        }

        /// <summary>Invoked before resizing.</summary>
        public event EventHandler Resizing;

        /// <summary>Invoked after resizing.</summary>
        public event EventHandler Resized;

        /// <summary>
        /// Constructor create structures, Device, SwapChain, DeviceContext, RasterizerState, Factory and make window associtiaon.
        /// </summary>
        /// <param name="renderForm">SharpDX render form.</param>
        /// <remarks>After creation call Resize method before render.</remarks>
        public DirectX3DGraphics(RenderForm renderForm)
        {
            _renderForm = renderForm;

            // For debug.
            Configuration.EnableObjectTracking = true;

            // Sampling description.
            _sampleDescription =
                //new SampleDescription(4, (int)StandardMultisampleQualityLevels.StandardMultisamplePattern); // 4xMSAA. In base tutors used (1, 0).
                new SampleDescription(1, 0);

            // SwapChain description.
            _swapChainDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(_renderForm.ClientSize.Width, _renderForm.ClientSize.Height,
                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = _renderForm.Handle,
                SampleDescription = _sampleDescription,
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Rasterizer state description.
            _rasterizerStateDescription = RasterizerStateDescription.Default();
            _rasterizerStateDescription.FillMode = _renderMode == RenderModes.Solid ? FillMode.Solid : FillMode.Wireframe;
            _rasterizerStateDescription.CullMode = CullMode.Back;
            _rasterizerStateDescription.IsFrontCounterClockwise = true;
            _rasterizerStateDescription.IsMultisampleEnabled = true;
            _rasterizerStateDescription.IsAntialiasedLineEnabled = true;
            _rasterizerStateDescription.IsDepthClipEnabled = true;

            //Configuration.EnableObjectTracking = true;

            // Create Device and SwapChain.
            Device11.CreateWithSwapChain(DriverType.Hardware,
                DeviceCreationFlags.BgraSupport, // |                                // ???DeviceCreationFlags.None. For Direct2D need BGRA support
                //DeviceCreationFlags.Debug, //| DeviceCreationFlags.Debuggable,   // Not for all devices and drivers
                _swapChainDescription, out _device, out _swapChain);

            //int q_l = _device.CheckMultisampleQualityLevels(Format.R8G8B8A8_SInt, 4); Check@@@@
            //Console.WriteLine(q_l);
            //q_l = _device.CheckMultisampleQualityLevels(Format.R32G32B32_Float, 4);
            //Console.WriteLine(q_l);

            // Device visualization context.
            _deviceContext = _device.ImmediateContext;

            // Rasterizer state.
            _rasterizerState = new RasterizerState(_device, _rasterizerStateDescription);
            _deviceContext.Rasterizer.State = _rasterizerState;

            // Factory.
            _factory = _swapChain.GetParent<Factory>();

            // Ignore all windows events.
            _factory.MakeWindowAssociation(_renderForm.Handle, WindowAssociationFlags.IgnoreAll);

            // Description for Dept and Stencil buffer.
            _depthStencilBufferDescription = new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt, // Depth - denormalized 32 bit float, stencil - short unsigned 8 bit int, 24 bit not used added for 32 bit aligned.
                ArraySize = 1,
                MipLevels = 1,
                Width = _renderForm.ClientSize.Width,
                Height = _renderForm.ClientSize.Height,
                SampleDescription = _sampleDescription,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
        }

        /// <summary>Resize Depth, Stencil and Back buffers.</summary>
        /// <remarks>Must be called before render for creation of buffers.</remarks>
        public void Resize()
        {
            // Invoke Resizing event.
            Resizing?.Invoke(this, EventArgs.Empty);

            // Dispose all previous allocated resources.
            Utilities.Dispose(ref _depthStencilView);
            Utilities.Dispose(ref _depthStencilBuffer);
            Utilities.Dispose(ref _renderView);
            Utilities.Dispose(ref _backBuffer);

            // Resize the backbuffer.
            _swapChain.ResizeBuffers(_swapChainDescription.BufferCount, _renderForm.ClientSize.Width, _renderForm.ClientSize.Height,
                Format.Unknown, SwapChainFlags.None);

            // Get the backbuffer from the swapchain.
            _backBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);

            // Renderview on the backbuffer.
            _renderView = new RenderTargetView(_device, _backBuffer);

            // Create the depth and stencil buffer.
            _depthStencilBufferDescription.Width = _renderForm.Width;
            _depthStencilBufferDescription.Height = _renderForm.Height;
            _depthStencilBuffer = new Texture2D(_device, _depthStencilBufferDescription);

            // Create the depth buffer view.
            _depthStencilView = new DepthStencilView(_device, _depthStencilBuffer);

            // Setup targets and viewport for rendering.
            _deviceContext.Rasterizer.SetViewport(new Viewport(0, 0, _renderForm.ClientSize.Width, _renderForm.ClientSize.Height, 0.0f, 1.0f));
            _deviceContext.OutputMerger.SetTargets(_depthStencilView, _renderView);

            // Invoke Resized event.
            Resized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Clear Dept and Stencil buffer and Render Target View.</summary>
        /// <param name="backgroundColor">Color for fill Render Target View.</param>
        public void ClearBuffers(Color backgroundColor)
        {
            // Clear depth buffer and render view
            _deviceContext.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            _deviceContext.ClearRenderTargetView(_renderView, backgroundColor);
        }

        /// <summary>Release of all used resourses.</summary>
        public void Dispose()
        {
            Utilities.Dispose(ref _depthStencilView);
            Utilities.Dispose(ref _depthStencilBuffer);
            Utilities.Dispose(ref _renderView);
            Utilities.Dispose(ref _backBuffer);
            Utilities.Dispose(ref _factory);
            Utilities.Dispose(ref _rasterizerState);
            Utilities.Dispose(ref _deviceContext);
            Utilities.Dispose(ref _swapChain);
            Utilities.Dispose(ref _device);
        }
    }
}
