﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TabML.Core.Document;
using TabML.Core.MusicTheory;
using TabML.Core.Style;

namespace TabML.Editor.Rendering
{
    class RowRenderingContext : RenderingContextBase<TablatureRenderingContext>
    {
        private const int HeightMapSampleRate = 1;
        
        private readonly GapCollection[] _stringBarlineGaps;

        private readonly Dictionary<VoicePart, HeightMap> _heightMaps;
        public Point Location { get; }
        public Size AvailableSize { get; }

        public Point BottomRight => this.Location + new Vector(this.AvailableSize.Width, this.AvailableSize.Height);
        public PrimitiveRenderer PrimitiveRenderer => this.Owner.PrimitiveRenderer;
        public TablatureStyle Style => this.Owner.Style;
        public TablatureRenderingContext TablatureRenderingContext => this.Owner;

        // used for bar rendering to determine if the document state is changed
        public DocumentState PreviousDocumentState { get; set; }
        
        public double HeaderWidth { get; set; }


        public RowRenderingContext(TablatureRenderingContext owner, Point location, Size availableSize)
            : base(owner)
        {
            this.Location = location;
            this.AvailableSize = availableSize;

            this.PreviousDocumentState = owner.PreviousDocumentState;
            
            _stringBarlineGaps = new GapCollection[this.Style.StringCount];

            for (var i = 0; i < this.Style.StringCount; ++i)
                _stringBarlineGaps[i] = new GapCollection();

            _heightMaps = new Dictionary<VoicePart, HeightMap>
            {
                {VoicePart.Bass, this.CreatehHeightMap(availableSize)},
                {VoicePart.Treble, this.CreatehHeightMap(availableSize)},
            };
        }

        private HeightMap CreatehHeightMap(Size availableSize)
        {
            return new HeightMap((int)Math.Ceiling(availableSize.Width), HeightMapSampleRate, this.Style.MinimumNoteTailOffset + this.Style.NoteTailVerticalMargin);
        }

        public HeightMap GetHeightMap(VoicePart voicePart)
        {
            return _heightMaps[voicePart];
        }

        public double GetRelativeX(double position)
        {
            return position - this.Location.X;
        }

        public double GetRelativeY(double position)
        {
            return position - this.Location.Y;
        }

        public void UpdateHorizontalBarLine(int stringIndex, double left, double right)
        {
            var gapList = _stringBarlineGaps[stringIndex];

            gapList.Add(left, right);
        }

        public void FinishHorizontalBarLines(double width)
        {
            for (var i = 0; i < _stringBarlineGaps.Length; ++i)
            {
                var x = 0.0;

                var y = this.GetStringPosition(i);
                foreach (var gap in _stringBarlineGaps[i])
                {
                    this.PrimitiveRenderer.DrawHorizontalBarLine(this.Location.X + x, y, gap.Left - x);
                    x = gap.Right;
                }

                if (x < width)
                    this.PrimitiveRenderer.DrawHorizontalBarLine(this.Location.X + x, y, width - x);
            }
        }

        /// <summary>
        /// Gets the absolute location of the top-most bar line
        /// </summary>
        public double GetBodyCeiling() => this.Location.Y + this.Style.BarTopMargin;

        /// <summary>
        /// Gets the absolute location of the bottom-most bar line
        /// </summary>
        public double GetBodyFloor() => this.GetBodyCeiling() + this.Style.BarLineHeight * this.Style.StringCount;

        public double GetBodyCenter() => this.GetStringSpacePosition(this.Style.StringCount / 2.0);

        public double GetStringPosition(double stringIndex) => this.Location.Y + this.Style.BarTopMargin + (stringIndex + 0.5) * this.Style.BarLineHeight;
        public double GetStringSpacePosition(double stringIndex) => this.Location.Y + this.Style.BarTopMargin + stringIndex * this.Style.BarLineHeight;


