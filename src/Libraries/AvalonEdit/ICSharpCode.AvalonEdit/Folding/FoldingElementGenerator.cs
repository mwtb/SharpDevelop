// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit.Folding
{
	/// <summary>
	/// A <see cref="VisualLineElementGenerator"/> that produces line elements for folded <see cref="FoldingSection"/>s.
	/// </summary>
	public class FoldingElementGenerator : VisualLineElementGenerator
	{
		
		public Brush Foreground{get; set;}
		public Brush Background{get; set;}
		public Brush Border{get; set;}
				
		/// <summary>
		/// Gets/Sets the folding manager from which the foldings should be shown.
		/// </summary>
		public FoldingManager FoldingManager { get; set; }
		
		public FoldingElementGenerator()
		{
			Foreground = Brushes.Gray;
			Background = Brushes.Transparent;
			Border = Brushes.Gray;
		}
		
		/// <inheritdoc/>
		public override void StartGeneration(ITextRunConstructionContext context)
		{
			base.StartGeneration(context);
			if (FoldingManager != null) {
				if (context.TextView != FoldingManager.textView)
					throw new ArgumentException("Invalid TextView");
				if (context.Document != FoldingManager.document)
					throw new ArgumentException("Invalid document");
			}
		}
		
		/// <inheritdoc/>
		public override int GetFirstInterestedOffset(int startOffset)
		{
			if (FoldingManager != null)
				return FoldingManager.GetNextFoldedFoldingStart(startOffset);
			else
				return -1;
		}
		
		/// <inheritdoc/>
		public override VisualLineElement ConstructElement(int offset)
		{
			if (FoldingManager == null)
				return null;
			int foldedUntil = -1;
			FoldingSection foldingSection = null;
			foreach (FoldingSection fs in FoldingManager.GetFoldingsAt(offset)) {
				if (fs.IsFolded) {
					if (fs.EndOffset > foldedUntil) {
						foldedUntil = fs.EndOffset;
						foldingSection = fs;
					}
				}
			}
			if (foldedUntil > offset && foldingSection != null) {
				return getFoldedElement(foldingSection,foldedUntil - offset);
			} else {
				return null;
			}
		}
		
		protected virtual VisualLineElement getFoldedElement( FoldingSection foldingSection, int sectionLength )
		{
			string title = foldingSection.Title;
				if (string.IsNullOrEmpty(title))
					title = "...";
				var p = new VisualLineElementTextRunProperties(CurrentContext.GlobalTextRunProperties);
				p.SetForegroundBrush(Foreground);
				p.SetBackgroundBrush(Background);
				var textFormatter = TextFormatterFactory.Create(CurrentContext.TextView);
				var text = FormattedTextElement.PrepareText(textFormatter, title, p);
				return new FoldingLineElement(foldingSection, text, sectionLength, new Pen(Border,1.0));
		}
		
		sealed class FoldingLineElement : FormattedTextElement
		{
			readonly FoldingSection fs;
			readonly Pen _outlinePen;
			
			public FoldingLineElement(FoldingSection fs, TextLine text, int documentLength, Pen outlinePen) : base(text, documentLength)
			{
				this.fs = fs;
				_outlinePen = outlinePen;
			}
			
			public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
			{
				return new FoldingLineTextRun(this, this.TextRunProperties, _outlinePen);
			}
			
			protected internal override void OnMouseDown(MouseButtonEventArgs e)
			{
				if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left) {
					fs.IsFolded = false;
					e.Handled = true;
				} else {
					base.OnMouseDown(e);
				}
			}
		}
		
		sealed class FoldingLineTextRun : FormattedTextRun
		{
			private Pen _outlinePen = new Pen(Brushes.Gray, 1);
			
			public FoldingLineTextRun(FormattedTextElement element, TextRunProperties properties)
				: base(element, properties)
			{
			}
			
			public FoldingLineTextRun(FormattedTextElement element, TextRunProperties properties, Pen outlinePen)
				: this(element, properties)
			{
				if( outlinePen == null )
					throw new ArgumentNullException("outlinePen", "Pen cannot be null.");
				else
					_outlinePen = outlinePen;
			}
			
			public override void Draw(DrawingContext drawingContext, Point origin, bool rightToLeft, bool sideways)
			{
				var metrics = Format(double.PositiveInfinity);
				Rect r = new Rect(origin.X, origin.Y - metrics.Baseline, metrics.Width, metrics.Height);
				drawingContext.DrawRectangle(null, _outlinePen, r);
				base.Draw(drawingContext, origin, rightToLeft, sideways);
			}
		}
	}
}
