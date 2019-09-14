using System;
using Hydraulics;
using LandingGear;

namespace Ownship
{
    public class Aircraft
    {
        public Aircraft()
        {

            HydSys = new HydraulicSys[2];
            for (int i = 0; i < 2; i++)
            {
                HydSys[i] = new HydraulicSys();
            }

            LandingGears = new LandingGear.LandingGear[3];
            for (int i = 0; i < 3; i++)
            {
                LandingGears[i] = new LandingGear.LandingGear();
            }

        }
        public void GearPositionChange()
        {
            if(HydSys[0].GetAvailPress() < G32_RequiredPressure)
            {
                G32_LowGearPressure = true;
                return;
            }
            else
            {
                LandingGears[0].GearLeverPositionChanged();
                LandingGears[1].GearLeverPositionChanged();
                LandingGears[2].GearLeverPositionChanged();
            }
        }

        public string GlobalGearStatus()
        {
            int Sum = 0;

            for(int i = 0; i < 3; i++)
            {
                if (LandingGears[i].GearStatus() == "IN TRANSIT")
                {
                    return "IN TRANSIT";
                }
                if (LandingGears[i].GearStatus() == "DOWN") Sum += 5;
                else Sum += 1;
            }

            switch (Sum)
            {
                case 3:
                    return "UP";
                case 15:
                    return "DOWN";
                default :
                    return "GEAR MISMATCH";
            }
            
        }

        //Aircraft Systems
        HydraulicSys[] HydSys;
        LandingGear.LandingGear[] LandingGears;

        //Aircraft Parameters
        float G32_RequiredPressure = 500;//psi
        bool G32_LowGearPressure = false;
    }
}
