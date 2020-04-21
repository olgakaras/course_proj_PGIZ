using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using SharpDX.Mathematics;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;

namespace Template
{
    /// <summary>
    /// DirectX2DGraphics is holder of Direct2D objects.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <listheader>DirectX2DGraphics holds:</listheader>
    /// <item>Factory,</item>
    /// <item>DirectWrite Factory,</item>
    /// <item>Windows Imaging Component Factory,</item>
    /// <item>Collection of text format objects,</item>
    /// <item>Collection of solid color brushes,</item>
    /// <item>Collection of bitmaps.</item>
    /// </list>
    /// <para>And uses to perform drawing commands.</para>
    /// <para>You need call BeforeResizeSwapChain and AfterResizeSwapChain methods before render.</para>
    /// </remarks>
    public class DirectX2DGraphics : IDisposable
    {
        /// <summary>DirectX 3D object.</summary>
        private DirectX3DGraphics _directX3DGraphics;

        /// <summary>Factory for Direct2D objects.</summary>
        private Factory _factory;
        /// <summary>Factory for Direct2D objects.</summary>
        /// <value>Factory for Direct2D objects.</value>
        public Factory Factory { get => _factory; }

        /// <summary>Factory for work with text.</summary>
        private SharpDX.DirectWrite.Factory _writeFactory;
        /// <summary>Factory for work with text.</summary>
        /// <value>Factory for work with text.</value>
        public SharpDX.DirectWrite.Factory WriteFactory { get => _writeFactory; }

        /// <summary>Windows Imaging Component factory.</summary>
        private ImagingFactory _imagingFactory;
        /// <summary>Windows Imaging Component factory.</summary>
        /// <value>Windows Imaging Component factory.</value>
        public ImagingFactory ImagingFactory { get => _imagingFactory; }

        /// <summary>Properties of rendet target.</summary>
        private RenderTargetProperties _renderTargetProperties;

        /// <summary>Drawing surface.</summary>
        private RenderTarget _renderTarget;
        /// <summary>Drawing surface.</summary>
        /// <value>Drawing surface.</value>
        public RenderTarget RenderTarget { get => _renderTarget; }

        /// <summary>Client rectangle of render target in device independent pixels (DIP).</summary>
        private RawRectangleF _renderTargetClientRectangle;
        /// <summary>Client rectangle of render target in device independent pixels (DIP).</summary>
        /// <value>Client rectangle of render target in device independent pixels (DIP).</value>
        public RawRectangleF RenderTargetClientRectangle { get => _renderTargetClientRectangle; }

        /// <summary>Text format resources collection.</summary>
        private List<SharpDX.DirectWrite.TextFormat> _textFormats;
        /// <summary>Text format resources collection.</summary>
        /// <value>Text format resources collection.</value>
        public List<SharpDX.DirectWrite.TextFormat> TextFormats { get => _textFormats; }

        /// <summary>Internal collection of brushes colors.</summary>
        /// <remarks>Oh. Brushes is render target dependend resources. Render target depend size of buffer of swap chain. And!
        /// When user resize render form we must resize swap chain and recreate all dependend resourses.</remarks>
        private List<RawColor4> _solidColorBrushesColors;
        /// <summary>Brush resources collection.</summary>
        private List<SolidColorBrush> _solidColorBrushes;
        /// <summary>Brush resources collection.</summary>
        /// <value>Brush resources collection.</value>
        public List<SolidColorBrush> SolidColorBrushes { get => _solidColorBrushes; }

        /// <summary>Internal collection of decoded first frames of bitmaps.</summary>
        /// <remarks>Oh. Bitmaps is render target dependend resources. Render target depend size of buffer of swap chain. And!
        /// When user resize render form we must resize swap chain and recreate all dependend resourses.</remarks>
        private List<BitmapFrameDecode> _decodedFirstFrameOfBitmaps;
        /// <summary>Bitmaps collection.</summary>
        private List<SharpDX.Direct2D1.Bitmap> _bitmaps;
        /// <summary>Bitmaps collection.</summary>
        /// <value>Bitmaps collection.</value>
        public List<SharpDX.Direct2D1.Bitmap> Bitmaps { get => _bitmaps; }

