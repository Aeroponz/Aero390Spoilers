using System;
using System.Threading.Tasks;

namespace LandingGear
{
    public class LandingGear
    {
        public LandingGear()
        {
            GearInTransit = false;
            GearDown = true;
            GearTransitTime = 10;
        }
        public async void GearLeverPositionChanged()
        {
            GearInTransit = true;
            GearDown = !GearDown;

            await Task.Delay((int)GearTransitTime * 1000);

            GearInTransit = false;
            GearDown = !GearDown;
        }

        public string GearStatus()
        {
            string oStatus = "";

            if (GearDown)
            {
                oStatus = "DOWN";
            }
            else
            {
                if (!GearInTransit)
                {
                    oStatus += "UP";
                }
                else
                {
                    oStatus = "IN TRANSIT";
                }
            }
            return oStatus;
        }

        public bool GearInTransit { get; set; }
        public bool GearDown { get; set; }
        public float GearTransitTime { get; set; }
    }
}
