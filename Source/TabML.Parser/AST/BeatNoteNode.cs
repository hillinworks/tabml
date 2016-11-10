﻿using System.Collections.Generic;
using TabML.Core.Logging;
using TabML.Core.MusicTheory;
using TabML.Core.Document;
using TabML.Parser.Parsing;

namespace TabML.Parser.AST
{
    class BeatNoteNode : Node, IDocumentElementFactory<BeatNote>
    {
        public LiteralNode<int> String { get; set; }
        public LiteralNode<int> Fret { get; set; }
        public LiteralNode<PreNoteConnection> PreConnection { get; set; }
        public LiteralNode<PostNoteConnection> PostConnection { get; set; }
        public LiteralNode<NoteEffectTechnique> EffectTechnique { get; set; }
        public LiteralNode<double> EffectTechniqueParameter { get; set; }

        public override IEnumerable<Node> Children
        {
            get
            {
                if (this.PreConnection != null)
                    yield return this.PreConnection;

                yield return this.String;

                if (this.Fret != null)
                    yield return this.Fret;

                if (this.EffectTechnique != null)
                {
                    yield return this.EffectTechnique;
                    if (this.EffectTechniqueParameter != null)
                        yield return this.EffectTechniqueParameter;
                }

                if (this.PostConnection != null)
                    yield return this.PostConnection;
            }
        }

        public bool ValueEquals(BeatNote other)
        {
            if (other == null)
                return false;

            if (this.String.Value != other.String)
                return false;

            if ((this.Fret?.Value ?? BeatNote.UnspecifiedFret) != other.Fret)
                return false;

            if ((this.EffectTechnique?.Value ?? NoteEffectTechnique.None) != other.EffectTechnique)
                return false;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((this.EffectTechniqueParameter?.Value ?? default(double)) != other.EffectTechniqueParameter)
                return false;

            if ((this.PreConnection?.Value ?? PreNoteConnection.None) != other.PreConnection)
                return false;

            if ((this.PostConnection?.Value ?? PostNoteConnection.None) != other.PostConnection)
                return false;

            return true;
        }

        public bool ToDocumentElement(TablatureContext context, ILogger logger, out BeatNote element)
        {
            var documentState = context.DocumentState;
            if (this.Fret != null
                && this.Fret.Value + documentState.MinimumCapoFret < (documentState.CapoFretOffsets?[this.String.Value - 1] ?? 0))
            {
                logger.Report(LogLevel.Warning, this.Fret.Range,
                                Messages.Warning_FretUnderCapo, this.String.Value,
                                this.Fret.Value);
            }

            element = new BeatNote
            {
                PreConnection = this.PreConnection?.Value ?? PreNoteConnection.None,
                PostConnection = this.PostConnection?.Value ?? PostNoteConnection.None,
                String = this.String.Value,
                Fret = this.Fret?.Value ?? BeatNote.UnspecifiedFret,
                EffectTechnique = this.EffectTechnique?.Value ?? NoteEffectTechnique.None,
                EffectTechniqueParameter = this.EffectTechniqueParameter?.Value ?? default(double)
            };

            return true;
        }
    }
}
