﻿using Aero390Spoilers.Properties;
using Ownship;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Media;
using System.IO;

namespace Aero390Spoilers
{

    public partial class AircraftGUI : Form
    {

        Ownship.Aircraft GUIOwnship = new Ownship.Aircraft();
        bool SpoilerThreadRunning = false;
        int AltCalloutTimeout = 0;
        //Constructor
        public AircraftGUI()
        {
            InitializeComponent();
            AircraftGUI_Tick();
        }

        #region GUI Tick
        //This function will be called every 0.5 seconds and will point to GUI_TickJobs(), where you can find the functions that will run every tick.
        public void AircraftGUI_Tick()
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = (100); // 0.1 secs
            timer.Tick += new EventHandler(GUI_TickJobs);
            timer.Start();
        }
        //To add an operation to be executed every tick, and the function in GUI_TickJobs()
        private void GUI_TickJobs(object sender, EventArgs e)
        {
            ReadCockpitControls();
            RefreshAutoBrakeSelector();
            RefreshCockpitInstruments();
            RefreshEICAS();
            RefreshFCSynoptic();
            RefreshGearPict();
            RefreshMasterLights();
            RefreshPrintOuts();
            UpdatePhaseOfFlight();
            if (GUIOwnship.PhaseOfFlight == "APPROACH") AltitudeCallouts(GUIOwnship.AltitudeASL-GUIOwnship.RunwayAltASL);
        }

