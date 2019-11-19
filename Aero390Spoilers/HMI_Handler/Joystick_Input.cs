using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using System.Threading;
using System.Media;

namespace Joystick_Input
{
    public class JS_Input
    {
        static DirectInput directInput = new DirectInput();
        static Guid joystickGuid = new Guid("5f939150-07ab-11ea-8001-444553540000");
        Joystick joystick = new Joystick(directInput, joystickGuid);
        bool first_check = true;
        int sound_delayer = 0;

        SoundPlayer ding = new SoundPlayer("..\\..\\Resources\\Misc\\acft_chime.wav");

        //static void Main()
        public JS_Input()
        {
           //Create buffer
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();
        }

        public double get_JS_X()
        {
            joystick.Poll();
            return Math.Round((joystick.GetCurrentState().X - 32767.0) / 32767.0, 2);
        }

        public double get_JS_Y()
        {
            joystick.Poll();
            return Math.Round((joystick.GetCurrentState().Y - 32767.0) / 32767.0, 2);
        }

        public int get_JS_Throttle()
        {
            if (first_check && joystick.GetCurrentState().Z == 32767)
            {
                return 0;
            }
            else if (first_check && joystick.GetCurrentState().Z != 32767)
            {
                first_check = false;
            }


            joystick.Poll();
            return (int)((joystick.GetCurrentState().Z - 65535) / (-655.35));
        }

        public bool JS_Triangle_button()
        {
            joystick.Poll();
            return joystick.GetCurrentState().Buttons[7];
        }

        public void JS_Options_button()
        {
            joystick.Poll();
            if (joystick.GetCurrentState().Buttons[11] && sound_delayer == 0) ding.Play();

            sound_delayer++;
            if (sound_delayer >= 5) sound_delayer = 0;
        }

        //static void Main()
        //{
        //    JS_Input test = new JS_Input();
        //    while (true)
        //    {
        //        Console.Clear();
        //        Console.WriteLine("X_axis: " + test.get_JS_X());
        //        Console.WriteLine("Y_axis: " + test.get_JS_Y());
        //        Console.WriteLine("Z_axis: " + test.get_JS_Throttle());
        //        Thread.Sleep(50);
        //    }
        //}
    }
}
