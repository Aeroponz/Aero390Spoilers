using Aero390Spoilers.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Aero390Spoilers
{

    public partial class AircraftGUI : Form
    {

        Ownship.Aircraft Ownship = new Ownship.Aircraft();

        public void AircraftGUI_Tick()
        {
            Timer timer = new Timer();
            timer.Interval = (500); // 0.5 secs
            timer.Tick += new EventHandler(GUI_TickJobs);
            timer.Start();
        }

        private void GUI_TickJobs(object sender, EventArgs e)
        {
            RefreshGearPict();
            RefreshPrintOuts();
        }
        public AircraftGUI()
        {
            InitializeComponent();
            AircraftGUI_Tick();
        }
        private void GearPos_Click(object sender, EventArgs e)
        {
            Ownship.GearPositionChange();
        }
        private void RefreshGearPict()
        {

            //AIRCRAFT SCHEMATIC AND GEAR ICON
            if (Ownship.GlobalGearStatus() == "UP")
            {
                GearStatusIconPB.Image = Resources.LgIcon_Up;
                ACSchemPB.Image = Resources.LgUpSchem;
            }
            else if (Ownship.GlobalGearStatus() == "DOWN")
            {
                GearStatusIconPB.Image = Resources.LgIcon_Down;

                if (Ownship.WeightOnWheels == true) //AC on ground
                {
                    ACSchemPB.Image = Resources.WowSchem;
                }
                else //AC in flight
                {
                    ACSchemPB.Image = Resources.LgDownSchem;
                }
            }
            else if (Ownship.GlobalGearStatus() == "IN TRANSIT")
            {
                GearStatusIconPB.Image = Resources.LgIcon_Transit;
            }
            else
            {
                GearStatusIconPB.Image = Resources.LgIcon_Unknown;
            }

            //WEIGHT ON WHEELS ICON
            if(Ownship.WeightOnWheels == true)
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
            GwPrintOut.Text = Ownship.GrossWeightLbs.ToString();
            BaroPrintOut.Text = Ownship.BaroSettingmmHg.ToString();
            AltPrintOut.Text = Ownship.AltitudeASL.ToString();
            IASPrintOut.Text = Ownship.IasKts.ToString();
        }

        private void WowButton_Click(object sender, EventArgs e)
        {
            Ownship.WeightOnWheels = !(Ownship.WeightOnWheels);
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
                if (Input_double<= Ownship.MTOW && Input_double >= Ownship.ZFW)
                {
                    Ownship.GrossWeightLbs = Input_double;
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
                    Ownship.BaroSettingmmHg = Input_double;
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
                    Ownship.AltitudeASL = Input_double;
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
                double Input_double = double.Parse(IntegerInput.Text, System.Globalization.CultureInfo.InvariantCulture);
                if (Input_double <= 300/*Ownship.IasNES*/ && Input_double >= 0)
                {
                    Ownship.IasKts = Input_double;
                    IntegerInput.Text = "Enter Value Here";
                }
                else
                {
                    IntegerInput.Text = "Invalid Input : Out of Range";
                }

            }
        }
    }
}
