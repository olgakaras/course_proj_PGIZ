using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectInput;
using SharpDX.Windows;

namespace Template
{
    /// <summary>
    /// Controller of keyboard and mouse input.
    /// </summary>
    public class InputController : IDisposable
    {
        /// <summary>Direct Input instance.</summary>
        private DirectInput _directInput;

        /// <summary>Keyboard.</summary>
        private Keyboard _keyboard;

        /// <summary>Keyboard state.</summary>
        private KeyboardState _keyboardState;
        
        /// <summary>Is keyboard state successfully updated.</summary>
        private bool _keyboardUpdated;
        /// <summary>Is keyboard state successfully updated.</summary>
        /// <value>Is Keyboard state successfully updated.</value>
        public bool KeyboardUpdated { get => _keyboardUpdated; }

        /// <summary>Is keyboard successfully acquired.</summary>
        private bool _keyboardAcquired;

        /// <summary>Mouse.</summary>
        private Mouse _mouse;

        /// <summary>Mouse state.</summary>
        private MouseState _mouseState;
        
        /// <summary>Is mouse successfully updated.</summary>
        private bool _mouseUpdated;
        /// <summary>Is mouse successfully updated.</summary>
        /// <value>Is mouse successfully updated.</value>
        public bool MouseUpdated { get => _mouseUpdated; }

        /// <summary>Is mouse successfully acquired.</summary>
        private bool _mouseAcquired;

        /// <summary>Ctrl (left or right) current pressed state.</summary>
        private bool _controlPressed;
        /// <summary>Ctrl (left or right) current pressed state.</summary>
        /// <value>Ctrl (left or right) current pressed state.</value>
        public bool ControlPressed { get => _controlPressed; }

        /// <summary>shift (left or right) current pressed state.</summary>
        private bool _shiftPressed;
        /// <summary>shift (left or right) current pressed state.</summary>
        /// <value>shift (left or right) current pressed state.</value>
        public bool shiftPressed { get => _shiftPressed; }

        /// <summary>Esc previous state.</summary>
        private bool _escPreviousPressed;
        /// <summary>Esc current state.</summary>
        private bool _escCurrentPressed;
        /// <summary>Esc key press "event".</summary>
        private bool _esc;
        /// <summary>Esc key press "event".</summary>
        /// <value>Esc key press "event".</value>
        public bool Esc { get => _esc; }

        /// <summary>Array of F1 - F10 key codes.</summary>
        private static Key[] _funcKeys = new Key[10] { Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9, Key.F10 };

        /// <summary>F1 - F10 previous state.</summary>
        private bool[] _funcPreviousPressed = new bool[10];
        /// <summary>F1 - F10 current state.</summary>
        private bool[] _funcCurrentPressed = new bool[10];
        /// <summary>F1 - F10 key press "event".</summary>
        private bool[] _func = new bool[10];
        /// <summary>F1 - F10 key press "event".</summary>
        /// <value>F1 - F10 key press "event".</value>
        public bool[] Func { get => _func; }

        /// <summary>Space previous state.</summary>
        private bool _spacePreviousPressed;
        /// <summary>Space current state.</summary>
        private bool _spaceCurrentPressed;
        /// <summary>Space key press "event".</summary>
        private bool _space;
        /// <summary>Space key press "event".</summary>
        /// <value>Space key press "event".</value>
        public bool Space { get => _esc; }
        private Dictionary<Key, bool> controlButtons;

        public bool this[Key key]
        {
            get => controlButtons[key];
            set => controlButtons[key] = value;
        }

        public bool this[Key key1, Key key2]
        {
            get => (this[key1] && this[key2]) ? true : false;
        }
        /// <summary>Mouse button press "event" (0 - left, 1 - right, 2 - middle, ... 7).</summary>
        private bool[] _mouseButtons = new bool[8];
        /// <summary>Mouse button press "event" (0 - left, 1 - right, 2 - middle, ... 7).</summary>
        /// <value>Mouse button press "event" (0 - left, 1 - right, 2 - middle, ... 7).</value>
        public bool[] MouseButtons { get => _mouseButtons; }