        // from and to are absolute positions
        public Task<Rect> DrawTie(double from, double to, int stringIndex, Core.Style.VerticalDirection tiePosition, string instruction,
                                  double instructionY)
        {
            var spaceIndex = tiePosition == Core.Style.VerticalDirection.Under ? stringIndex + 1 : stringIndex;
            var y = this.GetStringSpacePosition(spaceIndex);
            return this.PrimitiveRenderer.DrawTie(from, to, y, tiePosition.ToOffBarDirection());
        }

        /// <summary>
        /// Ensure height spanned between <paramref name="x0" /> and <paramref name="x1" /> in the 
        /// height map by selecting between <paramref name="y0" /> and <paramref name="y1" /> according to 
        /// the specified <paramref name="voicePart"/>
        /// </summary>
        /// <remarks>All coordinates are absolute</remarks>
        public void EnsureHeight(VoicePart voicePart, double x0, double x1, double y0, double y1, double vMargin = 0)
        {
            double height;
            switch (voicePart)
            {
                case VoicePart.Treble:
                    height = this.GetBodyCeiling() - Math.Min(y0, y1);
                    break;
                case VoicePart.Bass:
                    height = Math.Max(y0, y1) - this.GetBodyFloor();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(voicePart), voicePart, null);
            }

            _heightMaps[voicePart].EnsureHeight(this.GetRelativeX(x0), x1 - x0, height + vMargin);
        }

        public void EnsureHeight(VoicePart voicePart, Rect bounds)
        {
            this.EnsureHeight(voicePart, bounds.Left, bounds.Right, bounds.Top, bounds.Bottom);
        }

        /// <summary>
        /// Ensure height spanned between <paramref name="x0" /> and <paramref name="x1" /> in the 
        /// height map by lerping between <paramref name="y0" /> and <paramref name="y1" />.
        /// </summary>
        /// <remarks>All coordinates are absolute</remarks>
        public void EnsureHeightSloped(VoicePart voicePart, double x0, double x1, double y0, double y1, double vMargin, double hMargin)
        {
            switch (voicePart)
            {
                case VoicePart.Treble:
                    y0 = this.GetBodyCeiling() - y0;
                    y1 = this.GetBodyCeiling() - y1;
                    break;
                case VoicePart.Bass:
                    y0 = y0 - this.GetBodyFloor();
                    y1 = y1 - this.GetBodyFloor();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(voicePart), voicePart, null);
            }

            _heightMaps[voicePart].EnsureHeight(this.GetRelativeX(x0), x1 - x0, y0 + vMargin, y1 + vMargin, hMargin);
        }

        public double GetHeight(VoicePart voicePart, double x)
        {
            switch (voicePart)
            {
                case VoicePart.Treble:
                    return this.GetBodyCeiling() - _heightMaps[voicePart].GetHeight(this.GetRelativeX(x));
                case VoicePart.Bass:
                    return this.GetBodyFloor() + _heightMaps[voicePart].GetHeight(this.GetRelativeX(x));
                default:
                    throw new ArgumentOutOfRangeException(nameof(voicePart), voicePart, null);
            }
        }

        public void DebugDrawHeightMaps()
        {
            var ceiling = this.GetBodyCeiling();
            this.DebugDrawHeightMap(_heightMaps[VoicePart.Treble], h => ceiling - h);

            var floor = this.GetBodyFloor();
            this.DebugDrawHeightMap(_heightMaps[VoicePart.Bass], h => floor + h);
        }

        private void DebugDrawHeightMap(HeightMap heightMap, Func<double, double> heightConverter)
        {
            this.PrimitiveRenderer.DebugDrawHeightMap(heightMap.DebugGetVertices().Select(p => new Point(p.X + this.Location.X, heightConverter(p.Y))));
        }

        public void SealHeightMaps()
        {
            foreach (var heightMap in _heightMaps)
                heightMap.Value.Seal();
        }

        public void BeginPostRender()
        {
            this.SealHeightMaps();
            this.PreviousDocumentState = this.Owner.PreviousDocumentState;
        }

    }
}
