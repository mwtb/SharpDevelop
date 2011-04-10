// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;

namespace ICSharpCode.AvalonEdit.Folding
{
	public sealed class FoldingMarginMarkerFactory : IFoldingMarginMarkerFactory
	{
		public BaseFoldingMarginMarker CreateFoldingMarginMarker(FoldingMargin owner, FoldingSection section, VisualLine line)
		{
			return new FoldingMarginMarker(){
				OwningMargin = owner,
				FoldingSection = section,
				IsExpanded = !section.IsFolded,
				VisualLine = line
			};
		}
	
	}
	
	sealed class FoldingMarginMarker : BaseFoldingMarginMarker
	{
			
		public override VisualLine VisualLine{get; set;}
		public override FoldingSection FoldingSection{get; set;}
		internal FoldingMargin OwningMargin;
				
		bool isExpanded;
		
		public bool IsExpanded {
			get { return isExpanded; }
			set {
				if (isExpanded != value) {
					isExpanded = value;
					InvalidateVisual();
				}
				if (FoldingSection != null)
					FoldingSection.IsFolded = !value;
			}
		}
		
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);
			if (!e.Handled) {
				if (e.ChangedButton == MouseButton.Left) {
					IsExpanded = !IsExpanded;
					e.Handled = true;
				}
			}
		}
		
		const double MarginSizeFactor = 0.7;
		
		protected override Size MeasureCore(Size availableSize)
		{
			double size = MarginSizeFactor * FoldingMargin.SizeFactor * (double)GetValue(TextBlock.FontSizeProperty);
			size = PixelSnapHelpers.RoundToOdd(size, PixelSnapHelpers.GetPixelSize(this).Width);
			return new Size(size, size);
		}
		
		protected override void OnRender(DrawingContext drawingContext)
		{
			Pen foregroundPen = new Pen(IsMouseDirectlyOver ? OwningMargin.ForegroundHighlighted : OwningMargin.Foreground, 1);
			Pen borderPen = new Pen(IsMouseDirectlyOver ? OwningMargin.BorderHighlighted : OwningMargin.Border, 1);
			foregroundPen.StartLineCap = PenLineCap.Square;
			foregroundPen.EndLineCap = PenLineCap.Square;
			Size pixelSize = PixelSnapHelpers.GetPixelSize(this);
			Rect rect = new Rect(pixelSize.Width / 2,
			                     pixelSize.Height / 2,
			                     this.RenderSize.Width - pixelSize.Width,
			                     this.RenderSize.Height - pixelSize.Height);
			drawingContext.DrawRectangle(IsMouseDirectlyOver ? OwningMargin.BackgroundHighlighted:OwningMargin.Background,
			                             borderPen,
			                             rect);
			double middleX = rect.Left + rect.Width / 2;
			double middleY = rect.Top + rect.Height / 2;
			double space = PixelSnapHelpers.Round(rect.Width / 8, pixelSize.Width) + pixelSize.Width;
			drawingContext.DrawLine(foregroundPen,
			                        new Point(rect.Left + space, middleY),
			                        new Point(rect.Right - space, middleY));
			if (!isExpanded) {
				drawingContext.DrawLine(foregroundPen,
				                        new Point(middleX, rect.Top + space),
				                        new Point(middleX, rect.Bottom - space));
			}

		}
		
		protected override void OnIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnIsMouseDirectlyOverChanged(e);
			InvalidateVisual();
		}
	}
}