        /// <summary>Mouse X relative position from previous update.</summary>
        private int _mouseRelativePositionX;
        /// <summary>Mouse X relative position from previous update.</summary>
        /// <value>Mouse X relative position from previous update.</value>
        public int MouseRelativePositionX { get => _mouseRelativePositionX; }

        /// <summary>Mouse Y relative position from previous update.</summary>
        private int _mouseRelativePositionY;
        /// <summary>Mouse Y relative position from previous update.</summary>
        /// <value>Mouse Y relative position from previous update.</value>
        public int MouseRelativePositionY { get => _mouseRelativePositionY; }

        /// <summary>Mouse Z relative position (scroll) from previous update.</summary>
        private int _mouseRelativePositionZ;
        /// <summary>Mouse Z relative position (scroll) from previous update.</summary>
        /// <value>Mouse Z relative position (scroll) from previous update.</value>
        public int MouseRelativePositionZ { get => _mouseRelativePositionZ; }
        public List<Key> PressedButtons { get; set; }
        /// <summary>
        /// Constructor create DirectInput, Keyboard, 
        /// </summary>
        /// <param name="renderForm">Render form.</param>
        public InputController(RenderForm renderForm)
        {
            controlButtons = new Dictionary<Key, bool>()
            {
                {Key.W, false},
                {Key.A, false},
                {Key.S, false},
                {Key.D, false},
                {Key.Up, false },
                {Key.Down, false },
                {Key.Left, false },
                {Key.Right, false },
                {Key.U, false },
                {Key.J, false },
                {Key.H, false },
                {Key.K, false },
                {Key.Y, false },
                {Key.I, false },
                {Key.G, false },
                {Key.F, false },
                {Key.Space, false }
            };
            _directInput = new DirectInput();

            _keyboard = new Keyboard(_directInput);
            _keyboard.SetCooperativeLevel(renderForm.Handle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);
            AcquireKeyboard();
            _keyboardState = new KeyboardState();

            _mouse = new Mouse(_directInput);
            _mouse.Properties.AxisMode = DeviceAxisMode.Relative;
            _mouse.SetCooperativeLevel(renderForm.Handle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);
            AcquireMouse();
            _mouseState = new MouseState();
        }

        /// <summary>Try to acquire keyboard.</summary>
        private void AcquireKeyboard()
        {
            try
            {
                _keyboard.Acquire();
                _keyboardAcquired = true;
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Failure)
                    _keyboardAcquired = false;
            }
        }

        /// <summary>Try to acquire mouse.</summary>
        private void AcquireMouse()
        {
            try
            {
                _mouse.Acquire();
                _mouseAcquired = true;
            }
            catch (SharpDXException e)
            {
                if (e.ResultCode.Failure)
                    _mouseAcquired = false;
            }
        }

        /// <summary>Fire key "event" if current state - up, previous - down.</summary>
        /// <param name="key">Key.</param>
        /// <param name="previousPressed">Previous state.</param>
        /// <param name="currentPressed">Current state.</param>
        /// <returns>Event firing flag.</returns>
        private bool ProcessKeyPressTriggerByKeyUp(Key key, ref bool previousPressed, ref bool currentPressed)
        {
            previousPressed = currentPressed;
            currentPressed = _keyboardState.IsPressed(key);
            return previousPressed && !currentPressed;
        }

        /// <summary>Fire key "event" if current state - down, previous - up.</summary>
        /// <param name="key">Key.</param>
        /// <param name="previousPressed">Previous state.</param>
        /// <param name="currentPressed">Current state.</param>
        /// <returns>Event firing flag.</returns>
        private bool ProcessKeyPressTriggerByKeyDown(Key key, ref bool previousPressed, ref bool currentPressed)
        {
            previousPressed = currentPressed;
            currentPressed = _keyboardState.IsPressed(key);
            return previousPressed && !currentPressed;
        }

