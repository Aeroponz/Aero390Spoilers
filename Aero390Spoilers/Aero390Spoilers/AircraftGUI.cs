using Aero390Spoilers.Properties;
using Ownship;
using System;
using System.Drawing;
using System.Windows.Forms;


namespace Aero390Spoilers
{

    public partial class AircraftGUI : Form
    {

        Ownship.Aircraft GUIOwnship = new Ownship.Aircraft();
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
            Timer timer = new Timer();
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
            RefreshPrintOuts();
        }
        private void RefreshGearPict()
        {

            //AIRCRAFT SCHEMATIC AND GEAR ICON
            if (GUIOwnship.GlobalGearStatus() == "UP")
            {
                GearStatusIconPB.Image = Resources.LgIcon_Up;
                ACSchemPB.Image = Resources.LgUpSchem;
            }
            else if (GUIOwnship.GlobalGearStatus() == "DOWN")
            {
                GearStatusIconPB.Image = Resources.LgIcon_Down;

                if (GUIOwnship.WeightOnWheels == true) //AC on ground
                {
                    ACSchemPB.Image = Resources.WowSchem;
                }
                else //AC in flight
                {
                    ACSchemPB.Image = Resources.LgDownSchem;
                }
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
                WoWIconPB.Image = Resources.LgIcon_Wow;
            }
            else
            {
                WoWIconPB.Image = null;
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
        private void ReadCockpitControls()
        {
            int temp = GUIOwnship.SpoilerLeverPosition;
            GUIOwnship.SpoilerLeverPosition = -1 * SpoilerLever.Value;
            if (temp != GUIOwnship.SpoilerLeverPosition) RefreshSpoilerActuation();
            GUIOwnship.FlapLeverPosition = -1 * FlapLever.Value;
            GUIOwnship.SWControlWheelPosition = ControlWheelBar.Value;
            GUIOwnship.BankAngle = GUIOwnship.SWControlWheelPosition * 3;
        }
        private void RefreshSpoilerActuation()
        {

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
        private void GearPos_Click(object sender, EventArgs e)
        {
            GUIOwnship.GearPositionChange();
        }
        #endregion


    }
}
