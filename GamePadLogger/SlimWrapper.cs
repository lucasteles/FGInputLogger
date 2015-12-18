using SlimDX.DirectInput;
using SlimDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GamePadLogger
{
    class SlimWrapper
    {
        public static Joystick Pad;
        

        public static IList<GamepadDevice> Available()
        {

            IList<GamepadDevice> result = new List<GamepadDevice>();
            DirectInput dinput = new DirectInput();
            foreach (DeviceInstance di in dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
            {
                GamepadDevice dev = new GamepadDevice();
                dev.Guid = di.InstanceGuid;
                dev.Name = di.InstanceName;
                result.Add(dev);
            }
            return result;
        }


        public static void Acquire(System.Windows.Forms.Form parent, Guid Guid)
        {
            DirectInput dinput = new DirectInput();
            
            
            var pad = new Joystick(dinput, Guid);
            foreach (DeviceObjectInstance doi in pad.GetObjects(ObjectDeviceType.Axis))
            {
                pad.GetObjectPropertiesById((int)doi.ObjectType).SetRange(-5000, 5000);
            }

            pad.Properties.AxisMode = DeviceAxisMode.Absolute;
            pad.SetCooperativeLevel(parent, (CooperativeLevel.Nonexclusive | CooperativeLevel.Background));
            pad.Acquire();

            Pad =  pad;
        }

        public static PressedButtons GetInputs()
        {
            var state = new JoystickState();

            var buttons = new PressedButtons();

            if (Pad == null)
                return new PressedButtons();

            if (Pad.Poll().IsFailure)
            {

            }
                
            if (Pad.GetCurrentState(ref state).IsFailure)
            {

            }


            var _buttons = state.GetButtons().ToList();
            var pov = state.GetPointOfViewControllers()[0];

            _buttons.Add((state.X / 5000.0f == 1));
            _buttons.Add((state.X / 5000.0f  == -1));

            _buttons.Add((state.Y / 5000.0f == 1));
            _buttons.Add((state.Y / 5000.0f == -1));

            _buttons.Add((state.Z / 5000.0f >= 0.5));
            _buttons.Add((state.Z / 5000.0f <= -0.5));


            _buttons.Add(pov == 0 || pov == 31500 || pov == 4500);
            _buttons.Add(pov == 27000 || pov == 31500 || pov == 22500);
            _buttons.Add(pov == 9000 || pov == 4500 || pov == 13500);
            _buttons.Add(pov == 18000 || pov == 22500 || pov == 13500);


            for (int i = 0; i < _buttons.Count; i++)
                if (_buttons[i] )
                    buttons.buttons.Add(i);


            

            return buttons;
            


        }
    }

    internal class PressedButtons
    {
    
        public List<int> buttons = new List<int>();
        public PressedButtons()
        {
          
         }
    }
}
