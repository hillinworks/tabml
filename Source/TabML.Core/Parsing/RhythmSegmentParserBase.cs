﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabML.Core.MusicTheory;
using TabML.Core.Parsing.AST;

namespace TabML.Core.Parsing
{
    abstract class RhythmSegmentParserBase<TNode> : ParserBase<TNode>
         where TNode : RhythmSegmentNodeBase, new()
    {

        public bool OptionalBrackets { get; }

        protected RhythmSegmentParserBase(bool optionalBrackets = false)
        {
            this.OptionalBrackets = optionalBrackets;
        }


        protected bool TryParseRhythmDefinition(Scanner scanner, ref TNode node)
        {
            var hasBrackets = scanner.Expect('[');

            if (!this.OptionalBrackets)
            {
                this.Report(ParserReportLevel.Error, scanner.LastReadRange,
                            ParseMessages.Error_RhythmSegmentExpectOpeningBracket);
                return false;
            }

            scanner.SkipWhitespaces();

            if (!RhythmSegmentParser.IsEndOfSegment(scanner))
            {
                RhythmVoiceNode voice;
                while (new RhythmVoiceParser().TryParse(scanner, out voice))
                {
                    node.Voices.Add(voice);
                    scanner.SkipWhitespaces();

                    if (RhythmSegmentParser.IsEndOfSegment(scanner))
                        break;

                    if (scanner.Peek() == ';')
                        continue;

                    this.Report(ParserReportLevel.Error, scanner.Pointer.AsRange(),
                                ParseMessages.Error_UnrecognizableRhythmSegmentElement);
                    node = null;
                    return false;
                }
            }

            if (hasBrackets)
            {
                if (scanner.Expect(']'))
                    return true;

                if (this.OptionalBrackets)
                    this.Report(ParserReportLevel.Warning, scanner.LastReadRange,
                                ParseMessages.Warning_RhythmSegmentMissingCloseBracket);
                else
                {
                    this.Report(ParserReportLevel.Error, scanner.LastReadRange,
                                ParseMessages.Error_RhythmSegmentMissingCloseBracket);
                    node = null;
                    return false;
                }
            }

            if (node.Voices.Count == 0)
            {
                this.Report(ParserReportLevel.Warning, scanner.LastReadRange,
                            ParseMessages.Warning_EmptyRhythmSegment);
            }

            return true;
        }
    }
}
