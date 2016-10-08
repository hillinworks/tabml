﻿using System.Collections.Generic;
using System.Linq;
using TabML.Parser.Document;
using TabML.Parser.Parsing;

namespace TabML.Parser.AST
{
    class RhythmTemplateNode : Node, IValueEquatable<RhythmTemplateNode>, IDocumentElementFactory<Rhythm>
    {
        public List<RhythmTemplateSegmentNode> Segments { get; }

        public RhythmTemplateNode()
        {
            this.Segments = new List<RhythmTemplateSegmentNode>();
        }

        public override IEnumerable<Node> Children => this.Segments;

        public bool ValueEquals(RhythmTemplateNode other)
        {
            if (other == null)
                return false;

            return (other.Segments.Count != this.Segments.Count)
                && !this.Segments.Where((t, i) => !t.ValueEquals(other.Segments[i])).Any();
        }

        public bool ToDocumentElement(TablatureContext context, IReporter reporter, out Rhythm rhythm)
        {
            rhythm = new Rhythm();  // do not specify range

            foreach (var segment in this.Segments)
            {
                RhythmSegment rhythmSegment;
                if (!segment.ToDocumentElement(context, reporter, out rhythmSegment))
                    return false;

                rhythm.Segments.Add(rhythmSegment);
            }

            return true;
        }
    }
}
