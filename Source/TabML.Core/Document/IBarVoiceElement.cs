﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabML.Core.Document
{
    public interface IBarVoiceElement : IBarElement
    {
        VoicePart VoicePart { get; }
    }
}
