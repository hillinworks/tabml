﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabML.Core;
using TabML.Core.MusicTheory;

namespace TabML.Parser.Document
{
    public class Capo : Element
    {
        public CapoInfo CapoInfo { get; set; }

        public int[] OffsetFrets(int[] capoFretOffsets)
        {
            if (capoFretOffsets == null)
                capoFretOffsets = new int[Defaults.Strings];

            if (this.CapoInfo.AffectedStrings == null)
            {
                for (var i = 0; i < Defaults.Strings; ++i)
                    capoFretOffsets[i] = Math.Max(capoFretOffsets[i], this.CapoInfo.Position);
            }
            else
            {
                foreach (var stringIndex in this.CapoInfo.AffectedStrings)
                    capoFretOffsets[stringIndex - 1] = Math.Max(capoFretOffsets[stringIndex - 1], this.CapoInfo.Position);
            }

            return capoFretOffsets;
        }
    }
}
