using System;
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
            AltitudeASL = RunwayAltASL;
            PhaseOfFlight = "TAXI";
            BaroSettingmmHg = 29.92;
            GrossWeightLbs = 35000;
            VS = 0;
            InducedLift = 0;
            BankAngle = 0;
            AoA = 0;
            SpoilerLeverPosition = 0;
            FlapLeverPosition = 0;
            AutoBrakeSelectorPosition = 0;
            LThrottlePosition = 0;
            RThrottlePosition = 0;

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

            //Transition from wheels down to up (Takeoff) -> WoW should be false.
            if (GlobalGearStatus() == "DOWN")
            {
                WeightOnWheels = false;
            }
            LandingGears[0].GearLeverPositionChanged();
            LandingGears[1].GearLeverPositionChanged();
            LandingGears[2].GearLeverPositionChanged();

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

        #region EICAS

        public void AddEicasMessage(EICASMessage iMsg)
        {
            Stack<EICASMessage> TempMsgs = new Stack<EICASMessage>();
            //Check for higher priority message
            for (int i=0; i < EICASMessages.Count; i++)
            {
                if (EICASMessages.Peek().Importance > iMsg.Importance)
                {
                    TempMsgs.Push(EICASMessages.Pop());
                }
                else break;
            }
            //Push into position (top of the messages with the same priority)
            EICASMessages.Push(iMsg);
            //Push Higher Priority back on top
            for (int i = 0; i < TempMsgs.Count; i++)
            {
                EICASMessages.Push(TempMsgs.Pop());
            }
        }
        public void RemoveEicasMessage(EICASMessage iMsg)
        {
            Stack<EICASMessage> TempMsgs = new Stack<EICASMessage>();
            while(EICASMessages.Count>0)
            {
                if (EICASMessages.Peek().MessageText == iMsg.MessageText && EICASMessages.Peek().Importance == iMsg.Importance)
                {
                    EICASMessages.Pop();
                    break;
                }
                else TempMsgs.Push(EICASMessages.Pop());
            }
            //Push higher messages back into stack
            for (int i = 0; i < TempMsgs.Count; i++)
            {
                EICASMessages.Push(TempMsgs.Pop());
            }
        }

        #endregion EICAS

        #region Aircraft Systems Declarations
        //Aircraft Systems Declarations
        HydraulicSys[] HydSys;
        LandingGear.LandingGear[] LandingGears;
        #endregion

        #region Aircraft Specs Declarations
        public double MTOW = 40000.0;
        public double ZFW = 15000.0;
        public int NbofSpoilers = 8;
        public double SpoilerSBrakeDeflection = 0.5;
        public double SpoilerFlightDeflection = 0.2;
        #endregion

        #region Aircraft Parameters
        //AIRCRAFT PARAMETERS
        public bool WeightOnWheels { get; set; }
        public double GrossWeightLbs { get; set; }
        public double BaroSettingmmHg { get; set; }
        public double AltitudeASL { get; set; }
        public double IasKts { get; set; }
        public int IasOverspeed { get; set; }
        public int IasNES { get; set; }
        public int IasStall { get; set; }
        public double BankAngle { get; set; }
        public double AoA { get; set; }
        public double VS { get; set; }
        public int InducedLift { get; set; }
        public string PhaseOfFlight { get; set; }

        //COCKPIT CONTROLS
        public int SpoilerLeverPosition { get; set; }
        public int FlapLeverPosition { get; set; }
        public double SWControlWheelPosition { get; set; }
        public int AutoBrakeSelectorPosition { get; set; }
        public double LThrottlePosition { get; set; }
        public double RThrottlePosition { get; set; }

        //DISPLAYS
        public int[] SpoilerDeflectionPercentage = new int[8];
        public Stack<EICASMessage> EICASMessages = new Stack<EICASMessage>(); //Lines 1 to 8 for CW Messages, 9 to 11 for status

        //WARNING SYSTEM
        public bool WarningActive { get; set; }
        public bool CautionActive { get; set; }

        //MALFUNCTIONS AND SWITCHES
        public bool Switch1On { get; set; }
        public bool Switch2On { get; set; }
        public bool Switch3On { get; set; }
        public bool Switch4On { get; set; }
        public bool Switch5On { get; set; }
        public bool Switch6On { get; set; }
        public bool MalfPwrLoss { get; set; }
        public bool MalfFcc1 { get; set; }
        public bool MalfFcc2 { get; set; }
        public bool MalfHyd1 { get; set; }
        public bool MalfHyd2 { get; set; }
        public bool MalfSplrs { get; set; }

        //ARINC
        string[] wArincMessages = new string[1];
        #endregion

        #region Environmental Parameters

        public double RunwayAltASL = 118;

        #endregion
    }
}