        private void AltitudeCallouts(double ACRadioAltitude)
        {
            
            string wAlt = "";
            if (AltCalloutTimeout == 10) AltCalloutTimeout = 0;
            if (AltCalloutTimeout == 0)
            {
                if (ACRadioAltitude <= 11 && ACRadioAltitude > 9) wAlt = "10";
                else if (ACRadioAltitude <= 21 && ACRadioAltitude > 19) wAlt = "20";
                else if (ACRadioAltitude <= 31 && ACRadioAltitude > 29) wAlt = "30";
                else if (ACRadioAltitude <= 41 && ACRadioAltitude > 39) wAlt = "40";
                else if (ACRadioAltitude <= 51 && ACRadioAltitude > 49) wAlt = "50";
                else if (ACRadioAltitude <= 101 && ACRadioAltitude > 99) wAlt = "100";
                else if (ACRadioAltitude <= 116 && ACRadioAltitude > 114) wAlt = "Mins";
                else if (ACRadioAltitude <= 201 && ACRadioAltitude > 199) wAlt = "200";
                else if (ACRadioAltitude <= 216 && ACRadioAltitude > 214) wAlt = "AppMins";
                else if (ACRadioAltitude <= 301 && ACRadioAltitude > 299) wAlt = "300";
                else if (ACRadioAltitude <= 401 && ACRadioAltitude > 399) wAlt = "400";
                else if (ACRadioAltitude <= 501 && ACRadioAltitude > 499) wAlt = "500";
                else if (ACRadioAltitude <= 1001 && ACRadioAltitude > 999) wAlt = "1000";
                else if (ACRadioAltitude <= 2501 && ACRadioAltitude > 2499) wAlt = "2500";
            }
            else
            {
                AltCalloutTimeout++;
            }

            if (wAlt != "")
            {
                SoundPlayer simpleSound = new SoundPlayer("..\\..\\Resources\\AltitudeCallouts\\Boeing_" + wAlt + ".wav");
                simpleSound.Play();
                AltCalloutTimeout++;
            }
        }
        private void ReadCockpitControls()
        {
            //SPOILER LEVER REFRESH
            int temp = GUIOwnship.SpoilerLeverPosition;
            GUIOwnship.SpoilerLeverPosition = -1 * SpoilerLever.Value;
            if (temp != GUIOwnship.SpoilerLeverPosition)//Spoiler Lever Position has changed.
            {
                //TEMP WORK-AROUND
                if (!SpoilerThreadRunning)
                {
                    if (GUIOwnship.SpoilerLeverPosition == -1)
                    {
                        SpoilerLever.Value = 0;
                        GUIOwnship.SpoilerLeverPosition = 0;
                    }
                    double DeflectionPercent = ((double)(GUIOwnship.SpoilerLeverPosition) / 10.0) * 100;
                    double FromDeflection = GUIOwnship.SpoilerDeflectionPercentage[0];
                    if (GUIOwnship.SpoilerLeverPosition <= 0) DeflectionPercent = 0;
                    if (temp <= 0) FromDeflection = 0;
                    Thread IncrementSpoilers = new Thread(() => RefreshSpoilerActuation((int)FromDeflection, (int)DeflectionPercent, !GUIOwnship.WeightOnWheels));
                    SpoilerThreadRunning = true;
                    IncrementSpoilers.Start();
                }
            }

            //FLAP LEVER REFRESH
            GUIOwnship.FlapLeverPosition = -1 * FlapLever.Value;

            //CONTROL WHEEL REFRESH
            GUIOwnship.SWControlWheelPosition = ControlWheelBar.Value;
            GUIOwnship.BankAngle = GUIOwnship.SWControlWheelPosition * 3;

            //THROTTLES
            GUIOwnship.LThrottlePosition = LENGThrottle.Value;
            GUIOwnship.RThrottlePosition = RENGThrottle.Value;
        }
        private void ReadDataPipe(string PipeName)
        {

        }
        private void RefreshAutoBrakeSelector()
        {
            switch(GUIOwnship.AutoBrakeSelectorPosition)
            {
                case 0: ABSelectorPB.BackgroundImage = Resources.SelectorT_270deg; break;
                case 1: ABSelectorPB.BackgroundImage = Resources.SelectorT_315deg; break;
                case 2: ABSelectorPB.BackgroundImage = Resources.SelectorT_0deg; break;
                case 3: ABSelectorPB.BackgroundImage = Resources.SelectorT_45deg; break;
                case 4: ABSelectorPB.BackgroundImage = Resources.SelectorT_90deg; break;
            }
        }
        private void RefreshCockpitInstruments()
        {
            airSpeedIndicatorInstrumentControl1.SetAirSpeedIndicatorParameters(GUIOwnship.IasKts);
            attitudeIndicatorInstrumentControl1.SetAttitudeIndicatorParameters(GUIOwnship.AoA, GUIOwnship.BankAngle);
            altimeterInstrumentControl1.SetAlimeterParameters((int)GUIOwnship.AltitudeASL);
            verticalSpeedIndicatorInstrumentControl1.SetVerticalSpeedIndicatorParameters((int)GUIOwnship.VS);
            EIEngine1Control.SetEngineIndicatorParameters(LENGThrottle.Value * 10);
            EIEngine2Control.SetEngineIndicatorParameters(RENGThrottle.Value * 10);
        }
        private void RefreshEICAS()
        {
            RefreshEICASAllMessages();
            RefreshEICASSystemStatus();
        }
        private void RefreshEICASMessage(System.Windows.Forms.TextBox iTextbox, Ownship.EICASMessage iMsg)
        {
            iTextbox.Text = iMsg.MessageText;
            switch(iMsg.Importance)
            {
                case 0: iTextbox.ForeColor = Color.White; break;
                case 1: iTextbox.ForeColor = Color.Orange; break;
                case 2: iTextbox.ForeColor = Color.Red; break;
            }
        }
        private void RefreshEICASAllMessages()
        {
            RefreshEICASMessage(EicasMsgLine1, GUIOwnship.EICASMessages[0]);
            RefreshEICASMessage(EicasMsgLine2, GUIOwnship.EICASMessages[1]);
            RefreshEICASMessage(EicasMsgLine3, GUIOwnship.EICASMessages[2]);
            RefreshEICASMessage(EicasMsgLine4, GUIOwnship.EICASMessages[3]);
            RefreshEICASMessage(EicasMsgLine5, GUIOwnship.EICASMessages[4]);
            RefreshEICASMessage(EicasMsgLine6, GUIOwnship.EICASMessages[5]);
            RefreshEICASMessage(EicasMsgLine7, GUIOwnship.EICASMessages[6]);
            RefreshEICASMessage(EicasMsgLine8, GUIOwnship.EICASMessages[7]);
            RefreshEICASMessage(EicasMsgLine9, GUIOwnship.EICASMessages[8]);
            RefreshEICASMessage(EicasMsgLine10, GUIOwnship.EICASMessages[9]);
            RefreshEICASMessage(EicasMsgLine11, GUIOwnship.EICASMessages[10]);
        }
        private void RefreshEICASSystemStatus()
        {
            //LDG GEAR
            switch (GUIOwnship.GlobalGearStatus())
            {
                case ("DOWN"):
                    {
                        EicasGearMessage.Text = "LDG GEAR DN";
                        EicasGearMessage.ForeColor = Color.Lime;
                        break;
                    }
                case ("UP"):
                    {
                        EicasGearMessage.Text = "LDG GEAR UP";
                        EicasGearMessage.ForeColor = Color.Lime;
                        break;
                    }
                case ("IN TRANSIT"):
                    {
                        EicasGearMessage.Text = "LDG GEAR TR";
                        EicasGearMessage.ForeColor = Color.Lime;
                        break;
                    }
                case ("UNKNOWN"):
                    {
                        EicasGearMessage.Text = "LDG GEAR UNKNOWN";
                        EicasGearMessage.ForeColor = Color.Orange;
                        break;
                    }
            }

            //FLAPS
            string FlapPos = "";
            if (GUIOwnship.FlapLeverPosition == 0)
            {
                FlapPos = "UP";
            }
            else
            {
                FlapPos = Convert.ToString(GUIOwnship.FlapLeverPosition*10);
            }
            EicasFlapsMessage.Text = "FLAPS..........." + FlapPos;

            //SPOILERS
            if (SpoilerLever.Value >= 0) EicasSpoilerMessage.Text = "";
            else EicasSpoilerMessage.Text = "SPOILERS";


            //AUTOBRAKES
            switch(GUIOwnship.AutoBrakeSelectorPosition)
            {
                case 0: EICASAutoBrakesMessage.Text = "AUTOBRAKES OFF"; break;
                case 1: EICASAutoBrakesMessage.Text = "AUTOBRAKES RTO"; break;
                case 2: EICASAutoBrakesMessage.Text = "AUTOBRAKES 1"; break;
                case 3: EICASAutoBrakesMessage.Text = "AUTOBRAKES 2"; break;
                case 4: EICASAutoBrakesMessage.Text = "AUTOBRAKES MAX"; break;
            }

        }
        private void RefreshFCSynoptic()
        {
            Spoiler1PGB.Value = 100 - GUIOwnship.SpoilerDeflectionPercentage[0];
            Spoiler1PGB.Refresh();
            Spoiler2PGB.Value = 100 - GUIOwnship.SpoilerDeflectionPercentage[1];
            Spoiler2PGB.Refresh();
            Spoiler3PGB.Value = 100 - GUIOwnship.SpoilerDeflectionPercentage[2];
            Spoiler3PGB.Refresh();
            Spoiler4PGB.Value = 100 - GUIOwnship.SpoilerDeflectionPercentage[3];
            Spoiler4PGB.Refresh();
            Spoiler5PGB.Value = 100 - GUIOwnship.SpoilerDeflectionPercentage[4];
            Spoiler5PGB.Refresh();
            Spoiler6PGB.Value = 100 - GUIOwnship.SpoilerDeflectionPercentage[5];
            Spoiler6PGB.Refresh();
            Spoiler7PGB.Value = 100 - GUIOwnship.SpoilerDeflectionPercentage[6];
            Spoiler7PGB.Refresh();
            Spoiler8PGB.Value = 100 - GUIOwnship.SpoilerDeflectionPercentage[7];
            Spoiler8PGB.Refresh();
        }
        private void RefreshGearPict()
        {

            //AIRCRAFT SCHEMATIC AND GEAR ICON
            if (GUIOwnship.GlobalGearStatus() == "UP")
            {
                GearStatusIconPB.Image = Resources.LDG_UP;
            }
            else if (GUIOwnship.GlobalGearStatus() == "DOWN")
            {
                GearStatusIconPB.Image = Resources.LDG_Down;
            }
            else if (GUIOwnship.GlobalGearStatus() == "IN TRANSIT")
            {
                GearStatusIconPB.Image = Resources.LgIcon_Transit;
            }
            else
            {
                GearStatusIconPB.Image = Resources.LgIcon_Unknown;
            }

            //WEIGHT ON WHEELS ICON
            if (GUIOwnship.WeightOnWheels == true)
            {
                WoWPBLight.Image = Resources.WoWLightOn;
            }
            else
            {
                WoWPBLight.Image = Resources.WoWLightOff;
            }

        }
        private void RefreshMasterLights()
        {
            if (GUIOwnship.WarningActive)
            {
                if (GUIOwnship.CautionActive) MWMCPB.BackgroundImage = Resources.MWMC_11;
                else MWMCPB.BackgroundImage = Resources.MWMC_10; ;
            }
            else
            {
                if (GUIOwnship.CautionActive) MWMCPB.BackgroundImage = Resources.MWMC_01;
                else MWMCPB.BackgroundImage = Resources.MWMC_00; ;
            }
        }
        private void RefreshPrintOuts()
        {
            GwPrintOut.Text = GUIOwnship.GrossWeightLbs.ToString();
            BaroPrintOut.Text = GUIOwnship.BaroSettingmmHg.ToString();
            AltPrintOut.Text = ((int)GUIOwnship.AltitudeASL).ToString();
            IASPrintOut.Text = ((int)GUIOwnship.IasKts).ToString();
            PhaseOfFlightTB.Text = GUIOwnship.PhaseOfFlight;
        }
        private void RefreshSpoilerActuation(int CurrentDeflection, int TargetDeflection,  bool InFlight = true, bool SymDeploy = true)
        {
            if (InFlight)
            {
                if (SymDeploy)
                {
                    TargetDeflection = (int)((double)TargetDeflection * GUIOwnship.SpoilerSBrakeDeflection);
                    CurrentDeflection = (int)((double)CurrentDeflection * GUIOwnship.SpoilerSBrakeDeflection);
                }
                else
                {
                    TargetDeflection = (int)((double)TargetDeflection * GUIOwnship.SpoilerFlightDeflection);
                    CurrentDeflection = (int)((double)CurrentDeflection * GUIOwnship.SpoilerFlightDeflection);
                }
            }
            
            while (CurrentDeflection != TargetDeflection)
            {
                int increment = TargetDeflection >= CurrentDeflection ? 1 : -1;
                if (SymDeploy)
                {
                    for(int j = 0; j < GUIOwnship.NbofSpoilers; j++)
                    {
                        GUIOwnship.SpoilerDeflectionPercentage[j] += increment;
                    }
                }
                else
                {
                    if (GUIOwnship.BankAngle > 0)
                    {
                        for (int j = 4; j < GUIOwnship.NbofSpoilers; j++)
                        {
                            GUIOwnship.SpoilerDeflectionPercentage[j] += increment;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < (GUIOwnship.NbofSpoilers / 2); j++)
                        {
                            GUIOwnship.SpoilerDeflectionPercentage[j] += increment;
                        }
                    }

                }
                CurrentDeflection += increment;
                Thread.Sleep(30);
            }
            SpoilerThreadRunning = false;
            return;
        }
        private void RepositionTo(string Reposition)
        {
            switch (Reposition)
            {
                case ("Takeoff"):
                    {
                        GUIOwnship.AltitudeASL = GUIOwnship.RunwayAltASL;
                        GUIOwnship.AoA = 0;
                        GUIOwnship.AutoBrakeSelectorPosition = 0;
                        GUIOwnship.BankAngle = 0;
                        GUIOwnship.BaroSettingmmHg = 29.92;
                        FlapLever.Value = 0;
                        GUIOwnship.FlapLeverPosition = 0;
                        GUIOwnship.GrossWeightLbs = 35000;
                        LENGThrottle.Value = 0;
                        RENGThrottle.Value = 0;
                        GUIOwnship.LThrottlePosition = 0;
                        GUIOwnship.RThrottlePosition = 0;
                        SpoilerLever.Value = 2;
                        GUIOwnship.SpoilerLeverPosition = 2;
                        ControlWheelBar.Value = 0;
                        GUIOwnship.SWControlWheelPosition = 0;
                        GUIOwnship.VS = 0;
                        GUIOwnship.IasKts = 0;
                        if (GUIOwnship.GlobalGearStatus() != "DOWN") GUIOwnship.GearPositionChange();
                        GUIOwnship.WeightOnWheels = true;
                        GUIOwnship.PhaseOfFlight = "TAXI";
                        break;
                    }
                case ("InAir"):
                    {
                        GUIOwnship.AltitudeASL = 10000;
                        GUIOwnship.AoA = 1;
                        GUIOwnship.AutoBrakeSelectorPosition = 0;
                        GUIOwnship.BankAngle = 0;
                        GUIOwnship.BaroSettingmmHg = 29.92;
                        FlapLever.Value = 0;
                        GUIOwnship.FlapLeverPosition = 0;
                        GUIOwnship.GrossWeightLbs = 30000;
                        LENGThrottle.Value = 8;
                        RENGThrottle.Value = 8;
                        GUIOwnship.LThrottlePosition = 8;
                        GUIOwnship.RThrottlePosition = 8;
                        SpoilerLever.Value = 2;
                        GUIOwnship.SpoilerLeverPosition = 2;
                        ControlWheelBar.Value = 0;
                        GUIOwnship.SWControlWheelPosition = 0;
                        GUIOwnship.VS = 0;
                        GUIOwnship.IasKts = 250;
                        if (GUIOwnship.GlobalGearStatus() != "UP") GUIOwnship.GearPositionChange();
                        GUIOwnship.WeightOnWheels = false;
                        GUIOwnship.PhaseOfFlight = "CRUISE";
                        break;
                    }
                case ("Approach"):
                    {
                        GUIOwnship.AltitudeASL = 1073 + GUIOwnship.RunwayAltASL;
                        GUIOwnship.AoA = -3;
                        GUIOwnship.AutoBrakeSelectorPosition = 3;
                        GUIOwnship.BankAngle = 0;
                        GUIOwnship.BaroSettingmmHg = 29.92;
                        FlapLever.Value = -3;
                        GUIOwnship.FlapLeverPosition = -3;
                        GUIOwnship.GrossWeightLbs = 28500;
                        LENGThrottle.Value = 4;
                        RENGThrottle.Value = 4;
                        GUIOwnship.LThrottlePosition = 4;
                        GUIOwnship.RThrottlePosition = 4;
                        SpoilerLever.Value = 0;
                        GUIOwnship.SpoilerLeverPosition = 0;
                        ControlWheelBar.Value = 0;
                        GUIOwnship.SWControlWheelPosition = 0;
                        GUIOwnship.VS = -600;
                        GUIOwnship.IasKts = 154;
                        if (GUIOwnship.GlobalGearStatus() != "DOWN") GUIOwnship.GearPositionChange();
                        GUIOwnship.WeightOnWheels = false;
                        GUIOwnship.PhaseOfFlight = "APPROACH";
                        Thread ApproachScenario = new Thread(() => RADALTStub());
                        ApproachScenario.Start();
                        break;
                    
}
            }
        }
        private void RADALTStub()
        {
            while (GUIOwnship.AltitudeASL - GUIOwnship.RunwayAltASL > 0)
            {
                if (GUIOwnship.AltitudeASL - GUIOwnship.RunwayAltASL >= 30)//-600fpm == 5ft/0.5sec, AOA -3
                {
                    GUIOwnship.AltitudeASL -= 1;
                }
                else //-150fpm, AOA +3
                {
                    GUIOwnship.AltitudeASL -= 0.50;
                    GUIOwnship.AoA += 0.1;
                    GUIOwnship.VS += 7.5;
                    if (GUIOwnship.AltitudeASL < GUIOwnship.RunwayAltASL) GUIOwnship.AltitudeASL = GUIOwnship.RunwayAltASL;
                }
                Thread.Sleep(100);
            }
            GUIOwnship.VS = 0;
            GUIOwnship.WeightOnWheels = true;
            while (GUIOwnship.IasKts > 0)
            {
                if(GUIOwnship.AoA > 0) GUIOwnship.AoA -= 0.05;
                GUIOwnship.IasKts -= 1;
                if (GUIOwnship.IasKts < 0) GUIOwnship.IasKts = 0;
                Thread.Sleep(100);
            }
            return;
        }
        private void UpdatePhaseOfFlight()
        {
            switch(GUIOwnship.PhaseOfFlight)
            {
                case ("TAXI"):
                    {
                        if (GUIOwnship.LThrottlePosition > 5 && GUIOwnship.RThrottlePosition > 5) GUIOwnship.PhaseOfFlight = "TAKEOFF";
                        break;
                    }
                case ("TAKEOFF"):
                    {
                        if(GUIOwnship.LThrottlePosition < 9 && GUIOwnship.RThrottlePosition < 9) GUIOwnship.PhaseOfFlight = "RTO";
                        else if (GUIOwnship.GlobalGearStatus() == "UP") GUIOwnship.PhaseOfFlight = "CLIMB";
                        break;
                    }
                case ("CLIMB"):
                    {
                        if (GUIOwnship.AoA < 1.25 && GUIOwnship.AoA >= 0) GUIOwnship.PhaseOfFlight = "CRUISE";
                        break;
                    }
                case ("CRUISE"):
                    {
                        if (GUIOwnship.GlobalGearStatus() == "DOWN") GUIOwnship.PhaseOfFlight = "APPROACH";
                        break;
                    }
                case ("APPROACH"):
                    {
                        if (GUIOwnship.WeightOnWheels) GUIOwnship.PhaseOfFlight = "LANDING";
                        break;
                    }
                case ("LANDING"):
                    {
                        if (GUIOwnship.IasKts <= 50) GUIOwnship.PhaseOfFlight = "TAXI";
                        break;
                    }
            }
        }


