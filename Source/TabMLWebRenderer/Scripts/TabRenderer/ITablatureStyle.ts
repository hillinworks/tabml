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
        }

        title: ITablatureTextStyle;
        fretNumber: ITablatureTextStyle;
        lyrics: ITablatureTextStyle;
    }
}