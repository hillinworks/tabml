﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TabML.Core.Logging;
using TabML.Core.MusicTheory;
using TabML.Core.Document;
using TabML.Core.MusicTheory.String.Plucked;
using TabML.Parser.Parsing;

namespace TabML.Parser.AST
{
    [DebuggerDisplay("bar: {Range.Content}")]
    class BarNode : TopLevelNode
    {
        public LiteralNode<OpenBarLine> OpenLine { get; set; }
        public LiteralNode<CloseBarLine> CloseLine { get; set; }
        public RhythmNode Rhythm { get; set; }
        public LyricsNode Lyrics { get; set; }

        public override IEnumerable<Node> Children
        {
            get
            {
                if (this.OpenLine != null)
                    yield return this.OpenLine;
                if (this.Rhythm != null)
                    yield return this.Rhythm;
                if (this.Lyrics != null)
                    yield return this.Lyrics;
                if (this.CloseLine != null)
                    yield return this.CloseLine;
            }
        }

        internal override bool Apply(TablatureContext context, ILogger logger)
        {
            Bar bar;
            if (!this.ToDocumentElement(context, logger, null, out bar))
                return false;

            if (bar.Rhythm != null && bar.Lyrics != null)
            {
                var beats = bar.Rhythm.Segments.Sum(s => s.FirstVoice.Beats?.Count ?? 0);
                if (beats < bar.Lyrics.Segments.Count)
                    logger.Report(LogLevel.Suggestion, bar.Lyrics.Range, Messages.Suggestion_LyricsTooLong);
            }

            context.AddBar(bar);

            // check if this bar terminates an alternative ending, must be done AFTER adding this bar to context
            if ((bar.CloseLine == CloseBarLine.End || bar.CloseLine == CloseBarLine.EndRepeat)
                && context.DocumentState.CurrentAlternation != null)
            {
                using (var state = context.AlterDocumentState())
                {
                    state.CurrentAlternation = null;
                }
            }
            
            return true;
        }

        public bool ToDocumentElement(TablatureContext context, ILogger logger, Bar template, out Bar bar)
        {
            bar = new Bar
            {
                OpenLine = this.OpenLine?.Value,
                CloseLine = this.CloseLine?.Value,
                Range = this.Range
            };

            var previousBar = context.CurrentBar;
            context.CurrentBar = bar;
            
            if (this.Rhythm == null)
            {
                if (template != null)
                    bar.Rhythm = template.Rhythm.Clone();
            }
            else
            {
                Rhythm rhythm;
                if (!this.Rhythm.ToDocumentElement(context, logger, out rhythm))
                {
                    bar = null;
                    return false;
                }

                bar.Rhythm = rhythm;
            }

            if (context.DocumentState.RhythmTemplate != null)
                bar.Rhythm = context.DocumentState.RhythmTemplate.Apply(bar.Rhythm, logger);

            if (this.Lyrics == null)
                bar.Lyrics = null;
            else
            {
                Lyrics lyrics;
                if (!this.Lyrics.ToDocumentElement(context, logger, out lyrics))
                {
                    bar = null;
                    return false;
                }

                bar.Lyrics = lyrics;
            }


            new BarArranger(context, bar).Arrange();

            foreach (var column in bar.Columns)
            {
                if (!this.ValidateColumn(context, logger, column))
                    return false;
            }

            if (previousBar != null)
            {
                this.ConnectBars(previousBar, bar, VoicePart.Treble);
                this.ConnectBars(previousBar, bar, VoicePart.Bass);
            }

            return true;
        }

        public bool ValidateColumn(TablatureContext context, ILogger logger, BarColumn column)
        {
            if (column.VoiceBeats.Count == 2 && column.VoiceBeats.All(b => b.StrumTechnique != StrumTechnique.None))
            {
                if (column.VoiceBeats[0].StrumTechnique != column.VoiceBeats[1].StrumTechnique)
                {
                    logger.Report(LogLevel.Warning, column.VoiceBeats[1].Range, Messages.Warning_ConflictedStrumTechniques);

                    column.VoiceBeats[1].StrumTechnique = StrumTechnique.None;
                }
            }
            
            return true;
        }

        private void ConnectBars(Bar previousBar, Bar bar, VoicePart voicePart)
        {
            var firstBeat = bar.GetVoice(voicePart)?.GetFirstBeat();
            var lastBeat = previousBar.GetVoice(voicePart)?.GetLastBeat();

            if (firstBeat == null || lastBeat == null)
                return;

            firstBeat.PreviousBeat = lastBeat;
            lastBeat.NextBeat = firstBeat;
        }
    }
}
