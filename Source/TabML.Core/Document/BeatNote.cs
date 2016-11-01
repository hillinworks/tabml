﻿using TabML.Core.MusicTheory;

namespace TabML.Core.Document
{
    public class BeatNote : Element
    {
        public const int UnspecifiedFret = -1;

        public int String { get; set; }
        public int Fret { get; set; }
        public NoteEffectTechnique EffectTechnique { get; set; }
        public double EffectTechniqueParameter { get; set; }
        public PreNoteConnection PreConnection { get; set; }
        public PostNoteConnection PostConnection { get; set; }

        public void ClearRange()
        {
            this.Range = null;
        }

        public BeatNote Clone()
        {
            return (BeatNote)this.MemberwiseClone();
        }
    }
}