        /// <summary>
        /// Constructor create Factory, DirectWrite Factory, Windows Imaging Component (WIC) Factory. Add handlers
        /// BeforeResizeSwapChain and AfterResizeSwapChain to Resizing and Resized events of Direct3DGraphics objects.
        /// </summary>
        /// <param name="directX3DGraphics">DirectX 3D object.</param>
        /// <remarks>After creation call BeforeResizeSwapChain and AfterResizeSwapChain method before render.</remarks>
        public DirectX2DGraphics(DirectX3DGraphics directX3DGraphics)
        {
            _directX3DGraphics = directX3DGraphics;
            // Add resizing events handlers.
            _directX3DGraphics.Resizing += BeforeResizeSwapChain;
            _directX3DGraphics.Resized += AfterResizeSwapChain;

            // Create all factories.
            _factory = new Factory();
            _writeFactory = new SharpDX.DirectWrite.Factory();
            _imagingFactory = new ImagingFactory();

            // Render target properties.
            _renderTargetProperties.DpiX = 0;
            _renderTargetProperties.DpiY = 0;
            _renderTargetProperties.MinLevel = FeatureLevel.Level_10;
            _renderTargetProperties.PixelFormat = new SharpDX.Direct2D1.PixelFormat(
                SharpDX.DXGI.Format.Unknown,                              // SharpDX.DXGI.Format.R8G8B8A8_UNorm
                AlphaMode.Premultiplied);                                 // ????  Straight not supported
            _renderTargetProperties.Type = RenderTargetType.Hardware;     // Default
            _renderTargetProperties.Usage = RenderTargetUsage.None;

            // Create collections.
            _textFormats = new List<SharpDX.DirectWrite.TextFormat>(4);
            _solidColorBrushesColors = new List<RawColor4>(4);
            _solidColorBrushes = new List<SolidColorBrush>(4);
            _decodedFirstFrameOfBitmaps = new List<BitmapFrameDecode>(4);
            _bitmaps = new List<SharpDX.Direct2D1.Bitmap>(4);
        }

        /// <summary>Event handler of DirectX3DGraphics.Resizing event. Release some resources before resizing of swap chain.</summary>
        /// <remarks>Correct operation order: BeforeResizeSwapChain() - resizing swap chain - AfterResizeSwapChain().</remarks>
        public void BeforeResizeSwapChain(object sender, EventArgs e)
        {
            DisposeBitmaps();
            DisposeSolidColorBrushes();
            Utilities.Dispose(ref _renderTarget);
        }

        /// <summary>Create bitmap for decoded image (first frame) and add to bitmaps collection.</summary>
        private void CreateBitmap(BitmapFrameDecode decodedFirstFrameOfBitmap)
        {
            FormatConverter imageFormatConverter = new FormatConverter(_imagingFactory);
            imageFormatConverter.Initialize(
                decodedFirstFrameOfBitmap,
                SharpDX.WIC.PixelFormat.Format32bppPRGBA, // PRGBA = RGB premultiplied to alpha channel!!!!! YoPRST!
                BitmapDitherType.Ordered4x4, null, 0.0, BitmapPaletteType.Custom);
            SharpDX.Direct2D1.Bitmap bitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(_renderTarget, imageFormatConverter);
            _bitmaps.Add(bitmap);
            Utilities.Dispose(ref imageFormatConverter);
        }

        // TODO: Move delegates of resizing events from DirectX 2D object to Game object.
        /// <summary>Event handler for DirectX3DGraphics Resized event. Recreate render target and set to back buffer of swap chain.</summary>
        /// <remarks>Correct operation order: BeforeResizeSwapChain() - resizing swap chain - AfterResizeSwapChain().</remarks>
        public void AfterResizeSwapChain(object sender, EventArgs e)
        {
            // Render target is back buffer of swap chain.
            SharpDX.DXGI.Surface surface = _directX3DGraphics.BackBuffer.QueryInterface<SharpDX.DXGI.Surface>();
            _renderTarget = new RenderTarget(_factory, surface, _renderTargetProperties);
            Utilities.Dispose(ref surface);
            _renderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
            _renderTarget.TextAntialiasMode = TextAntialiasMode.Cleartype;

            // Set render target client rectangle in DIP.
            _renderTargetClientRectangle.Left = 0;
            _renderTargetClientRectangle.Top = 0;
            _renderTargetClientRectangle.Right = _renderTarget.Size.Width;
            _renderTargetClientRectangle.Bottom = _renderTarget.Size.Height;

            // Recreate brushes.
            if (_solidColorBrushesColors.Count > 0)
                for (int i = 0; i <= _solidColorBrushesColors.Count - 1; ++i)
                    _solidColorBrushes.Add(new SolidColorBrush(_renderTarget, _solidColorBrushesColors[i]));

            // Recreate bitmaps.
            if (_decodedFirstFrameOfBitmaps.Count > 0)
                for (int i = 0; i <= _decodedFirstFrameOfBitmaps.Count - 1; ++i)
                    CreateBitmap(_decodedFirstFrameOfBitmaps[i]);
        }

        /// <summary>Create text format object and add to internal collection.</summary>
        /// <param name="fontFamilyName">Font family name.</param>
        /// <param name="fontWeight">Font weight.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="fontStretch">Font stretch.</param>
        /// <param name="fontSize">Font size in DIP.</param>
        /// <param name="textAlignment">Horizontal alignment.</param>
        /// <param name="paragraphAlignment">Vertical alignment.</param>
        /// <returns>Text format index.</returns>
        public int NewTextFormat(string fontFamilyName, SharpDX.DirectWrite.FontWeight fontWeight,
            SharpDX.DirectWrite.FontStyle fontStyle, SharpDX.DirectWrite.FontStretch fontStretch, float fontSize,
            SharpDX.DirectWrite.TextAlignment textAlignment, SharpDX.DirectWrite.ParagraphAlignment paragraphAlignment)
        {
            SharpDX.DirectWrite.TextFormat textFormat = new SharpDX.DirectWrite.TextFormat(_writeFactory, fontFamilyName, fontWeight,
                fontStyle, fontStretch, fontSize);
            textFormat.TextAlignment = textAlignment;
            textFormat.ParagraphAlignment = paragraphAlignment;
            _textFormats.Add(textFormat);
            return _textFormats.Count - 1;
        }

