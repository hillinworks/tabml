﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabML.Core.Parsing.AST
{
    abstract class CapoStringsSpecifierNode : Node
    {
        public abstract int[] GetStringNumbers();
    }
}
