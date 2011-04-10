﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Drawing;
using ICSharpCode.Reports.Core.BaseClasses.Printing;
using ICSharpCode.Reports.Core.Interfaces;

namespace ICSharpCode.Reports.Core.Exporter
{
	/// <summary>
	/// Description of RowConverter.
	/// </summary>
	/// 
	
	public class GroupedRowConverter:BaseConverter
	{

		private BaseReportItem parent;
		
		public GroupedRowConverter(IDataNavigator dataNavigator,
		                           ExporterPage singlePage, ILayouter layouter):base(dataNavigator,singlePage,layouter)
		{
		}
		
		
		public override ExporterCollection Convert(BaseReportItem parent, BaseReportItem item)
		{
			if (parent == null) {
				throw new ArgumentNullException("parent");
			}
			if (item == null) {
				throw new ArgumentNullException("item");
			}
			
			ISimpleContainer simpleContainer = item as ISimpleContainer;
			this.parent = parent;
			
			simpleContainer.Parent = parent;
			
			PrintHelper.AdjustParent(parent,simpleContainer.Items);
			if (PrintHelper.IsTextOnlyRow(simpleContainer)) {
				ExporterCollection myList = new ExporterCollection();

				ConvertContainer (myList,simpleContainer,parent.Location.X,
				             new Point(base.SectionBounds.DetailStart.X,base.SectionBounds.DetailStart.Y));
				
				return myList;
			} else {
				return this.ConvertDataRow(simpleContainer);
			}
		}
		
		
		private ExporterCollection ConvertDataRow (ISimpleContainer simpleContainer)
		{
			ExporterCollection exporterCollection = new ExporterCollection();
			base.CurrentPosition = new Point(base.SectionBounds.DetailStart.X,base.SectionBounds.DetailStart.Y);
			BaseSection section = parent as BaseSection;
			
			DefaultLeftPosition = parent.Location.X;
			Size groupSize = Size.Empty;
			Size childSize = Size.Empty;
			
			if (section.Items.IsGrouped)
			{
				groupSize = section.Items[0].Size;
				childSize  = section.Items[1].Size;
			}
			
			
			do {
				base.SaveSectionSize(section.Size);
				PrintHelper.AdjustSectionLocation (section);
				section.Size = this.SectionBounds.DetailSectionRectangle.Size;
				
				// did we have GroupedItems at all
				if (section.Items.IsGrouped)
				{
					// GetType child navigator
					IDataNavigator childNavigator = base.DataNavigator.GetChildNavigator();
					
					base.Evaluator.SinglePage.IDataNavigator = childNavigator;
					
					base.CurrentPosition = ConvertGroupHeader(exporterCollection,section,base.CurrentPosition);
					
					section.Size = base.RestoreSectionSize;
					section.Items[0].Size = groupSize;
					section.Items[1].Size = childSize;
					
					childNavigator.Reset();
					childNavigator.MoveNext();
					
					//Convert children
					if (childNavigator != null) {
						StandardPrinter.AdjustBackColor(simpleContainer,GlobalValues.DefaultBackColor);
						do
						{
							section.Size = base.RestoreSectionSize;
							section.Items[0].Size = groupSize;
							section.Items[1].Size = childSize;
						
							FillRow(simpleContainer,childNavigator);
							FireRowRendering(simpleContainer,childNavigator);
							PrepareContainerForConverting(section,simpleContainer);
 
//							FireRowRendering(simpleContainer,childNavigator);
							base.CurrentPosition = ConvertStandardRow(exporterCollection,simpleContainer);
							CheckForPageBreak(section,exporterCollection);
						}
						while ( childNavigator.MoveNext());
						
						// GroupFooter
						base.ConvertGroupFooter(section,exporterCollection);
						
						base.PageBreakAfterGroupChange(section,exporterCollection);
						
						base.Evaluator.SinglePage.IDataNavigator = base.DataNavigator;
					}
				}
				else
				{
					// No Grouping at all, the first item in section.items is the DetailRow
					
					Size containerSize = section.Items[0].Size;
					FillRow(simpleContainer,base.DataNavigator);
					FireRowRendering(simpleContainer,base.DataNavigator);
					base.PrepareContainerForConverting(section,simpleContainer);
//					FireRowRendering(simpleContainer,base.DataNavigator);
					base.CurrentPosition = ConvertStandardRow (exporterCollection,simpleContainer);
					section.Size = base.RestoreSectionSize;
					section.Items[0].Size = containerSize;
				}
				CheckForPageBreak (section,exporterCollection);
				ShouldDrawBorder (section,exporterCollection);
			}
			while (base.DataNavigator.MoveNext());
			
			SectionBounds.ReportFooterRectangle =  new Rectangle(SectionBounds.ReportFooterRectangle.Left,
			                                                     section.Location.Y + section.Size.Height,
			                                                     SectionBounds.ReportFooterRectangle.Width,
			                                                     SectionBounds.ReportFooterRectangle.Height);
		
			return exporterCollection;
		}
		
		
		void CheckForPageBreak(BaseSection section, ExporterCollection exporterCollection)
		{
			var pageBreakRect = PrintHelper.CalculatePageBreakRectangle((BaseReportItem)section.Items[0],base.CurrentPosition);
			
			if (PrintHelper.IsPageFull(pageBreakRect,base.SectionBounds)) {
				base.CurrentPosition = ForcePageBreak (exporterCollection,section);
			}
		}
		
		
		protected override Point ForcePageBreak(ExporterCollection exporterCollection, BaseSection section)
		{
			base.ForcePageBreak(exporterCollection,section);
			return CalculateStartPosition();
		}
		
		
		private Point CalculateStartPosition()
		{
			return new Point(base.SectionBounds.PageHeaderRectangle.X,base.SectionBounds.PageHeaderRectangle.Y);
		}
		
		
		private Point ConvertGroupHeader(ExporterCollection exportList,BaseSection section,Point offset)
		{
			var retVal = Point.Empty;
			var rowSize = Size.Empty;
			ReportItemCollection groupCollection = null;
			var groupedRows  = BaseConverter.FindGroupHeader(section);
			if (groupedRows.Count == 0) {
				groupCollection = section.Items.ExtractGroupedColumns();
				base.DataNavigator.Fill(groupCollection);
				base.FireSectionRendering(section);
				ExporterCollection list = StandardPrinter.ConvertPlainCollection(groupCollection,offset);
				
				StandardPrinter.EvaluateRow(base.Evaluator,list);
				
				exportList.AddRange(list);
				AfterConverting (list);
				retVal =  new Point (DefaultLeftPosition,offset.Y + groupCollection[0].Size.Height + 20  + (3 *GlobalValues.GapBetweenContainer));
			} else {
				FillRow(groupedRows[0],base.DataNavigator);
				rowSize = groupedRows[0].Size;
				base.FireGroupHeaderRendering(groupedRows[0]);
				retVal = ConvertStandardRow(exportList,groupedRows[0]);
				
				groupedRows[0].Size = rowSize;
			}
			return retVal;
		}
		
		
		private static void ShouldDrawBorder (BaseSection section,ExporterCollection list)
		{
			if (section.DrawBorder == true) {
				BaseRectangleItem br = BasePager.CreateDebugItem (section);
				BaseExportColumn bec = br.CreateExportColumn();
				bec.StyleDecorator.Location = section.Location;
				list.Insert(0,bec);
			}
		}
	}
}
