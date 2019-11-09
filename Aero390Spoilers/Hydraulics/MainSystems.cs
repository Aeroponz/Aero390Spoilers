using System;

namespace Hydraulics
{
    public class HydraulicSys
    {
        //MFS Inboard
        //Ground Spoilers Inboard
        //Rudder
        //Left TR
        //inboard brakes
        //nose wheel actuator
        //elevator
        //alternate flaps
        //landing gear
        public HydraulicSys()
        {
            AvailPressure = 3000;
        }
        public double GetAvailPress()
        {
            return AvailPressure;
        }
        double AvailPressure; //psi
    }
}
