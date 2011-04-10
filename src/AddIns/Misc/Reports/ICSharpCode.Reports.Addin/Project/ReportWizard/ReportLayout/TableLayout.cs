﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Drawing;
using ICSharpCode.Reports.Core;
using ICSharpCode.Reports.Core.Interfaces;

namespace ICSharpCode.Reports.Addin.ReportWizard
{
	/// <summary>
	/// Description of TableLayout.
	/// </summary>

	public class TableLayout: AbstractLayout
	{
		
		public TableLayout(ReportModel reportModel,ReportItemCollection reportItemCollection):base(reportModel)
		{
			ReportItems = reportItemCollection;
		}
		
	
		public override void CreatePageHeader()
		{
			base.CreatePageHeader();
			base.ReportModel.PageHeader.Size = new Size(base.ReportModel.PageHeader.Size.Width,10);
			base.ReportModel.PageHeader.BackColor = Color.LightGray;
		}
		
		
		public override void CreateDataSection(ICSharpCode.Reports.Core.BaseSection section)
		{
			if (section == null) {
				throw new ArgumentNullException("section");
			}
			System.Drawing.Printing.Margins margin = GlobalValues.ControlMargins;
			
			ICSharpCode.Reports.Core.BaseTableItem table = new ICSharpCode.Reports.Core.BaseTableItem();
			ICSharpCode.Reports.Core.BaseRowItem detailRow = new ICSharpCode.Reports.Core.BaseRowItem();
			
			table.Name = "Table1";
			base.Container = table;
			AdjustContainer(base.ReportModel.DetailSection,table);
			base.ReportModel.DetailSection.Items.Add(table);
			
			ICSharpCode.Reports.Core.BaseRowItem headerRow = CreateRowWithTextColumns(Container);
			Container.Items.Add (headerRow);
			
			Point insertLocation =  new Point (margin.Left,headerRow.Location.Y + headerRow.Size.Height + margin.Bottom + margin.Top);
			
			
			if (base.ReportModel.ReportSettings.GroupColumnsCollection.Count > 0) {
				
				//Groupheader
				var groupHeader = base.CreateGroupHeader(insertLocation);
				Container.Items.Add(groupHeader);
				insertLocation = new Point(margin.Left,insertLocation.Y + groupHeader.Size.Height + margin.Bottom + margin.Top);
				
				//Detail
				CreateDetail(detailRow,insertLocation);
				Container.Items.Add (detailRow);
				
				// GroupFooter
				var groupFooter = base.CreateFooter(new Point(margin.Left,130));
				Container.Items.Add(groupFooter);

			}
			else
			{
				CreateDetail(detailRow,insertLocation);
				Container.Items.Add (detailRow);
			}
			
			CalculateContainerSize();
			section.Size = new Size (section.Size.Width,Container.Size.Height + margin.Top + margin.Bottom);
		}
		
		
		void CreateDetail (ICSharpCode.Reports.Core.BaseRowItem detailRow,Point insertLocation)
		{
			AdjustContainer (Container,detailRow);
			detailRow.Location = insertLocation;
			detailRow.Size =  new Size(detailRow.Size.Width,30);
			int defX = AbstractLayout.CalculateControlWidth(detailRow,ReportItems);
			
			int startX =  GlobalValues.ControlMargins.Left;
			
			foreach (ICSharpCode.Reports.Core.BaseReportItem ir in ReportItems)
			{
				Point np = new Point(startX,GlobalValues.ControlMargins.Top);
				startX += defX;
				ir.Location = np;
				ir.Parent = detailRow;
				detailRow.Items.Add(ir);
			}
		}
		
		
		private void CalculateContainerSize()
		{
			int h = GlobalValues.ControlMargins.Top;
			
			foreach (ICSharpCode.Reports.Core.BaseReportItem item  in Container.Items)
			{
				h = h + item.Size.Height + GlobalValues.ControlMargins.Bottom;
			}
			h 	= h + 3*GlobalValues.ControlMargins.Bottom;
			Container.Size =  new Size (Container.Size.Width,h);
		}
	}
}
