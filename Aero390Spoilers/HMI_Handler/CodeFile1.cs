using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SharpDX.DirectInput;



namespace HMI
{
    static class JS_Handler
    {

        static void Main()
        {
            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;
            var joystickState = new JoystickState();

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Flight,
                        DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("No joystick/Gamepad found.");
                Console.WriteLine(DeviceEnumerationFlags.AllDevices.ToString());
                Console.ReadKey();
                Environment.Exit(1);
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            double X_axis = 0;
            // Poll events from joystick
            while (true)
            {
                joystick.Poll();
                joystick.GetCurrentState(ref joystickState);
                //foreach (var state in datas)
                X_axis = ((double)joystickState.X - 32767.0)/32767.0;

                Console.WriteLine(Math.Round(X_axis,2));
            }

         }
        //public static int GetAxis(string XYZ, Joystick joystick)
        //{
        //    switch (XYZ)
        //    {
        //        case ("X"):
        //            {
        //                joystick.Poll();
        //                joystick.GetCurrentState(ref joystickState);
        //                Console.WriteLine(joystickState.X.ToString());
        //                break;
        //            }
        //        case ("Y"):
        //            {
        //                break;
        //            }
        //        case ("Z"):
        //            {
        //                break;
        //            }
        //    }

        //    return -999;
        //}
    }
}