        /// <summary>Read keys state and fill boolean fields.</summary>
        private void ProcessKeyboardState()
        {
            // Ctrl and shift is modifiers. Trigget dont needed.
            _controlPressed = _keyboardState.IsPressed(Key.LeftControl) || _keyboardState.IsPressed(Key.RightControl);
            _shiftPressed = _keyboardState.IsPressed(Key.LeftShift) || _keyboardState.IsPressed(Key.RightShift);

            // Esc by key up.
            _esc = ProcessKeyPressTriggerByKeyUp(Key.Escape, ref _escPreviousPressed, ref _escCurrentPressed);

            // Functional keys by key up.
            for (int i = 0; i <= 9; i++)
                _func[i] = ProcessKeyPressTriggerByKeyUp(_funcKeys[i], ref _funcPreviousPressed[i], ref _funcCurrentPressed[i]);
            PressedButtons = _keyboardState.PressedKeys;
            // For move keys we need current state.
            for(int index = 0; index < controlButtons.Keys.Count; index++)
            {
                Key key = controlButtons.Keys.ElementAt(index);
                controlButtons[key] = _keyboardState.IsPressed(key) ? true : false;
            }
            // Space (typicaly jump) - by key down.
            _space = ProcessKeyPressTriggerByKeyDown(Key.Space, ref _spacePreviousPressed, ref _spaceCurrentPressed);
        }

        /// <summary>Update keyboard state.</summary>
        public void UpdateKeyboardState()
        {
            // Try to acquire keyboard now if she is'nt acquired.
            if (!_keyboardAcquired) AcquireKeyboard();

            // Try to update keyboard state.
            ResultDescriptor resultCode = ResultCode.Ok;
            try
            {
                _keyboard.GetCurrentState(ref _keyboardState);
                // Success.
                ProcessKeyboardState();
                _keyboardUpdated = true;
            }
            catch (SharpDXException e)
            {
                resultCode = e.Descriptor;
                // Fail.
                _keyboardUpdated = false;
            }

            // In most cases error occured because of window lost input focus. Reset flag to try acquire in next time.
            if (resultCode == ResultCode.InputLost || resultCode == ResultCode.NotAcquired)
                _keyboardAcquired = false;
        }

        /// <summary>Read mouse coords and buttons state and fill fields.</summary>
        private void ProcessMouseState()
        {
            for (int i = 0; i <= 7; i++)
                _mouseButtons[i] = _mouseState.Buttons[i];
            _mouseRelativePositionX = _mouseState.X;
            _mouseRelativePositionY = _mouseState.Y;
            _mouseRelativePositionZ = _mouseState.Z;
        }

        /// <summary>Update mouse state.</summary>
        public void UpdateMouseState()
        {
            // Try to acquire mouse now if she is'nt acquired.
            if (!_mouseAcquired) AcquireMouse();

            // Try to update mouse state.
            ResultDescriptor resultCode = ResultCode.Ok;
            try
            {
                _mouse.GetCurrentState(ref _mouseState);
                // Success.
                ProcessMouseState();
                _mouseUpdated = true;
            }
            catch (SharpDXException e)
            {
                resultCode = e.Descriptor;
                // Fail.
                _mouseUpdated = false;
            }

            // In most cases error occured because of window lost input focus. Reset flag to try acquire in next time.
            if (resultCode == ResultCode.InputLost || resultCode == ResultCode.NotAcquired)
                _mouseAcquired = false;
        }

        /// <summary>Release all resources.</summary>
        public void Dispose()
        {
            _mouse.Unacquire();
            Utilities.Dispose(ref _mouse);
            _keyboard.Unacquire();
            Utilities.Dispose(ref _keyboard);
            Utilities.Dispose(ref _directInput);
        }

        public bool ContainsButton(Key key)
        {
            return controlButtons.ContainsKey(key);
        }

        public void AddButton(Key key)
        {
            controlButtons.Add(key, false);
        }
    }
}
