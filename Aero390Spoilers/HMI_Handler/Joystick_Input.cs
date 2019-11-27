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
        bool first_check = true, O_toggle = false;
        int options_delay = 0, L1_delay = 0, square_delay = 0, X_delay = 0, l2_delay = 0, r2_delay = 0;

        public JS_Input()
        {
            //Create buffer
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();
        }

        public double X_axis()
        {
            return Math.Round((joystick.GetCurrentState().X - 32767.0) / 32767.0, 2);
        }

        public double Y_axis()
        {
            return Math.Round((joystick.GetCurrentState().Y - 32767.0) / 32767.0, 2);
        }

        public int Throttle()
        {
            if (first_check && joystick.GetCurrentState().Z == 32767)
            {
                return 0;
            }
            else if (first_check && joystick.GetCurrentState().Z != 32767)
            {
                first_check = false;
            }

            return (int)((joystick.GetCurrentState().Z - 65535) / (-655.35));
        }

        public bool Triangle_button()
        {
            return joystick.GetCurrentState().Buttons[7];
        }

        public bool Square_button()
        {
            if (joystick.GetCurrentState().Buttons[4] && square_delay == 0)
            {
                square_delay++;
                return true;
            }

            if (square_delay > 0) square_delay++;
            if (square_delay >= 2) square_delay = 0;
            return false;
        }

        public bool X_button()
        {
            if (joystick.GetCurrentState().Buttons[5] && X_delay == 0)
            {
                X_delay++;
                return true;
            }

            if (X_delay > 0) X_delay++;
            if (X_delay >= 2) X_delay = 0;
            return false;
        }

        public bool O_button()
        {
            if(!O_toggle && joystick.GetCurrentState().Buttons[6])
            {
                O_toggle = true;
                return true;
            }
            else if(!joystick.GetCurrentState().Buttons[6])
            {
                O_toggle = false;
            }
            return false;
        }

        public bool Options_button()
        {
            if (joystick.GetCurrentState().Buttons[11] && options_delay == 0)
            {
                options_delay++;
                return true;
            }

            if (options_delay > 0) options_delay++;
            if (options_delay >= 30) options_delay = 0;
            return false;
        }

        public bool L1_button()
        {
            if (joystick.GetCurrentState().Buttons[1] && L1_delay == 0)
            {
                L1_delay++;
                return true;
            }

            if (L1_delay > 0) L1_delay++;
            if (L1_delay >= 50) L1_delay = 0;
            return false;
        }

        public bool R2_button()
        {
            if (joystick.GetCurrentState().Buttons[8] && r2_delay == 0)
            {
                r2_delay++;
                return true;
            }

            if (r2_delay > 0) r2_delay++;
            if (r2_delay >= 2) r2_delay = 0;
            return false;
        }

        public bool L2_button()
        {
            if (joystick.GetCurrentState().Buttons[9] && l2_delay == 0)
            {
                l2_delay++;
                return true;
            }

            if (l2_delay > 0) l2_delay++;
            if (l2_delay >= 2) l2_delay = 0;
            return false;
        }

        public bool Trigger_button()
        {
            return joystick.GetCurrentState().Buttons[0];
        }

        public int POV_Hat()
        {
            switch (joystick.GetCurrentState().PointOfViewControllers[0])
            {
                case 27000 :  return 0;
                case 31500 : return 1;
                case 0 : return 2;
                case 4500 : return 3;
                case 9000 : return 4;
                default :  return -1;
            }
        }

        public double Slider()
        {
            return Math.Round((joystick.GetCurrentState().Sliders[0] - 32767.0) / 32767.0, 2);
        }
    }
}
