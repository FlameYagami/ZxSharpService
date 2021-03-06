﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxWrapper.Enums
{
    public enum Query
    {
        Code = 0x01,
        Position = 0x02,
        Alias = 0x04,
        Type = 0x08,
        Cost = 0x10,
        Rank = 0x20,
        Attribute = 0x40,
        Race = 0x80,
        Power = 0x100,
        Defence = 0x200,
        BasePower = 0x400,
        BaseDefence = 0x800,
        Reason = 0x1000,
        ReasonCard = 0x2000,
        EquipCard = 0x4000,
        TargetCard = 0x8000,
        OverlayCard = 0x10000,
        Counters = 0x20000,
        Owner = 0x40000,
        IsDisabled = 0x80000,
        IsPublic = 0x100000,
    }
}
