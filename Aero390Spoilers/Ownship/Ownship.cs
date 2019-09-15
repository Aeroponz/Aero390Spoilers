using System;
using Hydraulics;
using LandingGear;

namespace Ownship
{
    public class Aircraft
    {
        //Constructor
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

            //Parameter Initialisation
            WeightOnWheels = true;
            IasKts = 0;
            IasNES = 350;
            IasOverspeed = 320;
            IasStall = 100;
            AltitudeASL = 118;
            BaroSettingmmHg = 29.92;
            GrossWeightLbs = 35000;

        }

        #region Landing Gear
        //Landing Gear Lever Position Change
        public void GearPositionChange()
        {
            if (HydSys[0].GetAvailPress() < G32_RequiredPressure)
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

        //Returns a common gear status calculated from the individual gear status
        public string GlobalGearStatus()
        {
            int Sum = 0;

            for (int i = 0; i < 3; i++)
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
                default:
                    return "GEAR MISMATCH";
            }

        }
        #endregion

        #region Aircraft Systems Declarations
        //Aircraft Systems Declarations
        HydraulicSys[] HydSys;
        LandingGear.LandingGear[] LandingGears;
        #endregion

        #region Aircraft Specs Declarations
        public double MTOW = 40000.0;
        public double ZFW = 15000.0;
        #endregion

        #region Aircraft Parameters

        double G32_RequiredPressure = 500;//psi
        bool G32_LowGearPressure = false;
        public bool WeightOnWheels { get; set; }
        public double GrossWeightLbs { get; set; }
        public double BaroSettingmmHg { get; set; }
        public double AltitudeASL { get; set; }
        public double IasKts { get; set; }
        public double IasOverspeed { get; set; }
        public double IasNES { get; set; }
        public double IasStall { get; set; }
        #endregion
    }
}
