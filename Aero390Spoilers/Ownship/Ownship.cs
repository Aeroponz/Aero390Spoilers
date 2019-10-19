﻿using System;
using System.Collections.Generic;
using Hydraulics;
using LandingGear;
using ARINC;

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
            VS = 1000;
            BankAngle = 5;
            AoA = 10;
            SpoilerLeverPosition = 0;
            FlapLeverPosition = 0;

            for (int i = 0; i < wArincMessages.Length; i++)
            {
                wArincMessages[i] = "";
            }

            for (int i = 0; i < NbofSpoilers; i++)
            {
                SpoilerDeflectionPercentage[i] = 0;
            }

        }


        //Aircraft Tick and Returns Updated Aircraft Status
        public string[] AircraftTick()
        {
            //Landing Gear Update
            string GearUpdate = ArincGearStatus();
            if(GearUpdate != wArincMessages[0]) wArincMessages[0] = ArincGearStatus();


            return wArincMessages;  
        }

        #region Landing Gear

        public static IDictionary<string, string> A429GearStatus = new Dictionary<string, string>()
        {
            {"UP","0000000000000000001" },
            {"DOWN", "0000000000000000010" },
            {"IN TRANSIT", "0000000000000000011" },
            {"GEAR MISMATCH", "0000000000000000011" },
        };

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
                //Transition from wheels down to up (Takeoff) -> WoW should be false.
                if (GlobalGearStatus() == "DOWN")
                {
                    WeightOnWheels = false;
                }
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

        public string ArincGearStatus()
        {
            string LGStatusData = "";
            A429GearStatus.TryGetValue(GlobalGearStatus(), out LGStatusData);
            ARINCMessage NewMessage;
            if (GlobalGearStatus() != "GEAR MISMATCH")
            {
                NewMessage = new ARINCMessage("NO", LGStatusData, "FCC", "LandingGear_Status", true);
            }
            else
            {
                NewMessage = new ARINCMessage("FW", LGStatusData, "FCC", "LandingGear_Status", true);
            }
            return NewMessage.ToString();
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
        public int NbofSpoilers = 8;
        #endregion

        #region Aircraft Parameters

        double G32_RequiredPressure = 500;//psi
        bool G32_LowGearPressure = false;
        public bool WeightOnWheels { get; set; }
        public double GrossWeightLbs { get; set; }
        public double BaroSettingmmHg { get; set; }
        public double AltitudeASL { get; set; }
        public int IasKts { get; set; }
        public int IasOverspeed { get; set; }
        public int IasNES { get; set; }
        public int IasStall { get; set; }
        public int BankAngle { get; set; }
        public int AoA { get; set; }
        public int VS { get; set; }
        public int SpoilerLeverPosition { get; set; }
        public int FlapLeverPosition { get; set; }
        public int SWControlWheelPosition { get; set; }
        public int[] SpoilerDeflectionPercentage = new int[8];
        string[] wArincMessages = new string[1];
        #endregion
    }
}
