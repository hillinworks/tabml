﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabML.Core.Document
{
    interface IInternalBeatElement : IBeatElement
    {
        void SetOwner(IBeatElementContainer owner);
        IInternalBeatElement Clone();
    }
}
