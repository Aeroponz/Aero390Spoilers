using Aero390Spoilers.Properties;
using Ownship;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;


namespace Aero390Spoilers
{

    public partial class AircraftGUI : Form
    {

        Ownship.Aircraft GUIOwnship = new Ownship.Aircraft();
        bool SpoilerThreadRunning = false;
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
            timer.Interval = (100); // 0.5 secs
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
            RefreshPrintOuts();
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
            if(GUIOwnship.WeightOnWheels == true)
            {
                WoWPBLight.Image = Resources.WoWLightOn;
            }
            else
            {
                WoWPBLight.Image = Resources.WoWLightOff;
            }

        }
        private void RefreshPrintOuts()
        {
            GwPrintOut.Text = GUIOwnship.GrossWeightLbs.ToString();
            BaroPrintOut.Text = GUIOwnship.BaroSettingmmHg.ToString();
            AltPrintOut.Text = GUIOwnship.AltitudeASL.ToString();
            IASPrintOut.Text = GUIOwnship.IasKts.ToString();
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
            verticalSpeedIndicatorInstrumentControl1.SetVerticalSpeedIndicatorParameters(GUIOwnship.VS);
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

        #endregion
    }
}
