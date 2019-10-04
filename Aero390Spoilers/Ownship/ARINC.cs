using System;
using System.Collections.Generic;
using System.Text;

namespace Ownship
{
    class ARINC
    {
        public ARINC()
        {
            A429Message = 0b_0000_0000_0000_0000_0000_0000_0000_0000;

        }

        #region Parameters

        //|        ARINC 429 Word Format                                                         |
        //|P |SSM  |MSB                     Data                         LSB|SDI |Label          |
        //|32|31|30|29|28|27|26|25|24|23|22|21|20|19|18|17|16|15|14|13|12|11|10|9|8|7|6|5|4|3|2|1|

        // BIT 32
        //Parity
        public byte GetP()
        {
            return A429Message[0];
        }
        public string SetP(string)
        //BITS 31 and 30 (2 Bits)
        //Sign/Status Matrix
        //| Bit 31 | Bit 30 | Meaning BCD                           | Meaning BNR           | Meaning Discrete |
        //|    0   |    0   | Plus, North, East, Right, To, Above   | Failure Warning (FW)  | NO               |
        //|    0   |    1   | No Computed Data (NCD)                | NCD                   | NCD              |
        //|    1   |    0   | Functional Test (FT)                  | FT                    | FT               |
        //|    1   |    1   | Minus, South, West, Left, From, Below | Normal Operation (NO) | FW               |
        string SSM { get; set; }
        //BITS 29 to 11 (19 Bits)
        //BNR ONLY -- Bit 29 is the Sign Matrix -- Bit 28->11 are the data
        //| Bit 29 | Meaning BNR                           |
        //|    0   | Plus, North, East, Right, Above       |
        //|    1   | Minus, South, West, Left, From, Below |
        //ALL OTHER Bit 29 -> 11
        string Data { get; set; }
        //BITS 10 and 9 (2 Bits)
        // Source-Destination Identifier (Transmiting system ID)
        string SDI { get; set; }
        //BITS 8 to 1 (8 Bits)
        //Label
        string Label { get; set; }
        //FULL ARINC MESSAGE (32 Bits)
        uint A429Message { get; set; }

        #endregion Parameters
    }
}
