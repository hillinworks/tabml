﻿using System.Collections.Generic;
using System.Linq;
using TabML.Core.MusicTheory;

namespace TabML.Core.Document
{
    public class Voice : Element
    {
        public List<IBeatElement> BeatElements { get; }
        public VoicePart Part { get; }

        public Voice(VoicePart part)
        {
            this.Part = part;
            this.BeatElements = new List<IBeatElement>();
        }
        public PreciseDuration GetDuration() => this.BeatElements.Sum(n => n.GetDuration());

        public void ClearRange()
        {
            this.Range = null;

            foreach (var beat in this.BeatElements)
                beat.ClearRange();
        }

        public Voice Clone()
        {
            var clone = new Voice(this.Part)
            {
                Range = this.Range,
            };
            clone.BeatElements.AddRange(this.BeatElements.Select(b => b.Clone()));
            return clone;
        }
    }
}
