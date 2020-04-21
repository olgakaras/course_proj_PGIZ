using SharpDX;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Game.Services
{
    public class CameraService
    {
        private Camera camera;
        private InputController controller;

        private Dictionary<string, Key> inputs;
        Key this[string action]
        {
            set
            {
                inputs[action] = value; if (controller.ContainsButton(value)) controller.AddButton(value);
            }
        }
        public float MoveSpeed { get; set; }
        public float LookSpeed { get; set; }
        public CameraService(Camera camera, InputController controller)
        {
            this.camera = camera;
            this.controller = controller;
            inputs = new Dictionary<string, Key>
            {
                {"move_forward", Key.U},
                {"move_back", Key.J },
                { "move_left", Key.H},
                { "move_right", Key.K},
                { "move_up", Key.Y},
                { "move_down", Key.I},
                { "look_down", Key.Down},
                { "look_up", Key.Up},
                { "look_left", Key.Left},
                { "look_right", Key.Right}
            };
            MoveSpeed = 0.3f;
            LookSpeed = 0.01f;
            camera.Pitch = -0.73f;
            camera.Yaw = -PositionalObject.HALF_PI;
        }
        public Matrix GetViewMatrix()
        {
            return camera.GetViewMatrix();
        }

        public Matrix SetAfterResize(int width, int height)
        {
            camera.Aspect = width / (float)height;
            return camera.GetProjectionMatrix(width, height);
        }

        public void Update()
        {

            if (controller[inputs["look_up"]]) camera.PitchBy(LookSpeed);
            if (controller[inputs["look_down"]]) camera.PitchBy(-LookSpeed);
            if (controller[inputs["look_right"]]) camera.YawBy(LookSpeed);
            if (controller[inputs["look_left"]]) camera.YawBy(-LookSpeed);
            if (controller[inputs["move_left"]]) camera.MoveRightBy(-MoveSpeed);
            if (controller[inputs["move_right"]]) camera.MoveRightBy(MoveSpeed);
            if (controller[inputs["move_back"]]) camera.MoveForwardBy(-MoveSpeed);
            if (controller[inputs["move_forward"]]) camera.MoveForwardBy(MoveSpeed);
            if (controller[inputs["move_down"]]) camera.MoveUpBy(-MoveSpeed);
            if (controller[inputs["move_up"]]) camera.MoveUpBy(MoveSpeed);

        }

        public string GetDebugString()
        {
            return $"Camera_pos: {camera.description.pos}\n" +
                   $"Camera_target: {camera.description.target}\n" +
                   $"Camera_up: {camera.description.up}\n" + 
                   $"pitch: {camera.Pitch}\n" +
                   $"yaw: {camera.Yaw}\n" +
                   $"roll: {camera.Roll}\n";
        }
    }
}
