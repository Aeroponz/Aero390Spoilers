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
        public AircraftGUI()
        {
            InitializeComponent();
        }
        private void GearPos_Click(object sender, EventArgs e)
        {
            Ownship.GearPositionChange();
            RefreshGearPict();
        }
        private void RefreshGearPict()
        {
            string ImageNameIcon = "";
            string ImageNameSchem = "";
            if(Ownship.GlobalGearStatus() == "UP")
            {
                ImageNameIcon = "LgIcon_Up.png";
                ImageNameSchem = "LgUpSchem.png";
                ACSchemPB.Image = Image.FromFile(@".\Resources\" + ImageNameSchem);
            }
            else if (Ownship.GlobalGearStatus() == "DOWN")
            {
                ImageNameIcon = "LgIcon_Down.png";
                ImageNameSchem = "LgDownSchem.png";
                ACSchemPB.Image = Image.FromFile(@".\Resources\" + ImageNameSchem);
            }
            else if (Ownship.GlobalGearStatus() == "IN TRANSIT")
            {
                ImageNameIcon = "LgIcon_Transit.png";
            }
            else
            {
                ImageNameIcon = "LgIcon_Unknown.png";
            }
            GearStatusIconPB.Image = Image.FromFile(Directory.GetCurrentDirectory() + @".\Resources\" + ImageNameIcon);
        }

    }
}
