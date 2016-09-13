﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabML.Core.MusicTheory
{
    public struct TempoSignature
    {
        public BaseNoteValue NoteValue { get; }
        public int Beats { get; }

        public TempoSignature(int beats, BaseNoteValue noteValue = BaseNoteValue.Quater)
        {
            this.Beats = beats;
            this.NoteValue = noteValue;
        }
    }
}