        #endregion

        #region GUI Elements
        private void WowButton_Click(object sender, EventArgs e)
        {
            GUIOwnship.WeightOnWheels = !(GUIOwnship.WeightOnWheels);
        }
        private void GWButton_Click(object sender, EventArgs e)
        {
            if(IntegerInput.Text == "Enter Value Here")
            {
                //Do Nothing
            }
            else
            {
                double Input_double = double.Parse(IntegerInput.Text, System.Globalization.CultureInfo.InvariantCulture);
                if (Input_double <= GUIOwnship.MTOW && Input_double >= GUIOwnship.ZFW)
                {
                    GUIOwnship.GrossWeightLbs = Input_double;
                    IntegerInput.Text = "Enter Value Here";
                }
                else
                {
                    IntegerInput.Text = "Invalid Input : Out of Range";
                }
                
            }
        }
        private void Barometer_Click(object sender, EventArgs e)
        {
            if (IntegerInput.Text == "Enter Value Here")
            {
                //Do Nothing
            }
            else
            {
                double Input_double = double.Parse(IntegerInput.Text, System.Globalization.CultureInfo.InvariantCulture);
                if (Input_double <= 30.20 && Input_double >= 29.80)
                {
                    GUIOwnship.BaroSettingmmHg = Input_double;
                    IntegerInput.Text = "Enter Value Here";
                }
                else
                {
                    IntegerInput.Text = "Invalid Input : Out of Range";
                }

            }
        }
        private void Altitude_Click(object sender, EventArgs e)
        {
            if (IntegerInput.Text == "Enter Value Here")
            {
                //Do Nothing
            }
            else
            {
                double Input_double = double.Parse(IntegerInput.Text, System.Globalization.CultureInfo.InvariantCulture);
                if (Input_double <= 45000 && Input_double >= 0)
                {
                    GUIOwnship.AltitudeASL = Input_double;
                    IntegerInput.Text = "Enter Value Here";
                }
                else
                {
                    IntegerInput.Text = "Invalid Input : Out of Range";
                }

            }
        }
        private void IASButton_Click(object sender, EventArgs e)
        {
            if (IntegerInput.Text == "Enter Value Here")
            {
                //Do Nothing
            }
            else
            {
                int Input_int = int.Parse(IntegerInput.Text, System.Globalization.CultureInfo.InvariantCulture);
                if (Input_int <= GUIOwnship.IasNES && Input_int >= 0)
                {
                    GUIOwnship.IasKts = Input_int;
                    IntegerInput.Text = "Enter Value Here";
                }
                else
                {
                    IntegerInput.Text = "Invalid Input : Out of Range";
                }

            }
        }
        private void GearStatusIconPB_Click(object sender, EventArgs e)
        {
            GUIOwnship.GearPositionChange();
        }
        public void ABOffTB_Click(object sender, EventArgs e)
        {
            GUIOwnship.AutoBrakeSelectorPosition = 0;
        }
        public void ABRtoTB_Click(object sender, EventArgs e)
        {
            GUIOwnship.AutoBrakeSelectorPosition = 1;
        }
        public void AB1TB_Click(object sender, EventArgs e)
        {
            GUIOwnship.AutoBrakeSelectorPosition = 2;
        }
        public void AB2TB_Click(object sender, EventArgs e)
        {
            GUIOwnship.AutoBrakeSelectorPosition = 3;
        }
        public void ABMaxTB_Click(object sender, EventArgs e)
        {
            GUIOwnship.AutoBrakeSelectorPosition = 4;
        }
        private void MWMCPB_Click(object sender, EventArgs e)
        {
            GUIOwnship.CautionActive = false;
            GUIOwnship.WarningActive = false;
        }
        private void SW1PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch1On = !GUIOwnship.Switch1On;
            if (GUIOwnship.Switch1On) SW1PB.BackgroundImage = Resources.Switch_ON;
            else SW1PB.BackgroundImage = Resources.Switch_OFF;
        }
        private void SW2PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch2On = !GUIOwnship.Switch2On;
            if (GUIOwnship.Switch2On) SW2PB.BackgroundImage = Resources.Switch_ON;
            else SW2PB.BackgroundImage = Resources.Switch_OFF;
        }
        private void SW3PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch3On = !GUIOwnship.Switch3On;
            if (GUIOwnship.Switch3On) SW3PB.BackgroundImage = Resources.Switch_ON;
            else SW3PB.BackgroundImage = Resources.Switch_OFF;
        }
        private void SW4PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch4On = !GUIOwnship.Switch4On;
            if (GUIOwnship.Switch4On) SW4PB.BackgroundImage = Resources.Switch_ON;
            else SW4PB.BackgroundImage = Resources.Switch_OFF;
        }
        private void SW5PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch5On = !GUIOwnship.Switch5On;
            if (GUIOwnship.Switch5On) SW5PB.BackgroundImage = Resources.Switch_ON;
            else SW5PB.BackgroundImage = Resources.Switch_OFF;
        }
        private void SW6PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch6On = !GUIOwnship.Switch6On;
            if (GUIOwnship.Switch6On) SW6PB.BackgroundImage = Resources.Switch_ON;
            else SW6PB.BackgroundImage = Resources.Switch_OFF;
        }
        #endregion

        private void TORepoButton_Click(object sender, EventArgs e)
        {
            RepositionTo("Takeoff");
        }

        private void InAirRepoButton_Click(object sender, EventArgs e)
        {
            RepositionTo("InAir");
        }

        private void AppRepoButton_Click(object sender, EventArgs e)
        {
            RepositionTo("Approach");
        }
    }
}
