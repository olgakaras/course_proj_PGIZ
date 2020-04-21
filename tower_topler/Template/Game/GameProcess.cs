using System;
using System.Text;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Windows;
using Template.Graphics;
using System.Drawing;
using Template.Game.Services;
using Template.Game.gameObjects.newObjects;
using Template.Game.GameObjects.Services;

namespace Template
{
    class GameProcess : IDisposable
    {
        public struct HUDResources
        {
            public int textFPSTextFormatIndex;
            public int textFPSBrushIndex;
            public int heartIconIndex;
            public int arrowIconIndex;
        }

        private RenderForm renderForm;
        private DirectX3DGraphics directX3DGraphics;
        private Renderer renderer;

        private DirectX2DGraphics directX2DGraphics;

        private SamplerStates samplerStates;
        private Illumination illumination;


        private HUDResources hudResources;

        private CameraService cameraService;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;
        

        private InputController inputController;


        private TimeHelper timeHelper;
        private bool isFirstRun = true;

        private TestGameService gameService;

        public GameProcess()
        {
            Initialize3DGraphics();
            directX2DGraphics = new DirectX2DGraphics(directX3DGraphics);
            Loader loader = new Loader(directX3DGraphics, directX2DGraphics, renderer, directX2DGraphics.ImagingFactory);

            timeHelper = new TimeHelper();

            InitHUDResources();
            InitializeLight();

            inputController = new InputController(renderForm);

            gameService = new TestGameService(loader, inputController);
            cameraService = new CameraService(new Camera(new Vector4(-116.0f, 84.0f, 0.0f, 1.0f)), inputController);
        }

        private void Initialize3DGraphics()
        {
            renderForm = new RenderForm("SharpDX")
            {
                ClientSize = new Size(1500, 800)
            };

            renderForm.UserResized += RenderFormResizedCallback;
            renderForm.Activated += RenderFormActivatedCallback;
            renderForm.Deactivate += RenderFormDeactivateCallback;
            directX3DGraphics = new DirectX3DGraphics(renderForm);
            renderer = new Renderer(directX3DGraphics);
            renderer.CreateConstantBuffers();

            samplerStates = new SamplerStates(directX3DGraphics);
        }

        private void RenderFormResizedCallback(object sender, EventArgs args)
        {
            directX3DGraphics.Resize();
            projectionMatrix = cameraService.SetAfterResize(renderForm.ClientSize.Width, renderForm.ClientSize.Height);
        }

        private void RenderFormActivatedCallback(object sender, EventArgs args)
        {
            Cursor.Hide();
        }

        private void RenderFormDeactivateCallback(object sender, EventArgs args)
        {
            Cursor.Show();
        }

        private void InitHUDResources()
        {
            hudResources.textFPSTextFormatIndex = directX2DGraphics.NewTextFormat("Input", SharpDX.DirectWrite.FontWeight.Normal,
                SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, 12,
                SharpDX.DirectWrite.TextAlignment.Leading, SharpDX.DirectWrite.ParagraphAlignment.Near);
            hudResources.textFPSBrushIndex = directX2DGraphics.NewSolidColorBrush(new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 0.0f, 1.0f));
        }

        
        /// <summary>Callback for RenderLoop.Run. Handle input and render scene.</summary>
        private void RenderLoopCallback()
        {
            if (isFirstRun)
            {
                RenderFormResizedCallback(this, new EventArgs());
                isFirstRun = false;
            }

            timeHelper.Update();
            //_inputController.UpdateKeyboardState();
            inputController.UpdateMouseState();

            UpdateKeyBoard();

            gameService.Update();

            viewMatrix = cameraService.GetViewMatrix();

            renderer.BeginRender();

            renderer.UpdateIlluminationProperties(illumination);

            renderer.SetPerObjectConstants(timeHelper.Time, 0);//1);
            float angle = timeHelper.Time * 2.0f * (float)Math.PI * 0.25f; // Frequency = 0.25 Hz
            //_cube.Pitch = angle;

            float time = timeHelper.Time;
            renderer.SetPerObjectConstants(time, 0);

            gameService.Render(viewMatrix, projectionMatrix);
            RenderHUD();

            renderer.EndRender();
        }

        private void InitializeLight()
        {
            illumination = new Illumination(Vector4.Zero, new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new LightSource[]
            {
                new LightSource(LightSource.LightType.DirectionalLight,
                    new Vector4(-40.0f, 10.0f, 0.0f, 1.0f),   // Position
                    new Vector4(10.0f, -20.0f, 0.0f, 1.0f),   // Direction
                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f),    // Color
                    0.0f,                                   // Spot angle
                    1.0f,                                   // Const atten
                    1.0f,                                   // Linear atten
                    1.0f,                                   // Quadratic atten
                    1),
                new LightSource(LightSource.LightType.SpotLight,
                    new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    new Vector4(0.0f, -1.0f, 0.0f, 1.0f),
                    new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                    PositionalObject.HALF_PI / 4.0f,
                    1.0f,
                    0.05f,
                    0.01f,
                    0),
                new LightSource(LightSource.LightType.PointLight,
                    new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                    Vector4.Zero,
                    new Vector4(-4.0f, 1.0f, 0.0f, 1.0f),
                    1.0f,
                    1.0f,
                    0.05f,
                    0.005f,
                    0),
                new LightSource(),
                new LightSource(),
                new LightSource(),
                new LightSource(),
                new LightSource()
            });
        }

        private void RenderHUD()
        {
            StringBuilder description = new StringBuilder();
            description.Append($"FPS: {timeHelper.FPS,3:d2}").Append('\n');
            description.Append($"Time: {timeHelper.Time:f1}").Append('\n');
            description.Append(cameraService.GetDebugString()).Append('\n');

            directX2DGraphics.BeginDraw();
            directX2DGraphics.DrawText(description.ToString(), hudResources.textFPSTextFormatIndex, directX2DGraphics.RenderTargetClientRectangle, hudResources.textFPSBrushIndex);
            directX2DGraphics.EndDraw();
        }

        private void UpdateKeyBoard()
        {
            inputController.UpdateKeyboardState();
            if (inputController.Esc) renderForm.Close();                               

            if (inputController.Func[1]) directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Solid;
            if (inputController.Func[2]) directX3DGraphics.RenderMode = DirectX3DGraphics.RenderModes.Wireframe;
            if (inputController.Func[3]) directX3DGraphics.IsFullScreen = false;
            if (inputController.Func[4]) directX3DGraphics.IsFullScreen = true;
            cameraService.Update();
        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderLoopCallback);
        }

        public void Dispose()
        {
            samplerStates.Dispose();
            inputController.Dispose();
            directX2DGraphics.Dispose();
            renderer.Dispose();
            directX3DGraphics.Dispose();
            renderForm.Dispose();
        }
    }
}
