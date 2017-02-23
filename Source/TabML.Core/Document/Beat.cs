﻿using System;
using System.Collections.Generic;
using System.Linq;
using TabML.Core.MusicTheory;

namespace TabML.Core.Document
{
    public class Beat : Element, IInternalBeatElement
    {
        public NoteValue NoteValue { get; set; }
        public BeatNote[] Notes { get; set; }
        public bool IsRest { get; set; }
        public bool IsTied { get; set; }
        public StrumTechnique StrumTechnique { get; set; } = StrumTechnique.None;

        public bool HasBrushLikeStrumTechnique =>
            this.StrumTechnique == StrumTechnique.ArpeggioDown
            || this.StrumTechnique == StrumTechnique.ArpeggioUp
            || this.StrumTechnique == StrumTechnique.BrushDown
            || this.StrumTechnique == StrumTechnique.BrushUp
            || this.StrumTechnique == StrumTechnique.Rasgueado;

        public BeatEffectTechnique EffectTechnique { get; set; } = BeatEffectTechnique.None;
        public double EffectTechniqueParameter { get; set; }
        public BeatDurationEffect DurationEffect { get; set; } = BeatDurationEffect.None;
        public BeatAccent Accent { get; set; } = BeatAccent.Normal;
        public Beat PreviousBeat { get; set; }
        public Beat NextBeat { get; set; }
        public PreciseDuration Position { get; set; }
        public BarColumn OwnerColumn { get; set; }
        public Beam OwnerBeam => this.BeatElementOwner as Beam;
        internal IBeatElementContainer BeatElementOwner { get; private set; }
        public Bar OwnerBar => this.BeatElementOwner.OwnerBar;
        public VoicePart VoicePart { get; set; }

        public PreBeatConnection PreConnection { get; set; }
        public PostBeatConnection PostConnection { get; set; }
        public TiePosition? TiePosition { get; set; }

        /// <summary>
        /// Get the beat to which this beat is tied. If this beat is not tied, the value is null.
        /// </summary>
        public Beat TieHead
        {
            get
            {
                if (!this.IsTied)
                    return null;

                return this.PreviousBeat.IsTied ? this.PreviousBeat.TieHead : this.PreviousBeat;
            }
        }

        /// <summary>
        /// Get the beat which defines the nodes for this beat. As in, if this beat is tied,
        /// the tie head will be the notes definer
        /// </summary>
        public Beat NotesDefiner => this.IsTied ? this.TieHead : this;

        public override IEnumerable<Element> Children
        {
            get
            {
                if (this.Notes == null)
                    yield break;

                foreach (var note in this.Notes)
                    yield return note;
            }
        }

        public bool IsForceBeamStart { get; set; }
        public bool IsForceBeamEnd { get; set; }

        public Beat()
        {
        }

        public Beat GetTieHead()
        {
            var beat = this;
            while (beat.IsTied && beat.PreviousBeat != null)
                beat = beat.PreviousBeat;

            return beat;
        }

        public PreciseDuration GetDuration()
        {
            return this.NoteValue.GetDuration();
        }

        public void ClearRange()
        {
            this.Range = null;
            foreach (var note in this.Notes)
                note.ClearRange();
        }

        public Beat Clone()
        {
            return new Beat
            {
                Range = this.Range,
                NoteValue = this.NoteValue,
                IsRest = this.IsRest,
                IsTied = this.IsTied,
                PreConnection = this.PreConnection,
                TiePosition = this.TiePosition,
                PostConnection = this.PostConnection,
                StrumTechnique = this.StrumTechnique,
                EffectTechnique = this.EffectTechnique,
                EffectTechniqueParameter = this.EffectTechniqueParameter,
                DurationEffect = this.DurationEffect,
                Accent = this.Accent,
                Notes = this.Notes?.Select(n => n.Clone()).ToArray(),
                BeatElementOwner = this.BeatElementOwner,
                VoicePart = this.VoicePart
            };
        }

        void IInternalBeatElement.SetOwner(IBeatElementContainer owner)
        {
            this.BeatElementOwner = owner;
        }

        IInternalBeatElement IInternalBeatElement.Clone() => this.Clone();
    }
}
