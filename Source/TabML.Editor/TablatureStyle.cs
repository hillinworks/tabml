﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TabML.Core.Document;

namespace TabML.Editor
{
    class TablatureStyle
    {
        public int StringCount { get; set; } = 6;
        public double MinimumBeatSize { get; set; } = 16;
        public double MaximumBeatSizeWithoutLyrics { get; set; } = 24;
        public double BarLineHeight { get; set; } = 12;
        public double BarTopMargin { get; set; } = 120;
        public double BarBottomMargin { get; set; } = 60;
        public double BarHorizontalPadding { get; set; } = 32;

        public int RegularBarsPerRow { get; set; } = 3;
        public double FirstRowIndention { get; set; } = 48;

        public Thickness Padding { get; set; } = new Thickness(24);
        
        public Typeface LyricsTypeface { get; set; } = new Typeface("Segoe UI");
        public double LyricsFontSize { get; set; } = 12;
        public Brush LyricsForeground { get; set; } = Brushes.Black;

        public double NoteStemOffset { get; set; } = 2;
        public double MinimumNoteTailOffset { get; set; } = 16;
        public double NoteStemHeight { get; set; } = 24;
        public double NoteAlternationOffset { get; set; } = 10;


        public double BeamThickness { get; set; } = 4;
        public double BeamSpacing { get; set; } = 4;
        public double SemiBeamWidth { get; set; } = 12;
        public double NoteValueAugmentOffset { get; set; } = 8;

        public double OuterNoteInstructionOffset { get; set; } = 12;
        public double TieInstructionOffset { get; set; } = 24;

        public FormattedText MakeFormattedLyrics(string lyricsText)
        {
            return new FormattedText(lyricsText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                     this.LyricsTypeface, this.LyricsFontSize, this.LyricsForeground);
        }
    }
}
