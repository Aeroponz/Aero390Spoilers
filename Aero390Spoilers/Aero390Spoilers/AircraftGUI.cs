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
            RefreshGearPict();
            RefreshCockpitInstruments();
            RefreshFCSynoptic();
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
        private void RefreshCockpitInstruments()
        {
            airSpeedIndicatorInstrumentControl1.SetAirSpeedIndicatorParameters(GUIOwnship.IasKts);
            attitudeIndicatorInstrumentControl1.SetAttitudeIndicatorParameters(GUIOwnship.AoA, GUIOwnship.BankAngle);
            altimeterInstrumentControl1.SetAlimeterParameters((int)GUIOwnship.AltitudeASL);
            verticalSpeedIndicatorInstrumentControl1.SetVerticalSpeedIndicatorParameters(GUIOwnship.VS);
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
            GUIOwnship.FlapLeverPosition = -1 * FlapLever.Value;
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
                //double newTarget = ((double)(GUIOwnship.SpoilerLeverPosition) / 10.0) * 100;
                // if (InFlight)
                // {
                //     if (SymDeploy)
                //     {
                //         newTarget *= GUIOwnship.SpoilerSBrakeDeflection;
                //     }
                //     else
                //     {
                //         newTarget *= GUIOwnship.SpoilerFlightDeflection;
                //     }
                //     if ((int)newTarget != TargetDeflection)
                //     {
                //         TargetDeflection = (int)newTarget;
                //     }
                // }

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

        #endregion
    }
}
