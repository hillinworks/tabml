﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TabML.Core.Document;

namespace TabML.Editor.Rendering
{
    abstract class ElementRenderer
    {
        public ElementRenderer Owner { get; }

        public IRootElementRenderer Root => this as IRootElementRenderer
                                            ?? this.Owner?.Root;

        protected internal ElementRenderer(ElementRenderer owner)
        {
            this.Owner = owner;
        }

        public virtual void Initialize() { }
    }

    [DebuggerDisplay("{GetType().Name, nq}: {DebuggerDisplay, nq}")]
    abstract class ElementRenderer<TElement> : ElementRenderer
        where TElement : ElementBase
    {
        public TElement Element { get; }

        protected ElementRenderer(ElementRenderer owner, TElement element)
            : base(owner)
        {
            this.Element = element;
            this.Root.RegisterRenderer(element, this);
        }

        [DebuggerHidden]
        private string DebuggerDisplay => (this.Element as Element)?.Range?.Content 
            ?? this.Element.GetType().ToString();
    }

    abstract class ElementRenderer<TElement, TRenderingContext>
        : ElementRenderer<TElement>, IElementRendererWithContext<TRenderingContext>
        where TElement : ElementBase
        where TRenderingContext : RenderingContextBase
    {
        public TRenderingContext RenderingContext
        {
            get { return this.Root.RenderingContext.GetRenderingContext<TRenderingContext>(this); }
            set
            {
                this.Root.RenderingContext.AssignRenderingContext(this, value);
                this.OnAssignRenderingContext(value);
            }
        }

        protected ElementRenderer(ElementRenderer owner, TElement element)
            : base(owner, element)
        {
        }

        protected virtual void OnAssignRenderingContext(TRenderingContext renderingContext)
        {

        }
    }
}
