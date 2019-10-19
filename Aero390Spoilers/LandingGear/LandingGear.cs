using System;
using System.Collections.Generic;

namespace LandingGear
{
    public class LandingGear
    {
        public LandingGear()
        {
            GearInTransit = false;
            GearDown = true;
            GearTransitTime = 2;
        }
        public void GearLeverPositionChanged()
        {
            GearInTransit = true;
            GearDown = !GearDown;
            GearInTransit = false;
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
        public double GearTransitTime { get; set; }
    }
}
