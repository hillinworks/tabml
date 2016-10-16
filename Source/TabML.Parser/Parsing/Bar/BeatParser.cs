﻿using System.Collections.Generic;
using TabML.Core.MusicTheory;
using TabML.Parser.AST;

// ReSharper disable InconsistentNaming

namespace TabML.Parser.Parsing.Bar
{
    class BeatParser : ParserBase<BeatNode>
    {

        public override bool TryParse(Scanner scanner, out BeatNode result)
        {
            var anchor = scanner.MakeAnchor();
            result = new BeatNode();

            ExistencyNode tiedNode;
            if (new CharExistencyParser('~').TryParse(scanner, out tiedNode))
            {
                result.Tied = tiedNode;
            }

            NoteValueNode noteValue;
            if (!new NoteValueParser().TryParse(scanner, out noteValue))
            {
                result = null;
                return false;
            }

            result.NoteValue = noteValue;

            scanner.SkipWhitespaces();

            var postNoteValueAnchor = scanner.MakeAnchor();

            ExistencyNode restNode;
            if (new CharExistencyParser('r').TryParse(scanner, out restNode))
            {
                result.Rest = restNode;
            }

            scanner.SkipWhitespaces();

            var postRestAnchor = scanner.MakeAnchor();

            if (!this.TryReadNotes(scanner, result.Notes))
            {
                result = null;
                return false;
            }

            var noteValueIndetemined = noteValue == null && !new NoteValueParser().HasError;

            // certain strum techniques (head strum techniques) can be placed before
            // the colon token
            LiteralNode<AllStringStrumTechnique> strumTechnique;
            Parser.TryReadAllStringStrumTechnique(scanner, this, out strumTechnique);

            if (noteValueIndetemined && result.Notes.Count == 0 && strumTechnique == null)
            {
                this.Report(ReportLevel.Error, scanner.LastReadRange,
                            Messages.Error_BeatBodyExpected);
                result = null;
                return false;
            }

            result.AllStringStrumTechnique = strumTechnique;

            scanner.SkipWhitespaces();

            if (scanner.Expect(':'))
            {
                scanner.SkipWhitespaces();
                do
                {
                    if (!this.TryReadModifier(scanner, result))
                    {
                        result = null;
                        return false;
                    }

                    scanner.SkipWhitespaces();
                } while (scanner.Expect(','));
            }

            if (tiedNode != null && result.HasRedunantSpecifierForTied)
            {
                this.Report(ReportLevel.Hint, postNoteValueAnchor.Range,
                    Messages.Hint_RedundantModifiersInTiedBeat);
            }

            if (restNode != null && result.HasRedunantSpecifierForRest)
            {
                this.Report(ReportLevel.Warning, postRestAnchor.Range,
                    Messages.Warning_RedundantModifiersInRestBeat);
            }

            result.Range = anchor.Range;
            return true;
        }

        private bool TryReadModifier(Scanner scanner, BeatNode result)
        {
            LiteralNode<BeatAccent> accent;
            if (Parser.TryReadNoteAccent(scanner, this, out accent))
            {
                if (result.Accent != null)
                    this.Report(ReportLevel.Warning, scanner.LastReadRange,
                                Messages.Warning_BeatAccentAlreadySpecified);
                else
                {
                    result.Accent = accent;
                    result.Modifiers.Add(accent);
                }
                return true;
            }

            LiteralNode<BeatEffectTechnique> beatEffectTechnique;
            LiteralNode<double> techniqueParameter;
            if (Parser.TryReadBeatEffectTechnique(scanner, this, out beatEffectTechnique, out techniqueParameter))
            {
                if (result.EffectTechnique != null)
                    this.Report(ReportLevel.Warning, scanner.LastReadRange,
                                Messages.Warning_BeatEffectTechniqueAlreadySpecified);
                else
                {
                    result.EffectTechnique = beatEffectTechnique;
                    result.EffectTechniqueParameter = techniqueParameter;
                    result.Modifiers.Add(beatEffectTechnique);
                    if (techniqueParameter != null)
                        result.Modifiers.Add(techniqueParameter);
                }

                return true;
            }

            LiteralNode<BeatDurationEffect> durationEffect;
            if (Parser.TryReadNoteDurationEffect(scanner, this, out durationEffect))
            {
                if (result.DurationEffect != null)
                    this.Report(ReportLevel.Warning, scanner.LastReadRange,
                                Messages.Warning_BeatNoteDurationEffectAlreadySpecified);
                else
                {
                    result.DurationEffect = durationEffect;
                    result.Modifiers.Add(durationEffect);
                }
                return true;
            }

            LiteralNode<StrumTechnique> strumTechnique;
            if (Parser.TryReadStrumTechnique(scanner, this, out strumTechnique))
            {
                if (result.StrumTechnique != null || result.AllStringStrumTechnique != null)
                    this.Report(ReportLevel.Warning, scanner.LastReadRange,
                                Messages.Warning_BeatStrumTechniqueAlreadySpecified);
                else
                {
                    result.StrumTechnique = strumTechnique;
                    result.Modifiers.Add(strumTechnique);
                }
                return true;
            }

            this.Report(ReportLevel.Error, scanner.LastReadRange,
                        Messages.Error_BeatModifierExpected);
            return false;
        }


        private bool TryReadNotes(Scanner scanner, ICollection<BeatNoteNode> notes)
        {
            if (!scanner.Expect('('))
                return true;

            scanner.SkipWhitespaces();

            var parenthesisClosed = false;
            while (!scanner.EndOfLine)
            {
                BeatNoteNode note;
                if (!new BeatNoteParser().TryParse(scanner, out note))
                {
                    return false;
                }

                notes.Add(note);

                if (!scanner.SkipOptional(',', true))
                {
                    if (scanner.Expect(')'))
                    {
                        parenthesisClosed = true;
                        break;
                    }
                }
            }

            if (!parenthesisClosed)
            {
                this.Report(ReportLevel.Error, scanner.LastReadRange,
                            Messages.Error_RhythmInstructionMissingCloseParenthesisInStringsSpecifier);
                return false;
            }

            return true;
        }
    }
}