        /// <summary>Create solid color brush and add to internal collection.</summary>
        /// <param name="color">Color with alpha channel.</param>
        /// <returns>Brush index.</returns>
        public int NewSolidColorBrush(RawColor4 color)
        {
            _solidColorBrushesColors.Add(color);
            int index = _solidColorBrushesColors.Count - 1;
            if (null != _renderTarget)
                _solidColorBrushes.Add(new SolidColorBrush(_renderTarget, color));
            return index;
        }

        /// <summary>Load bitmap (.bmp) from file and place into internal collection.</summary>
        /// <param name="imageFileName">File name of bitmap with path.</param>
        /// <returns>Index of added bitmap.</returns>
        public int LoadBitmapFromFile(string imageFileName)
        {
            BitmapDecoder decoder = new BitmapDecoder(_imagingFactory, imageFileName, DecodeOptions.CacheOnDemand);
            BitmapFrameDecode bitmapFirstFrame = decoder.GetFrame(0);
            _decodedFirstFrameOfBitmaps.Add(bitmapFirstFrame);
            int index = _decodedFirstFrameOfBitmaps.Count - 1;
            if (null != _renderTarget)
                CreateBitmap(bitmapFirstFrame);

            Utilities.Dispose(ref decoder);

            return index;
        }

        /// <summary>Begin 2D draw operations.</summary>
        public void BeginDraw()
        {
            _renderTarget.BeginDraw();
        }

        /// <summary>Draw text.</summary>
        /// <param name="text">String to draw.</param>
        /// <param name="textFormatIndex">Index of text format in internal collection.</param>
        /// <param name="layoutRectangle">Rectangle in witch draw text.</param>
        /// <param name="brushIndex">Index of brush in internal collection.</param>
        public void DrawText(string text, int textFormatIndex, RawRectangleF layoutRectangle, int brushIndex)
        {
            _renderTarget.Transform = Matrix3x2.Identity;
            _renderTarget.DrawText(text, _textFormats[textFormatIndex], layoutRectangle, _solidColorBrushes[brushIndex]);
        }

        /// <summary>Draw bitmap AKA sprite.</summary>
        /// <param name="bitmapIndex">Index of bitmap in internal collection.</param>
        /// <param name="transformMatrix">Coordinate transform matrix.</param>
        /// <param name="opacity">Opacity.</param>
        /// <param name="interpolationMode">Interpolation mode.</param>
        public void DrawBitmap(int bitmapIndex, Matrix3x2 transformMatrix, float opacity,
            SharpDX.Direct2D1.BitmapInterpolationMode interpolationMode)
        {
            _renderTarget.Transform = transformMatrix;
            _renderTarget.DrawBitmap(_bitmaps[bitmapIndex], opacity, interpolationMode);
        }

        /// <summary>End 2D draw operations.</summary>
        public void EndDraw()
        {
            _renderTarget.EndDraw();
        }

        /// <summary>Release brushes.</summary>
        public void DisposeSolidColorBrushes()
        {
            for (int i = _solidColorBrushes.Count - 1; i >= 0; i--)
            {
                SolidColorBrush brush = _solidColorBrushes[i];
                _solidColorBrushes.RemoveAt(i);
                Utilities.Dispose(ref brush);
            }
        }

        /// <summary>Release Direct2D bitmaps.</summary>
        public void DisposeBitmaps()
        {
            for (int i = _bitmaps.Count - 1; i >= 0; i--)
            {
                SharpDX.Direct2D1.Bitmap bitmap = _bitmaps[i];
                _bitmaps.RemoveAt(i);
                Utilities.Dispose(ref bitmap);
            }
        }

        /// <summary>Release of all used resourses.</summary>
        public void Dispose()
        {
            DisposeBitmaps();
            for (int i = _decodedFirstFrameOfBitmaps.Count - 1; i >= 0; i--)
            {
                BitmapFrameDecode bitmapFirstFrame = _decodedFirstFrameOfBitmaps[i];
                _decodedFirstFrameOfBitmaps.RemoveAt(i);
                Utilities.Dispose(ref bitmapFirstFrame);
            }
            DisposeSolidColorBrushes();
            for (int i = _textFormats.Count - 1; i >= 0; i--)
            {
                SharpDX.DirectWrite.TextFormat textFormat = _textFormats[i];
                _textFormats.RemoveAt(i);
                Utilities.Dispose(ref textFormat);
            }
            Utilities.Dispose(ref _imagingFactory);
            Utilities.Dispose(ref _writeFactory);
            Utilities.Dispose(ref _factory);
        }
    }
}
