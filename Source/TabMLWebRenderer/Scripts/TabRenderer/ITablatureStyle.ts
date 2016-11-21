namespace TR {
    export interface ITablatureStyle {
        fallback: {
            fontFamily: string,
        }

        page: {
            width: number,
            height: number,
        }

        bar: {
            lineHeight: number;
            beamThickness: number,
            beamSpacing: number
        }

        note: {
            circleOnLongNotes: boolean,
            longNoteCirclePadding: number,
            dot: {
                radius: number,
                offset: number,
                spacing: number
            },
            flagSpacing : number,
            tuplet : ITablatureTextStyle,
        }

        tie: {
            instructionOffset: number,
            instructionText: ITablatureTextStyle
        }

        title: ITablatureTextStyle;
        fretNumber: ITablatureTextStyle;
        lyrics: ITablatureTextStyle;
    }
}