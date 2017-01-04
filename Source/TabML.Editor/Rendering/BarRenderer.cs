﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TabML.Core.Document;
using TabML.Core.MusicTheory;
using TabML.Editor.Tablature;
using TabML.Editor.Tablature.Layout;

namespace TabML.Editor.Rendering
{
    class BarRenderer : ElementRenderer<Bar, RowRenderingContext>
    {
        public TablatureStyle Style { get; }
        public Point Location { get; private set; }
        public Size RenderSize { get; private set; }

        private double? _minSize;

        private readonly List<BarColumnRenderer> _columnRenderers;
        private readonly List<BarVoiceRenderer> _voiceRenderers;

        public BarRenderer(TablatureRenderer owner, TablatureStyle style, Bar bar)
            : base(owner, bar)
        {
            this.Style = style;

            _columnRenderers = new List<BarColumnRenderer>();
            _voiceRenderers = new List<BarVoiceRenderer>();
        }

        public override void Initialize()
        {
            base.Initialize();

            _columnRenderers.AddRange(this.Element.Columns.Select(c => new BarColumnRenderer(this, c)));

            _columnRenderers.Initialize();

            if (this.Element.BassVoice != null)
                _voiceRenderers.Add(new BarVoiceRenderer(this, this.Element.BassVoice));

            if (this.Element.TrebleVoice != null)
                _voiceRenderers.Add(new BarVoiceRenderer(this, this.Element.TrebleVoice));

            _voiceRenderers.Initialize();
        }

        public async Task<double> MeasureMinSize(PrimitiveRenderer primitiveRenderer)
        {
            if (_minSize != null)
                return _minSize.Value;

            var minDuration = this.Element.Columns.Min(c => c.GetDuration());
            var size = 0.0;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var column in this.Element.Columns)
                size += await this.GetColumnMinWidthInBar(primitiveRenderer, column, minDuration);

            _minSize = size;

            return _minSize.Value;
        }

        private async Task<double> GetColumnMinWidthInBar(PrimitiveRenderer primitiveRenderer, BarColumn column, PreciseDuration minDurationInBar)
        {
            var columnRegularWidth = Math.Min(this.Style.MaximumBeatSizeWithoutLyrics,
                                              this.Style.MinimumBeatSize * column.GetDuration() / minDurationInBar);

            double columnMinWidth;
            if (column.Lyrics == null)
                columnMinWidth = this.Style.MinimumBeatSize;
            else
            {
                
                var lyricsBounds = await primitiveRenderer.MeasureLyrics(column.Lyrics.Text);
                columnMinWidth = Math.Max(this.Style.MinimumBeatSize, lyricsBounds.Width);
            }

            return Math.Max(columnRegularWidth, columnMinWidth);
        }

        public async Task Render(Point location, Size size)
        {
            this.Location = location;
            this.RenderSize = size;

            var renderingContext = new BarRenderingContext(this.RenderingContext, location, size);
            _columnRenderers.AssignRenderingContexts(renderingContext);
            _voiceRenderers.AssignRenderingContexts(renderingContext);

            var width = size.Width;
            if (this.Element.OpenLine != null)
                renderingContext.DrawBarLine(this.Element.OpenLine.Value, 0.0);

            var minDuration = this.Element.Columns.Min(c => c.GetDuration());
            var widthRatio = (width - renderingContext.Style.BarHorizontalPadding * 2) /  await this.MeasureMinSize(this.RenderingContext.PrimitiveRenderer);

            var position = renderingContext.Style.BarHorizontalPadding;

            renderingContext.ColumnRenderingInfos = new BarColumnRenderingInfo[this.Element.Columns.Count];

            for (var i = 0; i < this.Element.Columns.Count; i++)
            {
                var column = this.Element.Columns[i];

                var columnWidth = await this.GetColumnMinWidthInBar(this.RenderingContext.PrimitiveRenderer, column, minDuration) * widthRatio;
                renderingContext.ColumnRenderingInfos[i] = new BarColumnRenderingInfo(column, position, columnWidth);

                var barColumnRenderer = _columnRenderers[i];
                barColumnRenderer.PreRender();
                position += columnWidth;
            }

            foreach (var renderer in _voiceRenderers)
                await renderer.Render();

            if (this.Element.CloseLine != null)
                renderingContext.DrawBarLine(this.Element.CloseLine.Value, width);

        }

        public async Task PostRender()
        {
            foreach (var barColumnRenderer in _columnRenderers)
                await barColumnRenderer.PostRender();
        }
    }
}
