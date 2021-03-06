﻿using System;
using System.Diagnostics;
using System.Text;

namespace TabML.Core.MusicTheory
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public struct NoteValue : IComparable<NoteValue>
    {
        public static bool TryResolveFromDuration(PreciseDuration duration, out NoteValue noteValue, bool complex = false)
        {
            for (var baseNoteValue = BaseNoteValue.Large; baseNoteValue >= BaseNoteValue.TwoHundredFiftySixth; --baseNoteValue)
            {
                if (duration == baseNoteValue.GetDuration())
                    continue;

                noteValue = new NoteValue(baseNoteValue);
                return true;
            }

            if (complex)
            {
                var searchAugments = new[] { NoteValueAugment.Dot, NoteValueAugment.TwoDots, NoteValueAugment.ThreeDots };
                var searchTuplets = new[] { 3, 5, 6, 7, 9, 10, 11, 12, 13, 14, 15 };

                for (var baseNoteValue = BaseNoteValue.Large; baseNoteValue >= BaseNoteValue.TwoHundredFiftySixth; --baseNoteValue)
                {
                    var baseDuration = baseNoteValue.GetDuration();
                    var baseInvertedDuration = (double)baseNoteValue.GetInvertedDuration();

                    foreach (var augment in searchAugments)
                    {
                        var augmentedDuration = baseDuration * augment.GetDurationMultiplier();
                        if (duration == augmentedDuration)
                        {
                            noteValue = new NoteValue(baseNoteValue, augment);
                            return true;
                        }

                        foreach (var tuplet in searchTuplets)
                        {
                            if (duration == augmentedDuration * (baseInvertedDuration / tuplet))
                            {
                                noteValue = new NoteValue(baseNoteValue, augment, tuplet);
                                return true;
                            }
                        }
                    }
                }
            }

            noteValue = default(NoteValue);
            return false;
        }

        public static double GetTupletMultiplier(int? tuplet)
        {
            if (tuplet == null)
                return 1.0;

            // even tuplet numbers are treated according to https://en.wikipedia.org/wiki/Tuplet
            if (tuplet.Value % 2 == 0)
                return 3.0 / tuplet.Value;

            return 2.0 / tuplet.Value;
        }

        public static bool IsValidTuplet(int tuplet)
        {
            return tuplet >= 2 && tuplet <= 64;
        }

        public BaseNoteValue Base { get; }
        public NoteValueAugment Augment { get; }
        public int? Tuplet { get; }

        public NoteValue(BaseNoteValue baseValue, NoteValueAugment augment = NoteValueAugment.None, int? tuplet = null)
        {
            if (tuplet != null)
            {
                if (!NoteValue.IsValidTuplet(tuplet.Value))
                    throw new ArgumentOutOfRangeException(nameof(tuplet), "invalid tuplet value");

                if (baseValue >= 0)
                    throw new ArgumentOutOfRangeException(nameof(tuplet),
                        "tuplet not supported for notes equal or longer than a whole");
            }

            this.Base = baseValue;
            this.Augment = augment;
            this.Tuplet = tuplet;
        }

        public PreciseDuration GetDuration()
        {
            return this.Base.GetDuration() * (this.Augment.GetDurationMultiplier() * NoteValue.GetTupletMultiplier(this.Tuplet));
        }


        public int CompareTo(NoteValue other)
        {
            return this.GetDuration().CompareTo(other.GetDuration());
        }

        public static bool operator ==(NoteValue n1, NoteValue n2)
        {
            return n1.CompareTo(n2) == 0;
        }

        public static bool operator !=(NoteValue n1, NoteValue n2)
        {
            return n1.CompareTo(n2) != 0;
        }

        public static bool operator >(NoteValue n1, NoteValue n2)
        {
            return n1.CompareTo(n2) > 0;
        }

        public static bool operator <(NoteValue n1, NoteValue n2)
        {
            return n1.CompareTo(n2) < 0;
        }

        public double GetBeats(BaseNoteValue beatLength)
        {
            return this.GetDuration() / beatLength.GetDuration();
        }

        public bool Equals(NoteValue other)
        {
            return this.Base == other.Base && this.Augment == other.Augment && this.Tuplet == other.Tuplet;
        }

        public override bool Equals(object obj)
        {
            if (null == obj)
                return false;

            return obj is NoteValue && this.Equals((NoteValue)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)this.Base;
                hashCode = (hashCode * 397) ^ (int)this.Augment;
                hashCode = (hashCode * 397) ^ this.Tuplet ?? 0;
                return hashCode;
            }
        }

#if DEBUG
        public string DebuggerDisplay
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append(this.Base);

                switch (this.Augment)
                {
                    case NoteValueAugment.Dot:
                        builder.Append("+1/2");
                        break;
                    case NoteValueAugment.TwoDots:
                        builder.Append("+3/4");
                        break;
                    case NoteValueAugment.ThreeDots:
                        builder.Append("+7/8");
                        break;
                }

                if (this.Tuplet != null)
                    builder.Append("/").Append(this.Tuplet);

                return builder.ToString();
            }
        }
#endif
    }
}
