﻿using TabML.Core;
using TabML.Core.Logging;
using TabML.Core.MusicTheory;
using TabML.Core.Parsing;
using TabML.Parser.AST;

namespace TabML.Parser.Parsing.Commandlets
{
    [CommandletParser("tempo")]
    class TempoCommandletParser : CommandletParserBase<TempoCommandletNode>
    {
        public override bool TryParse(Scanner scanner, out TempoCommandletNode commandlet)
        {
            scanner.SkipOptional(':', true);

            commandlet = new TempoCommandletNode();

            var match = scanner.Match(@"((\d+)\s*=\s*)?(\d+)");

            if (!match.Success)
            {
                this.Report(LogLevel.Error, scanner.LastReadRange,
                            Messages.Error_InvalidTempoSignature);
                commandlet = null;
                return false;
            }

            if (match.Groups[1].Success)
            {
                var noteValueNumber = int.Parse(match.Groups[2].Value);

                BaseNoteValue noteValue;
                if (!BaseNoteValues.TryParse(noteValueNumber, out noteValue))
                {
                    this.Report(LogLevel.Error, scanner.LastReadRange,
                                Messages.Error_IrrationalNoteValueInTempoSignatureNotSupported);
                    commandlet = null;
                    return false;
                }

                commandlet.NoteValue
                    = new LiteralNode<BaseNoteValue>(noteValue, new TextRange(scanner.LastReadRange, match.Groups[2], scanner.Source));
            }

            var beats = int.Parse(match.Groups[3].Value);

            if (beats == 0)
            {
                this.Report(LogLevel.Error, scanner.LastReadRange,
                            Messages.Error_TempoSignatureSpeedTooLow);
                commandlet = null;
                return false;
            }

            if (beats > 10000)
            {
                this.Report(LogLevel.Error, scanner.LastReadRange,
                            Messages.Error_TempoSignatureSpeedTooFast);
                commandlet = null;
                return false;
            }

            commandlet.Beats = new LiteralNode<int>(beats, new TextRange(scanner.LastReadRange, match.Groups[3], scanner.Source));

            return true;
        }
    }
}
