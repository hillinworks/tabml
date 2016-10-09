﻿using System.Collections.Generic;
using TabML.Core.MusicTheory;
using TabML.Parser.Document;
using TabML.Parser.Parsing;

namespace TabML.Parser.AST
{
    class TimeSignatureCommandletNode : CommandletNode, IDocumentElementFactory<TimeSignature>
    {
        public LiteralNode<int> Beats { get; set; }
        public LiteralNode<BaseNoteValue> NoteValue { get; set; }

        protected override IEnumerable<Node> CommandletChildNodes
        {
            get
            {
                yield return this.Beats;
                yield return this.NoteValue;
            }
        }

        public bool ValueEquals(TimeSignature other)
        {
            if (other == null)
                return false;

            return this.Beats.Value == other.Time.Beats && this.NoteValue.Value == other.Time.NoteValue;
        }

        internal override bool Apply(TablatureContext context, IReporter reporter)
        {
            TimeSignature time;
            if (!this.ToDocumentElement(context, reporter, out time))
                return false;

            using (var state = context.AlterDocumentState())
            {
                state.Time = time;
            }

            return true;
        }

        public bool ToDocumentElement(TablatureContext context, IReporter reporter, out TimeSignature element)
        {
            if (context.DocumentState.RhythmTemplate != null || context.DocumentState.BarAppeared)
            {
                reporter.Report(ReportLevel.Error, this.Range, Messages.Error_TimeInstructionAfterBarAppearedOrRhythmInstruction);
                element = null;
                return false;
            }

            if (context.DocumentState.Time != null && this.ValueEquals(context.DocumentState.Time))
            {
                reporter.Report(ReportLevel.Suggestion, this.Range, Messages.Suggestion_UselessTimeInstruction);
                element = null;
                return false;
            }

            element = new TimeSignature
            {
                Range = this.Range,
                Time = new Time(this.Beats.Value, this.NoteValue.Value)
            };

            return true;
        }
    }
}
