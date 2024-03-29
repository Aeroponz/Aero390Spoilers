﻿using Aero390Spoilers.Properties;
using Joystick_Input;
using Ownship;
using System;
using System.Drawing;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aero390Spoilers
{

    public partial class AircraftGUI : Form
    {

        JS_Input HOTAS = new JS_Input();

        Ownship.Aircraft GUIOwnship = new Ownship.Aircraft();
        bool underspeed_warning, underspeed_shown, avionics_start, armed_trigger, config_warning, config_wrng_shown;
        int AltCalloutTimeout = 0, speed_alert = 0;
        int splrmismatchactive = 0;
        SoundPlayer WarningSound = new SoundPlayer("..\\..\\Resources\\AltitudeCallouts\\Boeing_MC_Single.wav");
        SoundPlayer ding = new SoundPlayer("..\\..\\Resources\\Misc\\acft_chime.wav");
        SoundPlayer missile = new SoundPlayer("..\\..\\Resources\\Misc\\missile_fox.wav");
        SoundPlayer cannon = new SoundPlayer("..\\..\\Resources\\Misc\\cannon.wav");
        SoundPlayer underspeed = new SoundPlayer("..\\..\\Resources\\Misc\\speed_alert.wav");
        SoundPlayer avionics = new SoundPlayer("..\\..\\Resources\\Misc\\avionics_switch.wav");

        //Constructor
        public AircraftGUI()
        {
            InitializeComponent();
            AircraftGUI_Tick();
            HideMalfunctionComponents();
            PowerLossMalfunction(true);
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
            RefreshAttitude();
            SpoilerMismatchMalf();
            if (GUIOwnship.PhaseOfFlight == "APPROACH")
            {
                AltitudeCallouts(GUIOwnship.AltitudeASL - GUIOwnship.RunwayAltASL);
                AltitudeCheck();
            }
        }

        private void AltitudeCheck()
        {
            if (GUIOwnship.AltitudeASL <= GUIOwnship.RunwayAltASL + 0.49 && GUIOwnship.AltitudeASL >= GUIOwnship.RunwayAltASL - 0.49)
            {
                GUIOwnship.WeightOnWheels = true;
                GUIOwnship.VS = 0;
            }
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
        private void FccFault(bool Fcc1Fault, bool Fcc2Fault, int UpdateSide)
        {
            if (Fcc1Fault && Fcc2Fault)
            {
                EICASMessage SplrFail = new EICASMessage();
                SplrFail.Importance = 2;
                SplrFail.MessageText = "SPOILERS";
                GUIOwnship.AddEicasMessage(SplrFail);
                GUIOwnship.WarningActive = true;

                WarningSound.PlayLooping();

                //Loss of 1,3,6,8
                SplrLoss1.Show();
                SplrLoss3.Show();
                SplrLoss6.Show();
                SplrLoss8.Show();

                //Loss of 2,4,5,7
                SplrLoss2.Show();
                SplrLoss4.Show();
                SplrLoss5.Show();
                SplrLoss7.Show();
            }
            if ((Fcc1Fault && UpdateSide == 1) || (Fcc2Fault && UpdateSide == 2))
            {
                EICASMessage FccFail = new EICASMessage();
                FccFail.Importance = 1;
                FccFail.MessageText = "FCC " + UpdateSide.ToString();
                GUIOwnship.AddEicasMessage(FccFail);
                GUIOwnship.CautionActive = true;
            }
            else if ((!Fcc1Fault && UpdateSide == 1) || (!Fcc2Fault && UpdateSide == 2))
            {
                EICASMessage FccFail = new EICASMessage();
                FccFail.Importance = 1;
                FccFail.MessageText = "FCC " + UpdateSide.ToString();
                GUIOwnship.RemoveEicasMessage(FccFail);

                EICASMessage SplrFail = new EICASMessage();
                SplrFail.Importance = 2;
                SplrFail.MessageText = "SPOILERS";
                GUIOwnship.RemoveEicasMessage(SplrFail);

                if (!GUIOwnship.MalfHyd1)
                {
                    //Loss of 1,3,6,8
                    SplrLoss1.Hide();
                    SplrLoss3.Hide();
                    SplrLoss6.Hide();
                    SplrLoss8.Hide();
                }
                if (!GUIOwnship.MalfHyd2)
                {
                    //Loss of 2,4,5,7
                    SplrLoss2.Hide();
                    SplrLoss4.Hide();
                    SplrLoss5.Hide();
                    SplrLoss7.Hide();
                }
            }
        }
        private void HideMalfunctionComponents()
        {
            EICASDISPLAY1OFF.Hide();
            EICASDISPLAY2OFF.Hide();
            SplrLoss1.Hide();
            SplrLoss2.Hide();
            SplrLoss3.Hide();
            SplrLoss4.Hide();
            SplrLoss5.Hide();
            SplrLoss6.Hide();
            SplrLoss7.Hide();
            SplrLoss8.Hide();
        }
        private void HydSysFailure(bool MalfActive, int side)
        {
            if (MalfActive)
            {
                if (side == 1)
                {
                    EICASMessage HYDFail1 = new EICASMessage();
                    HYDFail1.Importance = 1;
                    HYDFail1.MessageText = "HYD SYS 1";
                    GUIOwnship.AddEicasMessage(HYDFail1);
                    GUIOwnship.CautionActive = true;
                    //Loss of 1,3,6,8
                    SplrLoss1.Show();
                    SplrLoss3.Show();
                    SplrLoss6.Show();
                    SplrLoss8.Show();
                }
                else
                {
                    EICASMessage HYDFail2 = new EICASMessage();
                    HYDFail2.Importance = 1;
                    HYDFail2.MessageText = "HYD SYS 2";
                    GUIOwnship.AddEicasMessage(HYDFail2);
                    GUIOwnship.CautionActive = true;
                    //Loss of 2,4,5,7
                    SplrLoss2.Show();
                    SplrLoss4.Show();
                    SplrLoss5.Show();
                    SplrLoss7.Show();

                }
            }
            else
            {
                if (side == 1)
                {
                    EICASMessage HYDFail1 = new EICASMessage();
                    HYDFail1.Importance = 1;
                    HYDFail1.MessageText = "HYD SYS 1";
                    GUIOwnship.RemoveEicasMessage(HYDFail1);
                    //Loss of 1,3,6,8
                    SplrLoss1.Hide();
                    SplrLoss3.Hide();
                    SplrLoss6.Hide();
                    SplrLoss8.Hide();
                }
                else
                {
                    EICASMessage HYDFail2 = new EICASMessage();
                    HYDFail2.Importance = 1;
                    HYDFail2.MessageText = "HYD SYS 2";
                    GUIOwnship.RemoveEicasMessage(HYDFail2);
                    //Loss of 2,4,5,7
                    SplrLoss2.Hide();
                    SplrLoss4.Hide();
                    SplrLoss5.Hide();
                    SplrLoss7.Hide();
                }
            }
            return;
        }

        private void UnderspeedCaution()
        {
            if (underspeed_warning && !underspeed_shown)
            {
                EICASMessage UnderSpeed = new EICASMessage();
                UnderSpeed.Importance = 1;
                UnderSpeed.MessageText = "UNDERSPEED";
                GUIOwnship.AddEicasMessage(UnderSpeed);
                GUIOwnship.CautionActive = true;
                underspeed_shown = true;
            }
            else if (!underspeed_warning && underspeed_shown)
            {
                EICASMessage UnderSpeed = new EICASMessage();
                UnderSpeed.Importance = 1;
                UnderSpeed.MessageText = "UNDERSPEED";
                GUIOwnship.RemoveEicasMessage(UnderSpeed);
                GUIOwnship.CautionActive = false;
                underspeed_shown = false;
            }
        }

        private void ConfigWarning()
        {
            if (config_warning && !config_wrng_shown)
            {
                EICASMessage Config_Wrng = new EICASMessage();
                Config_Wrng.Importance = 2;
                Config_Wrng.MessageText = "AIRCRAFT CONFIG";
                GUIOwnship.AddEicasMessage(Config_Wrng);
                config_wrng_shown = true;
                GUIOwnship.WarningActive = true;
                WarningSound.PlayLooping();
            }
            else if (!config_warning && config_wrng_shown)
            {
                EICASMessage Config_Wrng = new EICASMessage();
                Config_Wrng.Importance = 2;
                Config_Wrng.MessageText = "AIRCRAFT CONFIG";
                GUIOwnship.RemoveEicasMessage(Config_Wrng);
                config_wrng_shown = false;
                GUIOwnship.WarningActive = false;
                WarningSound.Stop();
            }
        }

        private void PowerLossMalfunction(bool MalfStatus)
        {
            if (MalfStatus)
            {
                EICASDISPLAY1OFF.Show();
                EICASDISPLAY2OFF.Show();
            }
            else
            {
                EICASDISPLAY1OFF.Hide();
                EICASDISPLAY2OFF.Hide();
            }
        }
        private void ReadCockpitControls()
        {
            //SPOILER LEVER VIA JOYSTICK
            if (HOTAS.DeviceStatus())
            {
                if (SpoilerLever.Value > -10 && HOTAS.X_button()) SpoilerLever.Value -= 2;
                if (SpoilerLever.Value < 2 && HOTAS.Square_button()) SpoilerLever.Value += 2;
            }
            //UPDATE SPOILERS
            if (SpoilerLever.Value <= 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    GUIOwnship.SpoilerDeflectionPercentage[i] = -SpoilerLever.Value * 10;
                }
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    GUIOwnship.SpoilerDeflectionPercentage[i] = 0;
                }
            }

            //REFRESH VIA JOYSTICK
            if (HOTAS.DeviceStatus())
            {

                //PITCH CTRL VIA JOYSTICK
                PitchBar.Value = (int)(HOTAS.Y_axis() * (-10));


                //CONTROL WHEEL VIA JOYSTICK
                ControlWheelBar.Value = (int)(HOTAS.X_axis() * 10);

                //FLAP
                if (FlapLever.Value < 0 && HOTAS.R2_button()) FlapLever.Value++;
                if (FlapLever.Value > -3 && HOTAS.L2_button()) FlapLever.Value--;

                //LANDING GEAR
                if (HOTAS.O_button()) GUIOwnship.GearPositionChange();

                //Options Button
                if (HOTAS.Options_button()) ding.Play();

                //Missile
                if (HOTAS.L1_button()) missile.Play();

                //Brrrt
                if (HOTAS.Trigger_button()) cannon.Play();

                //Autobrake Position
                if (HOTAS.POV_Hat() >= 0 && HOTAS.POV_Hat() <= 4)
                {
                    GUIOwnship.AutoBrakeSelectorPosition = HOTAS.POV_Hat();
                }

                //THROTTLES
                LENGThrottle.Value = HOTAS.Throttle();
            }

            //UPDATE FLAPS
            GUIOwnship.FlapLeverPosition = -1 * FlapLever.Value;


            //CONTROL WHEEL
            GUIOwnship.SWControlWheelPosition = ControlWheelBar.Value;

            if (GUIOwnship.SWControlWheelPosition >= 0)
            {
                GUIOwnship.MFS_Right = 100 - (int)(GUIOwnship.SWControlWheelPosition * 3);
                GUIOwnship.MFS_Left = 100;
            }
            else
            {
                GUIOwnship.MFS_Left = 100 + (int)(GUIOwnship.SWControlWheelPosition * 3);
                GUIOwnship.MFS_Right = 100;
            }


            //THROTTLES
            RENGThrottle.Value = LENGThrottle.Value;
            GUIOwnship.LThrottlePosition = LENGThrottle.Value;
            GUIOwnship.RThrottlePosition = RENGThrottle.Value;
            
        }
        private void ReadDataPipe(string PipeName)
        {

        }
        private void RefreshAutoBrakeSelector()
        {
            switch (GUIOwnship.AutoBrakeSelectorPosition)
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
            airSpeedIndicatorInstrumentControl1.SetAirSpeedIndicatorParameters((int)GUIOwnship.IasKts);
            attitudeIndicatorInstrumentControl1.SetAttitudeIndicatorParameters(GUIOwnship.AoA, GUIOwnship.BankAngle);
            altimeterInstrumentControl1.SetAlimeterParameters((int)GUIOwnship.AltitudeASL);
            verticalSpeedIndicatorInstrumentControl1.SetVerticalSpeedIndicatorParameters((int)GUIOwnship.VS);
            EIEngine1Control.SetEngineIndicatorParameters(LENGThrottle.Value);
            EIEngine2Control.SetEngineIndicatorParameters(RENGThrottle.Value);
        }
        private void RefreshEICAS()
        {
            RefreshEICASAllMessages();
            RefreshEICASSystemStatus();
        }
        private void RefreshEICASAllMessages()
        {
            EICASMessage[] ArEICASMsgs = GUIOwnship.EICASMessages.ToArray();
            int count = ArEICASMsgs.Length;
            for (int i = 0; i < 11; i++)
            {
                switch (i)
                {
                    case (0):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine1.Text = "";
                                break;
                            }
                            EicasMsgLine1.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine1.ForeColor = Color.White; break;
                                case 1: EicasMsgLine1.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine1.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                    case (1):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine2.Text = "";
                                break;
                            }
                            EicasMsgLine2.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine2.ForeColor = Color.White; break;
                                case 1: EicasMsgLine2.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine2.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                    case (2):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine3.Text = "";
                                break;
                            }
                            EicasMsgLine3.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine3.ForeColor = Color.White; break;
                                case 1: EicasMsgLine3.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine3.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                    case (3):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine4.Text = "";
                                break;
                            }
                            EicasMsgLine4.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine4.ForeColor = Color.White; break;
                                case 1: EicasMsgLine4.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine4.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                    case (4):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine5.Text = "";
                                break;
                            }
                            EicasMsgLine5.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine5.ForeColor = Color.White; break;
                                case 1: EicasMsgLine5.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine5.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                    case (5):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine6.Text = "";
                                break;
                            }
                            EicasMsgLine6.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine6.ForeColor = Color.White; break;
                                case 1: EicasMsgLine6.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine6.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                    case (6):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine7.Text = "";
                                break;
                            }
                            EicasMsgLine7.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine7.ForeColor = Color.White; break;
                                case 1: EicasMsgLine7.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine7.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                    case (7):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine8.Text = "";
                                break;
                            }
                            EicasMsgLine8.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine8.ForeColor = Color.White; break;
                                case 1: EicasMsgLine8.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine8.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                    case (8):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine9.Text = "";
                                break;
                            }
                            EicasMsgLine9.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine9.ForeColor = Color.White; break;
                                case 1: EicasMsgLine9.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine9.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                    case (9):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine10.Text = "";
                                break;
                            }
                            EicasMsgLine10.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine10.ForeColor = Color.White; break;
                                case 1: EicasMsgLine10.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine10.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                    case (10):
                        {
                            if (i >= count)
                            {
                                EicasMsgLine11.Text = "";
                                break;
                            }
                            EicasMsgLine11.Text = ArEICASMsgs[i].MessageText;
                            switch (ArEICASMsgs[i].Importance)
                            {
                                case 0: EicasMsgLine11.ForeColor = Color.White; break;
                                case 1: EicasMsgLine11.ForeColor = Color.Orange; break;
                                case 2: EicasMsgLine11.ForeColor = Color.Red; break;
                            }
                            break;
                        }
                }
            };
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
                FlapPos = Convert.ToString(GUIOwnship.FlapLeverPosition * 10);
            }
            EicasFlapsMessage.Text = "FLAPS..........." + FlapPos;

            //SPOILERS
            if (SpoilerLever.Value >= 0) EicasSpoilerMessage.Text = "";
            else EicasSpoilerMessage.Text = "SPOILERS";


            //AUTOBRAKES
            switch (GUIOwnship.AutoBrakeSelectorPosition)
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
            Spoiler1PGB.Refresh();          
            Spoiler2PGB.Refresh();           
            Spoiler3PGB.Refresh();           
            Spoiler4PGB.Refresh();           
            Spoiler5PGB.Refresh();          
            Spoiler6PGB.Refresh();           
            Spoiler7PGB.Refresh();           
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
                else MWMCPB.BackgroundImage = Resources.MWMC_10;
            }
            else
            {
                if (GUIOwnship.CautionActive) MWMCPB.BackgroundImage = Resources.MWMC_01;
                else MWMCPB.BackgroundImage = Resources.MWMC_00;
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
        private void RefreshSpoilerActuation(int CurrentDeflection, int TargetDeflection, bool InFlight = true, bool SymDeploy = true)
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
                    for (int j = 0; j < GUIOwnship.NbofSpoilers; j++)
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
            return;
        }
        private void RefreshAttitude()
        {
            //Indicated Airspeed
            if (GUIOwnship.PhaseOfFlight != "CRUISE" && GUIOwnship.PhaseOfFlight != "APPROACH")
            {
                if (GUIOwnship.IasKts < 1.9 * LENGThrottle.Value) GUIOwnship.IasKts += 1 * (LENGThrottle.Value / 100.0);
                else if (GUIOwnship.IasKts > (1.9 * LENGThrottle.Value) + 1)
                {
                    if (HOTAS.DeviceStatus())
                    {
                        if (HOTAS.Triangle_button()) GUIOwnship.IasKts--;
                    }
                    else if (GUIOwnship.AltitudeASL > GUIOwnship.RunwayAltASL) GUIOwnship.IasKts -= 0.5;
                    else GUIOwnship.IasKts -= 0.1;
                }
            }
            else
            {
                if (GUIOwnship.IasKts < 4.2 * LENGThrottle.Value) GUIOwnship.IasKts += 1 * (LENGThrottle.Value / 100.0);
                else if (GUIOwnship.IasKts > (4.2 * LENGThrottle.Value) + 1)
                {
                    if (HOTAS.DeviceStatus())
                    {
                        if (HOTAS.Triangle_button()) GUIOwnship.IasKts--;
                    }
                    else if (GUIOwnship.AltitudeASL > GUIOwnship.RunwayAltASL) GUIOwnship.IasKts -= 0.5;
                    else GUIOwnship.IasKts -= 0.1;
                }
            }
            if ((GUIOwnship.PhaseOfFlight == "CLIMB" || GUIOwnship.PhaseOfFlight == "CRUISE" || GUIOwnship.PhaseOfFlight == "APPROACH") && GUIOwnship.IasKts < 100)
            {
                if (speed_alert == 0)
                {
                    underspeed.Play();
                    speed_alert++;
                }
                else speed_alert++;

                if (speed_alert >= 2) speed_alert = 0;
                underspeed_warning = true;
            }
            else underspeed_warning = false;
            UnderspeedCaution();
            if (GUIOwnship.IasKts > 0 && SpoilerLever.Value < 0) GUIOwnship.IasKts -= (double)SpoilerLever.Value / -10;


            //Bank Angle
            if (GUIOwnship.AltitudeASL > GUIOwnship.RunwayAltASL && Math.Abs(GUIOwnship.BankAngle) <= 30)
            {
                GUIOwnship.BankAngle -= GUIOwnship.SWControlWheelPosition / 2;
            }

            if ((GUIOwnship.BankAngle > 30) || ((GUIOwnship.BankAngle + GUIOwnship.SWControlWheelPosition / 2) > 30)) GUIOwnship.BankAngle = 30;
            else if ((GUIOwnship.BankAngle < -30) || (GUIOwnship.BankAngle + GUIOwnship.SWControlWheelPosition / 2) < -30) GUIOwnship.BankAngle = -30;

            if (GUIOwnship.SWControlWheelPosition == 0 && GUIOwnship.BankAngle > 0) GUIOwnship.BankAngle -= 1;
            else if (GUIOwnship.SWControlWheelPosition == 0 && GUIOwnship.BankAngle < 0) GUIOwnship.BankAngle += 1;
            if (Math.Abs(GUIOwnship.BankAngle) < 1) GUIOwnship.BankAngle = 0;


            //Angle of Attack
            if (GUIOwnship.AltitudeASL > GUIOwnship.RunwayAltASL && Math.Abs(GUIOwnship.AoA) <= 25)
            {
                if (GUIOwnship.AoA < -PitchBar.Value * 2) GUIOwnship.AoA += 1 - PitchBar.Value / 10;
                else if (GUIOwnship.AoA > -PitchBar.Value * 2) GUIOwnship.AoA -= 1 + PitchBar.Value / 10;
            }
            else
            {
                if (GUIOwnship.PhaseOfFlight == "TAKEOFF" && GUIOwnship.IasKts > 100)
                {
                    if (PitchBar.Value < 0) GUIOwnship.AoA -= (double)PitchBar.Value;
                    else GUIOwnship.AoA = 0;
                }
                else
                {
                    GUIOwnship.AoA = 0;
                    GUIOwnship.VS = 0;
                }
            }
            if (GUIOwnship.AoA > 25) GUIOwnship.AoA = 25;
            else if (GUIOwnship.AoA < -25) GUIOwnship.AoA = -25;

            if (PitchBar.Value == 0 && GUIOwnship.AoA > 0) GUIOwnship.AoA -= 0.5;
            else if (PitchBar.Value == 0 && GUIOwnship.AoA < 0) GUIOwnship.AoA += 0.5;
            if (Math.Abs(GUIOwnship.AoA) < 0.25) GUIOwnship.AoA = 0;



            //Vertical Speed (Climb Indicator)
            if (GUIOwnship.PhaseOfFlight == "TAKEOFF" && GUIOwnship.IasKts >= 100 && GUIOwnship.InducedLift < 400) GUIOwnship.InducedLift += 5;
            else if (GUIOwnship.PhaseOfFlight == "APPROACH" && GUIOwnship.InducedLift > -400) GUIOwnship.InducedLift -= 5;
            else
            {
                if (GUIOwnship.InducedLift < 0) GUIOwnship.InducedLift += 5;
                else if (GUIOwnship.InducedLift > 0) GUIOwnship.InducedLift -= 5;
            }

            if (Math.Abs(GUIOwnship.VS) <= 2500)
            {
                GUIOwnship.VS = GUIOwnship.InducedLift + 100 * GUIOwnship.AoA;
            }
            else if (GUIOwnship.VS > 2500) GUIOwnship.VS = 2500;
            else if (GUIOwnship.VS < -2500) GUIOwnship.VS = -2500;


            //Altitude
            GUIOwnship.AltitudeASL += GUIOwnship.VS / 600;
            if (GUIOwnship.AltitudeASL <= GUIOwnship.RunwayAltASL)
            {
                GUIOwnship.AoA = 0;
                GUIOwnship.VS = 0;
                GUIOwnship.AltitudeASL = GUIOwnship.RunwayAltASL;
            }

            //GND Spoilers (Auto)
            if (GUIOwnship.WeightOnWheels && SpoilerLever.Value == 0 && (GUIOwnship.PhaseOfFlight == "LANDING" || GUIOwnship.PhaseOfFlight == "RTO")) armed_trigger = true;
            if (armed_trigger && SpoilerLever.Value > -10)
            {
                SpoilerLever.Value--;
                if (SpoilerLever.Value == -10) armed_trigger = false;
            }

            //GND Spoilers (Manual)
            if (Spoiler3PGB.Value < 100 - GUIOwnship.SpoilerDeflectionPercentage[2]) Spoiler3PGB.Value += 10;
            else if (Spoiler3PGB.Value > 100 - GUIOwnship.SpoilerDeflectionPercentage[2]) Spoiler3PGB.Value -= 10;

            if (Spoiler4PGB.Value < 100 - GUIOwnship.SpoilerDeflectionPercentage[3]) Spoiler4PGB.Value += 10;
            else if (Spoiler4PGB.Value > 100 - GUIOwnship.SpoilerDeflectionPercentage[3]) Spoiler4PGB.Value -= 10;

            if (Spoiler5PGB.Value < 100 - GUIOwnship.SpoilerDeflectionPercentage[4]) Spoiler5PGB.Value += 10;
            else if (Spoiler5PGB.Value > 100 - GUIOwnship.SpoilerDeflectionPercentage[4]) Spoiler5PGB.Value -= 10;

            if (Spoiler6PGB.Value < 100 - GUIOwnship.SpoilerDeflectionPercentage[5]) Spoiler6PGB.Value += 10;
            else if (Spoiler6PGB.Value > 100 - GUIOwnship.SpoilerDeflectionPercentage[5]) Spoiler6PGB.Value -= 10;

            //MFS Spoilers (Manual)
            if (GUIOwnship.MFS_as_brake < GUIOwnship.SpoilerDeflectionPercentage[1]) GUIOwnship.MFS_as_brake += 10;
            else if (GUIOwnship.MFS_as_brake > GUIOwnship.SpoilerDeflectionPercentage[1]) GUIOwnship.MFS_as_brake -= 10;

            if (GUIOwnship.MFS_Left - GUIOwnship.MFS_as_brake <= 0) Spoiler2PGB.Value = 0;
            else Spoiler2PGB.Value = GUIOwnship.MFS_Left - GUIOwnship.MFS_as_brake;
            Spoiler1PGB.Value = Spoiler2PGB.Value;

            if (GUIOwnship.MFS_as_brake < GUIOwnship.SpoilerDeflectionPercentage[6]) GUIOwnship.MFS_as_brake += 10;
            else if (GUIOwnship.MFS_as_brake > GUIOwnship.SpoilerDeflectionPercentage[6]) GUIOwnship.MFS_as_brake -= 10;

            if (GUIOwnship.MFS_Right - GUIOwnship.MFS_as_brake <= 0) Spoiler7PGB.Value = 0;
            else Spoiler7PGB.Value = GUIOwnship.MFS_Right - GUIOwnship.MFS_as_brake;
            Spoiler8PGB.Value = Spoiler7PGB.Value;

            //Acft Configuration
            if (GUIOwnship.PhaseOfFlight == "TAKEOFF" && (FlapLever.Value != -1 || SpoilerLever.Value < 0))
            {
                config_warning = true;
                ConfigWarning();
            }
            else
            {
                config_warning = false;
                ConfigWarning();
            }
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
                        GUIOwnship.IasKts = 154;
                        if (GUIOwnship.GlobalGearStatus() != "DOWN") GUIOwnship.GearPositionChange();
                        GUIOwnship.WeightOnWheels = false;
                        GUIOwnship.PhaseOfFlight = "APPROACH";
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
            //GUIOwnship.VS = 0;
            GUIOwnship.WeightOnWheels = true;
            while (GUIOwnship.IasKts > 0)
            {
                if (GUIOwnship.AoA > 0) GUIOwnship.AoA -= 0.05;
                GUIOwnship.IasKts -= 1;
                if (GUIOwnship.IasKts < 0) GUIOwnship.IasKts = 0;
                Thread.Sleep(100);
            }
            return;
        }
        private void SpoilerMismatchMalf()
        {

            int killsplr = 0;
            if (GUIOwnship.MalfSplrs && splrmismatchactive ==0)
            {
                Random rnd = new Random();
                killsplr = rnd.Next(5, 9); // creates a number between 1 and 12
                splrmismatchactive = killsplr;

                EICASMessage SplrMM = new EICASMessage();
                SplrMM.Importance = 1;
                SplrMM.MessageText = "SPOILERS";
                GUIOwnship.AddEicasMessage(SplrMM);
                GUIOwnship.CautionActive = true;

                switch (killsplr)
                {
                    case (5):
                        {
                            SplrLoss5.Show();
                            SplrLoss4.Show();
                            break;
                        }
                    case (6):
                        {
                            SplrLoss6.Show();
                            SplrLoss3.Show();
                            break;
                        }
                    case (7):
                        {
                            SplrLoss7.Show();
                            SplrLoss2.Show();
                            break;
                        }
                    case (8):
                        {
                            SplrLoss8.Show();
                            SplrLoss1.Show();
                            break;
                        }
                    default: break;
                }
            }
            else if(!GUIOwnship.MalfSplrs && splrmismatchactive !=0)
            {
                EICASMessage SplrMM = new EICASMessage();
                SplrMM.Importance = 1;
                SplrMM.MessageText = "SPOILERS";
                GUIOwnship.RemoveEicasMessage(SplrMM);
                GUIOwnship.CautionActive = false;


                switch (splrmismatchactive)
                {
                    case (5):
                        {
                            SplrLoss5.Hide();
                            SplrLoss4.Hide();
                            splrmismatchactive = 0;
                            break;
                        }
                    case (6):
                        {
                            SplrLoss6.Hide();
                            SplrLoss3.Hide();
                            splrmismatchactive = 0;
                            break;
                        }
                    case (7):
                        {
                            SplrLoss7.Hide();
                            SplrLoss2.Hide();
                            splrmismatchactive = 0;
                            break;
                        }
                    case (8):
                        {
                            SplrLoss8.Hide();
                            SplrLoss1.Hide();
                            splrmismatchactive = 0;
                            break;
                        }
                    default: break;
                }
            }
        }

        private void UpdatePhaseOfFlight()
        {
            switch (GUIOwnship.PhaseOfFlight)
            {
                case ("TAXI"):
                    {
                        if (GUIOwnship.LThrottlePosition > 70 && GUIOwnship.RThrottlePosition > 70 && GUIOwnship.IasKts > 60) GUIOwnship.PhaseOfFlight = "TAKEOFF";
                        break;
                    }
                case ("TAKEOFF"):
                    {
                        if (GUIOwnship.LThrottlePosition < 50 && GUIOwnship.RThrottlePosition < 50) GUIOwnship.PhaseOfFlight = "RTO";
                        else if (GUIOwnship.GlobalGearStatus() == "UP") GUIOwnship.PhaseOfFlight = "CLIMB";
                        break;
                    }
                case ("CLIMB"):
                    {
                        if (GUIOwnship.VS < 100) GUIOwnship.PhaseOfFlight = "CRUISE";
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
                        if (GUIOwnship.IasKts <= 60) GUIOwnship.PhaseOfFlight = "TAXI";
                        break;
                    }
                case ("RTO"):
                    {
                        if (GUIOwnship.IasKts <= 1) GUIOwnship.PhaseOfFlight = "TAXI";
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
            if (IntegerInput.Text == "Enter Value Here")
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
            if (GUIOwnship.WarningActive) WarningSound.Stop();
            GUIOwnship.CautionActive = false;
            GUIOwnship.WarningActive = false;
        }
        private void SW1PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch1On = !GUIOwnship.Switch1On;
            GUIOwnship.MalfPwrLoss = !GUIOwnship.MalfPwrLoss;
            PowerLossMalfunction(GUIOwnship.MalfPwrLoss);
            if (!GUIOwnship.Switch1On) SW1PB.BackgroundImage = Resources.Switch_ON;
            else SW1PB.BackgroundImage = Resources.Switch_OFF;
            if (!avionics_start)
            {
                avionics.Play();
                avionics_start = true;
            }
        }
        private void SW2PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch2On = !GUIOwnship.Switch2On;
            GUIOwnship.MalfFcc1 = !GUIOwnship.MalfFcc1;
            FccFault(GUIOwnship.MalfFcc1, GUIOwnship.MalfFcc2, 1);
            if (GUIOwnship.Switch2On) SW2PB.BackgroundImage = Resources.Switch_ON;
            else SW2PB.BackgroundImage = Resources.Switch_OFF;
        }
        private void SW3PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch3On = !GUIOwnship.Switch3On;
            GUIOwnship.MalfFcc2 = !GUIOwnship.MalfFcc2;
            FccFault(GUIOwnship.MalfFcc1, GUIOwnship.MalfFcc2, 2);
            if (GUIOwnship.Switch3On) SW3PB.BackgroundImage = Resources.Switch_ON;
            else SW3PB.BackgroundImage = Resources.Switch_OFF;
        }
        private void SW4PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch4On = !GUIOwnship.Switch4On;
            GUIOwnship.MalfHyd1 = !GUIOwnship.MalfHyd1;
            HydSysFailure(GUIOwnship.MalfHyd1, 1);
            if (GUIOwnship.Switch4On) SW4PB.BackgroundImage = Resources.Switch_ON;
            else SW4PB.BackgroundImage = Resources.Switch_OFF;
        }
        private void SW5PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch5On = !GUIOwnship.Switch5On;
            GUIOwnship.MalfHyd2 = !GUIOwnship.MalfHyd2;
            HydSysFailure(GUIOwnship.MalfHyd2, 2);
            if (GUIOwnship.Switch5On) SW5PB.BackgroundImage = Resources.Switch_ON;
            else SW5PB.BackgroundImage = Resources.Switch_OFF;
        }
        private void SW6PB_Click(object sender, EventArgs e)
        {
            GUIOwnship.Switch6On = !GUIOwnship.Switch6On;
            GUIOwnship.MalfSplrs = !GUIOwnship.MalfSplrs;
            if (GUIOwnship.Switch6On) SW6PB.BackgroundImage = Resources.Switch_ON;
            else SW6PB.BackgroundImage = Resources.Switch_OFF;
        }
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
        #endregion

    }
}
