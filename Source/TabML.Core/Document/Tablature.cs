﻿using System.Collections.Generic;

namespace TabML.Core.Document
{
    public class Tablature
    {
        public int Strings { get; set; }
        public MusicTheory.Tuning Tuning { get; set; }
        public List<ChordDefinition> ChordDefinitions { get; }

        public Tablature()
        {
            this.ChordDefinitions = new List<ChordDefinition>();
        }
    }
}