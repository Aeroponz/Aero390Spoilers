using System;
using System.Collections.Generic;
using System.Text;

namespace Ownship
{
     public class EICASMessage
    {
        public EICASMessage()
        {
            MessageText = "";
            Importance = 0;
        }

        public string MessageText { get; set; }
        public int Importance { get; set; }// 0 = Status / 1 = Caution / 2 = Warning
    }
}
