/*
 * Created by SharpDevelop.
 * User: mwtb
 * Date: 10/11/2010
 * Time: 10:01 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows;
using ICSharpCode.AvalonEdit.Rendering;

namespace ICSharpCode.AvalonEdit.Folding
{
	/// <summary>
	/// Description of Interface1.
	/// </summary>
	public interface IFoldingMarginMarkerFactory
	{
		BaseFoldingMarginMarker CreateFoldingMarginMarker(FoldingMargin owner, FoldingSection section, VisualLine line);
	}
	
	
	public abstract class BaseFoldingMarginMarker : UIElement
	{
		public abstract FoldingSection FoldingSection{ get; set; }
		public abstract VisualLine VisualLine{ get; set; }
	}
}
