using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxWrapper.Enums
{
    public enum CardPosition
    {
        Cost = 0x1,
        UpCost = 0x2,
        DownCost = 0x4,
        Power = 0x10,
        UpPower = 0x20,
        DownPower = 0x40
    }
}
