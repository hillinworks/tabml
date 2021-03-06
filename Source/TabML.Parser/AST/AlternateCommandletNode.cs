﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabML.Core.Logging;
using TabML.Core.MusicTheory;
using TabML.Core.Document;
using TabML.Parser.Parsing;

namespace TabML.Parser.AST
{
    internal class AlternateCommandletNode : CommandletNode
    {
        public List<LiteralNode<string>> AlternationTexts { get; }

        protected override IEnumerable<Node> CommandletChildNodes => this.AlternationTexts;

        public AlternateCommandletNode()
        {
            this.AlternationTexts = new List<LiteralNode<string>>();
        }

        internal override bool Apply(TablatureContext context, ILogger logger)
        {
            Alternation alternation;
            if (!this.ToDocumentElement(context, logger, out alternation))
                return false;

            if (context.DocumentState.AlternationTextExplicity != Explicity.NotSpecified &&
                alternation.Explicity != context.DocumentState.AlternationTextExplicity)
            {
                logger.Report(LogLevel.Warning, this.Range.To.AsRange(),
                              Messages.Warning_InconsistentAlternationTextExplicity);
            }

            using (var state = context.AlterDocumentState())
            {
                foreach (var index in alternation.Indices)
                    state.DefinedAlternationIndices.Add(index);

                state.CurrentAlternation = alternation;
                state.AlternationTextType = alternation.TextType;
                state.AlternationTextExplicity = alternation.Explicity;
            }

            return true;
        }

        public bool ToDocumentElement(TablatureContext context, ILogger logger, out Alternation element)
        {

            if (this.AlternationTexts.Count == 0) // implicit
            {
                var implicitIndex = context.DocumentState.DefinedAlternationIndices.Max() + 1;
                element = new Alternation
                {
                    Range = this.Range,
                    TextType = context.DocumentState.AlternationTextType ?? AlternationTextType.Arabic,
                    Explicity = Explicity.Implicit,
                    Indices = new[] { implicitIndex }
                };

                return true;
            }

            element = new Alternation
            {
                Range = this.Range,
                Explicity = Explicity.Explicit
            };

            var referenceTextType = context.DocumentState.AlternationTextType;
            var indices = new List<int>();
            foreach (var alternationText in this.AlternationTexts)
            {
                int index;
                AlternationTextType textType;
                Debug.Assert(AlternationText.TryParse(alternationText.Value,
                                                      out index, out textType));

                if (referenceTextType != null && referenceTextType.Value != textType)
                {
                    logger.Report(LogLevel.Warning, alternationText.Range,
                                    Messages.Warning_InconsistentAlternationTextType);
                }
                else
                    referenceTextType = textType;

                if (context.DocumentState.DefinedAlternationIndices.Contains(index))
                {
                    logger.Report(LogLevel.Error, alternationText.Range,
                                    Messages.Error_DuplicatedAlternationText, alternationText.Value);

                    element = null;
                    return false;
                }

                indices.Add(index);
            }

            Debug.Assert(referenceTextType != null, "referenceTextType != null");
            element.TextType = referenceTextType.Value;

            element.Indices = indices.ToArray();

            return true;
        }
    }
}
