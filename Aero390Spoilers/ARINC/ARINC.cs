using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ARINC
{
    public class ARINCMessage
    {
        #region ARINC Dictionaries
        public static IDictionary<string, BitArray> A429LabelLookup = new Dictionary<string, BitArray>()
        {
            {"LandingGear_Status", ToBitArray("00010000") },//DISCRETE
        };
        public static IDictionary<string, BitArray> A429DiscSSM = new Dictionary<string, BitArray>()
        {
            {"NO", ToBitArray("00") },//Normal Operation
            {"NCD", ToBitArray("01") },//No computed data
            {"FT", ToBitArray("10") },//Functional Test
            {"FW", ToBitArray("11") },//Failure Warning
        };
        public static IDictionary<string, BitArray> A429BcdSSM = new Dictionary<string, BitArray>()
        {
            {"Plus", ToBitArray("00") },//Plus, North, East, Right, To, Above
            {"NCD", ToBitArray("01") },//No computed data
            {"FT", ToBitArray("10") },//Functional Test
            {"Minus", ToBitArray("11") },//Minus, South, West, Left, From, Below
        };
        public static IDictionary<string, BitArray> A429SDI = new Dictionary<string, BitArray>()
        {
            {"FCC", ToBitArray("00") },//Plus, North, East, Right, To, Above
            {"GUI", ToBitArray("01") },//No computed data
            //{"FT", ToBitArray("10") },
            //{"Minus", ToBitArray("11") },
        };

        #endregion ARINC Dictionaries

        public ARINCMessage()
        {
            A429Message = new BitArray(32);
        }

        public ARINCMessage(string SSM, string Data, string SDI, string label, bool isDiscrete)
        {
            A429Message = new BitArray(32);
            //SSM
            BitArray iSSM = new BitArray(2);
            if (isDiscrete)
            {
                if (A429DiscSSM.TryGetValue(SSM, out iSSM))
                {
                    SetSSM(iSSM);
                }
                else
                {
                    throw new Exception("SSM Key not found!");
                }
            }
            else
            {
                if (A429BcdSSM.TryGetValue(SSM, out iSSM))
                {
                    SetSSM(iSSM);
                }
                else
                {
                    throw new Exception("SSM Key not found!");
                }
            }
            //DATA
            SetData(ToBitArray(Data));
            //SDI
            BitArray iSDI = new BitArray(2);
            if (A429SDI.TryGetValue(SDI, out iSDI))
            {
                SetSDI(iSDI);
            }
            else
            {
                throw new Exception("SDI Key not found!");
            }
            //LABEL
            BitArray iLabel = new BitArray(8);
            if (A429LabelLookup.TryGetValue(label, out iLabel))
            {
                SetLabel(iLabel);
            }
            else
            {
                throw new Exception("Label Key not found!");
            }
            SetP();
        }

        public override string ToString()
        {
            var oOutput = new StringBuilder();

            for (int i = 0; i < 32; i++)
            {
                oOutput.Append(A429Message[i] ? '1' : '0');
            }

            return oOutput.ToString();
        }

        public static BitArray ToBitArray(string stringMessage)
        {
            int aLength = stringMessage.Length;
            BitArray temp = new BitArray(aLength);
            for (int i = 0; i < aLength; i++)
            {
                temp[i] = (stringMessage[i] == '1');
            }
            return temp;
        }

        #region Parameters

        //|        ARINC 429 Word Format                                                         |
        //|P |SSM  |MSB                     Data                         LSB|SDI |Label          |
        //|32|31|30|29|28|27|26|25|24|23|22|21|20|19|18|17|16|15|14|13|12|11|10|9|8|7|6|5|4|3|2|1|

        // BIT 32
        //Parity
        public BitArray GetP()
        {
            BitArray oParity = new BitArray(1);
            oParity[0] = A429Message[0];
            return oParity;
        }
        public void SetP()//Recheck Parity
        {
            int bitSum = 0;
            for (int i = 1; i < 32; i++)
            {
                if (A429Message[i])
                {
                    bitSum++;
                }
            }
            if (bitSum % 2 == 0) A429Message[0] = false;
            else A429Message[0] = true;
        }

        //BITS 31 and 30 (2 Bits)
        //Sign/Status Matrix
        //| Bit 31 | Bit 30 | Meaning BCD                           | Meaning BNR           | Meaning Discrete |
        //|    0   |    0   | Plus, North, East, Right, To, Above   | Failure Warning (FW)  | NO               |
        //|    0   |    1   | No Computed Data (NCD)                | NCD                   | NCD              |
        //|    1   |    0   | Functional Test (FT)                  | FT                    | FT               |
        //|    1   |    1   | Minus, South, West, Left, From, Below | Normal Operation (NO) | FW               |
        public BitArray GetSSM()
        {
            BitArray oSSM = new BitArray(2);
            oSSM[0] = A429Message[1];
            oSSM[1] = A429Message[2];
            return oSSM;
        }
        public void SetSSM(BitArray iSSM)
        {
            A429Message[1] = iSSM[0];
            A429Message[2] = iSSM[1];
        }

        //BITS 29 to 11 (19 Bits)
        //BNR ONLY -- Bit 29 is the Sign Matrix -- Bit 28->11 are the data
        //| Bit 29 | Meaning BNR                           |
        //|    0   | Plus, North, East, Right, Above       |
        //|    1   | Minus, South, West, Left, From, Below |
        //ALL OTHER Bit 29 -> 11
        public BitArray GetData()
        {
            BitArray oData = new BitArray(19);
            for (int i = 3; i < 22; i++)
            {
                oData[i - 3] = A429Message[i];
            }
            return oData;
        }
        public void SetData(BitArray iData)
        {
            for (int i = 3; i < 22; i++)
            {
                A429Message[i] = iData[i - 3];
            }
        }

        //BITS 10 and 9 (2 Bits)
        // Source-Destination Identifier (Transmiting system ID)
        public BitArray GetSDI()
        {
            BitArray oSDI = new BitArray(2);
            oSDI[0] = A429Message[22];
            oSDI[1] = A429Message[23];
            return oSDI;
        }
        public void SetSDI(BitArray iSDI)
        {
            A429Message[22] = iSDI[0];
            A429Message[23] = iSDI[1];
        }

        //BITS 8 to 1 (8 Bits)
        //Label
        public BitArray GetLabel()
        {
            BitArray oLabel = new BitArray(19);
            for (int i = 24; i < 32; i++)
            {
                oLabel[i - 24] = A429Message[i];
            }
            return oLabel;
        }
        public void SetLabel(BitArray iLabel)
        {
            for (int i = 24; i < 32; i++)
            {
                A429Message[i] = iLabel[i - 24];
            }
        }

        //FULL ARINC MESSAGE (32 Bits)
        BitArray A429Message { get; set; }

        #endregion Parameters
    }
}
