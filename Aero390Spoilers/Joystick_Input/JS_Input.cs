using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using System.Threading; 

namespace Joystick_Input
{
    class JS_Input
    {
        static void Main()
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Flight,
                        DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("No joystick/Gamepad found.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            //Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            //// Query all suported ForceFeedback effects
            //var allEffects = joystick.GetEffects();
            //foreach (var effectInfo in allEffects)
            //    Console.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();



            double X_axis, Y_axis, Throttle;



            // Poll events from joystick
            while (true)
            {
                Console.Clear();
                joystick.Poll();
                X_axis = Math.Round((joystick.GetCurrentState().X - 32767.0) / 32767.0, 2);
                Y_axis = Math.Round((joystick.GetCurrentState().Y - 32767.0) / 32767.0, 2);
                Throttle = Math.Round(((joystick.GetCurrentState().Z*(-1) - 32767.0) / 32767.0)+2, 2);

                Console.WriteLine("X_axis: " + X_axis);
                Console.WriteLine("Y_axis: " + Y_axis);
                Console.WriteLine("Throttle: " + Throttle);
                Thread.Sleep(100);

                //foreach (var state in datas)
                //Console.WriteLine(state);
            }
        }

    }
}
