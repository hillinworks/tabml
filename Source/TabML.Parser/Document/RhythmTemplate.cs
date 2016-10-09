﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabML.Parser.Parsing;

namespace TabML.Parser.Document
{
    class RhythmTemplate : Element
    {
        public List<RhythmTemplateSegment> Segments { get; }

        public RhythmTemplate()
        {
            this.Segments = new List<RhythmTemplateSegment>();
        }

        public Rhythm Instantialize()
        {
            var rhythm = new Rhythm();  // do not set Range
            rhythm.Segments.AddRange(this.Segments.Select(s => s.Instantialize()));
            return rhythm;
        }

        public Rhythm Apply(Rhythm rhythm, IReporter reporter)
        {
            var templateInstance = this.Instantialize();

            if (rhythm == null)
                return templateInstance;

            if (rhythm.Segments.Count == 0) // empty rhythm, should be filled with rest
                return rhythm;

            if (rhythm.Segments.Any(s => s.Voices.Count != 0))  // rhythm already defined
                return rhythm;

            if (rhythm.Segments.Count > templateInstance.Segments.Count)
            {
                reporter.Report(ReportLevel.Warning, rhythm.Range,
                                Messages.Warning_TooManyChordsToMatchRhythmTemplate);

                for (var i = 0; i < templateInstance.Segments.Count; ++i)
                {
                    rhythm.Segments[i].Voices.AddRange(templateInstance.Segments[i].Voices);
                }

                for (var i = templateInstance.Segments.Count; i < rhythm.Segments.Count; ++i)
                {
                    rhythm.Segments[i].IsOmittedByTemplate = true;
                }
            }
            else if (rhythm.Segments.Count < templateInstance.Segments.Count && rhythm.Segments.Count != 1)
            {
                reporter.Report(ReportLevel.Warning, rhythm.Range,
                                Messages.Warning_InsufficientChordsToMatchRhythmTemplate);

                var lastChord = rhythm.Segments[rhythm.Segments.Count - 1].Chord;

                for (var i = 0; i < rhythm.Segments.Count; ++i)
                {
                    rhythm.Segments[i].Voices.AddRange(templateInstance.Segments[i].Voices);
                }

                for (var i = rhythm.Segments.Count; i < templateInstance.Segments.Count; ++i)
                {
                    var segment = templateInstance.Segments[i];
                    segment.Chord = lastChord;
                    rhythm.Segments.Add(segment);
                }
            }
            else
            {
                for (var i = 0; i < templateInstance.Segments.Count; ++i)
                {
                    rhythm.Segments[i].Voices.AddRange(templateInstance.Segments[i].Voices);
                }
            }

            return rhythm;
        }
    }
}