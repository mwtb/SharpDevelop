﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using ICSharpCode.Core;
using ICSharpCode.PackageManagement;
using NuGet;
using NUnit.Framework;
using PackageManagement.Tests.Helpers;

namespace PackageManagement.Tests
{
	[TestFixture]
	public class PackageManagementOptionsViewModelTests
	{
		PackageManagementOptionsViewModel viewModel;
		PackageManagementOptions options;
		
		void CreateViewModel()
		{
			options = new PackageManagementOptions(new Properties());
			options.PackageSources.Clear();
			viewModel = new PackageManagementOptionsViewModel(options);
		}
		
		void CreateViewModelWithOnePackageSource()
		{
			CreateViewModel();
			AddPackageSourceToOptions("Source 1", "http://url1");			
		}
		
		void CreateViewModelWithTwoPackageSources()
		{
			CreateViewModel();
			AddPackageSourceToOptions("Source 1", "http://url1");
			AddPackageSourceToOptions("Source 2", "http://url2");
		}
		
		void AddPackageSourceToOptions(string name, string url)
		{
			var source = new PackageSource(url, name);
			options.PackageSources.Add(source);
		}
		
		[Test]
		public void Constructor_InstanceCreated_NoPackageSourceViewModels()
		{
			CreateViewModel();
			
			Assert.AreEqual(0, viewModel.PackageSourceViewModels.Count);
		}
		
		[Test]
		public void Load_OptionsHasOneRegisteredPackageSource_ViewModelHasOnePackageSourceViewModel()
		{
			CreateViewModelWithOnePackageSource();
			viewModel.Load();
			
			Assert.AreEqual(1, viewModel.PackageSourceViewModels.Count);
		}
		
		[Test]
		public void Load_OptionsHasOneRegisteredPackageSource_ViewModelHasOnePackageSourceViewModelWithPackageSourceFromOptions()
		{
			CreateViewModelWithOnePackageSource();
			viewModel.Load();
			
			var expectedSources = new PackageSource[] {
				options.PackageSources[0]
			};
			
			PackageSourceCollectionAssert.AreEqual(expectedSources, viewModel.PackageSourceViewModels);
		}
		
		[Test]
		public void Load_OptionsHasTwoRegisteredPackageSources_ViewModelHasTwoPackageSourceViewModelWithPackageSourcesFromOptions()
		{
			CreateViewModelWithTwoPackageSources();
			viewModel.Load();
			
			var expectedSources = new PackageSource[] {
				options.PackageSources[0],
				options.PackageSources[1]
			};
			
			PackageSourceCollectionAssert.AreEqual(expectedSources, viewModel.PackageSourceViewModels);
		}
		
		[Test]
		public void Load_PackageSourceModifiedAfterLoadAndSaveNotCalled_RegisteredPackageSourcesInOptionsUnchanged()
		{
			CreateViewModel();
			AddPackageSourceToOptions("Test", "http://sharpdevelop.com");
			viewModel.Load();
			
			PackageSourceViewModel packageSourceViewModel = viewModel.PackageSourceViewModels[0];
			packageSourceViewModel.Name = "Changed-Name";
			packageSourceViewModel.SourceUrl = "changed-url";
			
			var expectedSources = new PackageSource[] {
				new PackageSource("http://sharpdevelop.com", "Test")
			};
			
			PackageSourceCollectionAssert.AreEqual(expectedSources, options.PackageSources);
		}
		
		[Test]
		public void Save_PackageSourceModifiedAfterLoad_RegisteredPackageSourcesInOptionsUpdated()
		{
			CreateViewModel();
			AddPackageSourceToOptions("Test", "http://sharpdevelop.com");
			viewModel.Load();
			
			PackageSourceViewModel packageSourceViewModel = viewModel.PackageSourceViewModels[0];
			packageSourceViewModel.Name = "Test-updated";
			packageSourceViewModel.SourceUrl = "url-updated";
			
			viewModel.Save();
			
			var expectedSources = new PackageSource[] {
				new PackageSource("url-updated", "Test-updated")
			};
			
			PackageSourceCollectionAssert.AreEqual(expectedSources, options.PackageSources);
		}
		
		[Test]
		public void Save_OnePackageSourceAddedAfterLoadAndBeforeSave_TwoRegisteredPackageSourcesInOptions()
		{
			CreateViewModel();
			AddPackageSourceToOptions("Test", "http://sharpdevelop.com/1");
			viewModel.Load();
			
			var newSource = new PackageSource("http://sharpdevelop.com/2", "Test");
			
			var newPackageSourceViewModel = new PackageSourceViewModel(newSource);
			viewModel.PackageSourceViewModels.Add(newPackageSourceViewModel);
			
			viewModel.Save();
			
			var expectedSource = new PackageSource("http://sharpdevelop.com/1", "Test");
			
			var expectedSources = new PackageSource[] {
				expectedSource,
				newSource
			};
			
			PackageSourceCollectionAssert.AreEqual(expectedSources, options.PackageSources);
		}
		
		[Test]
		public void AddPackageSourceCommand_CommandExecuted_AddsPackageSourceToPackageSourceViewModelsCollection()
		{
			CreateViewModel();
			viewModel.Load();
			viewModel.NewPackageSourceName = "Test";
			viewModel.NewPackageSourceUrl = "http://sharpdevelop.com";
			
			viewModel.AddPackageSourceCommand.Execute(null);
			
			var expectedSources = new PackageSource[] {
				new PackageSource("http://sharpdevelop.com", "Test")
			};
			
			PackageSourceCollectionAssert.AreEqual(expectedSources, viewModel.PackageSourceViewModels);
		}
		
		[Test]
		public void AddPackageSourceCommand_NewPackageSourceHasNameButNoUrl_CanExecuteReturnsFalse()
		{
			CreateViewModel();
			viewModel.Load();
			viewModel.NewPackageSourceName = "Test";
			viewModel.NewPackageSourceUrl = null;
			
			bool result = viewModel.AddPackageSourceCommand.CanExecute(null);
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void AddPackageSourceCommand_NewPackageSourceHasNameAndUrl_CanExecuteReturnsTrue()
		{
			CreateViewModel();
			viewModel.Load();
			viewModel.NewPackageSourceName = "Test";
			viewModel.NewPackageSourceUrl = "http://codeplex.com";
			
			bool result = viewModel.AddPackageSourceCommand.CanExecute(null);
			
			Assert.IsTrue(result);
		}
		
		[Test]
		public void AddPackageSourceCommand_NewPackageSourceHasUrlButNoName_CanExecuteReturnsFalse()
		{
			CreateViewModel();
			viewModel.Load();
			viewModel.NewPackageSourceName = null;
			viewModel.NewPackageSourceUrl = "http://codeplex.com";
			
			bool result = viewModel.AddPackageSourceCommand.CanExecute(null);
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void AddPackageSource_NoExistingPackageSources_SelectsPackageSourceViewModel()
		{
			CreateViewModel();
			viewModel.Load();
			viewModel.NewPackageSourceUrl = "http://url";
			viewModel.NewPackageSourceName = "abc";
			
			viewModel.AddPackageSource();
			
			PackageSourceViewModel expectedViewModel = viewModel.PackageSourceViewModels[0];
			
			Assert.AreEqual(expectedViewModel, viewModel.SelectedPackageSourceViewModel);
		}
		
		[Test]
		public void NewPackageSourceName_Changed_NewPackageSourceNameUpdated()
		{
			CreateViewModel();
			viewModel.Load();
			viewModel.NewPackageSourceName = "Test";
			
			Assert.AreEqual("Test", viewModel.NewPackageSourceName);
		}
		
		[Test]
		public void NewPackageSourceUrl_Changed_NewPackageSourceUrlUpdated()
		{
			CreateViewModel();
			viewModel.Load();
			viewModel.NewPackageSourceUrl = "Test";
			
			Assert.AreEqual("Test", viewModel.NewPackageSourceUrl);
		}
		
		[Test]
		public void RemovePackageSourceCommand_TwoPackagesSourcesInListAndOnePackageSourceSelected_PackageSourceIsRemoved()
		{
			CreateViewModelWithTwoPackageSources();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[0];
			
			viewModel.RemovePackageSourceCommand.Execute(null);
			
			var expectedSources = new PackageSource[] {
				options.PackageSources[1]
			};
			
			PackageSourceCollectionAssert.AreEqual(expectedSources, viewModel.PackageSourceViewModels);
		}
		
		[Test]
		public void RemovePackageSourceCommand_NoPackageSourceSelected_CanExecuteReturnsFalse()
		{
			CreateViewModel();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = null;
			
			bool result = viewModel.RemovePackageSourceCommand.CanExecute(null);
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void RemovePackageSourceCommand_PackageSourceSelected_CanExecuteReturnsTrue()
		{
			CreateViewModelWithOnePackageSource();
			
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[0];
			
			bool result = viewModel.RemovePackageSourceCommand.CanExecute(null);
			
			Assert.IsTrue(result);
		}
		
		[Test]
		public void SelectedPackageSourceViewModel_Changed_PropertyChangedEventFiredForCanAddPackageSource()
		{
			CreateViewModelWithOnePackageSource();
			viewModel.Load();
			
			string propertyName = null;
			viewModel.PropertyChanged += (sender, e) => propertyName = e.PropertyName;
			
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[0];
			
			Assert.AreEqual("CanAddPackageSource", propertyName);
		}
		
		[Test]
		public void MovePackageSourceUpCommand_TwoPackagesSourcesInListAndLastPackageSourceSelected_PackageSourceIsMovedUp()
		{
			CreateViewModelWithTwoPackageSources();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[1];
			
			viewModel.MovePackageSourceUpCommand.Execute(null);
			
			var expectedSources = new PackageSource[] {
				options.PackageSources[1],
				options.PackageSources[0]
			};
			
			PackageSourceCollectionAssert.AreEqual(expectedSources, viewModel.PackageSourceViewModels);
		}
		
		[Test]
		public void MovePackageSourceUpCommand_FirstPackageSourceSelected_CanExecuteReturnsFalse()
		{
			CreateViewModelWithTwoPackageSources();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[0];
			
			bool result = viewModel.MovePackageSourceUpCommand.CanExecute(null);
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void MovePackageSourceUpCommand_LastPackageSourceSelected_CanExecuteReturnsTrue()
		{
			CreateViewModelWithTwoPackageSources();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[1];
			
			bool result = viewModel.MovePackageSourceUpCommand.CanExecute(null);
			
			Assert.IsTrue(result);
		}
		
		[Test]
		public void CanMovePackageSourceUp_NoPackages_ReturnsFalse()
		{
			CreateViewModel();
			viewModel.Load();
			
			bool result = viewModel.CanMovePackageSourceUp;
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void MovePackageSourceDownCommand_TwoPackagesSourcesAndFirstPackageSourceSelected_PackageSourceIsMovedDown()
		{
			CreateViewModelWithTwoPackageSources();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[0];
			
			viewModel.MovePackageSourceDownCommand.Execute(null);
			
			var expectedSources = new PackageSource[] {
				options.PackageSources[1],
				options.PackageSources[0]
			};
			
			PackageSourceCollectionAssert.AreEqual(expectedSources, viewModel.PackageSourceViewModels);
		}
		
		[Test]
		public void MovePackageSourceDownCommand_TwoPackageSourcesAndLastPackageSourceSelected_CanExecuteReturnsFalse()
		{
			CreateViewModelWithTwoPackageSources();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[1];
			
			bool result = viewModel.MovePackageSourceDownCommand.CanExecute(null);
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void MovePackageSourceDownCommand_TwoPackageSourcesAndFirstPackageSourceSelected_CanExecuteReturnsTrue()
		{
			CreateViewModelWithTwoPackageSources();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[0];
			
			bool result = viewModel.MovePackageSourceDownCommand.CanExecute(null);
			
			Assert.IsTrue(result);
		}
		
		[Test]
		public void CanMovePackageSourceDown_NoPackageSources_ReturnFalse()
		{
			CreateViewModel();
			viewModel.Load();
			
			bool result = viewModel.CanMovePackageSourceDown;
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void CanMovePackageSourceDown_OnePackageSourceAndPackageSourceIsSelected_ReturnsFalse()
		{
			CreateViewModelWithOnePackageSource();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[0];
			
			bool result = viewModel.CanMovePackageSourceDown;
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void CanMovePackageSourceDown_OnePackageSourceAndNothingIsSelected_ReturnsFalse()
		{
			CreateViewModelWithOnePackageSource();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = null;
			
			bool result = viewModel.CanMovePackageSourceDown;
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void CanMovePackageSourceUp_OnePackageSourceAndNothingIsSelected_ReturnsFalse()
		{
			CreateViewModelWithOnePackageSource();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = null;
			
			bool result = viewModel.CanMovePackageSourceUp;
			
			Assert.IsFalse(result);
		}
		
		[Test]
		public void SelectedPackageSourceViewModel_PropertyChanged_FiresPropertyChangedEvent()
		{
			CreateViewModelWithOnePackageSource();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[0];
			
			List<string> propertyNames = new List<string>();
			viewModel.PropertyChanged += (sender, e) => propertyNames.Add(e.PropertyName);
			viewModel.SelectedPackageSourceViewModel = null;
			
			Assert.Contains("SelectedPackageSourceViewModel", propertyNames);
		}
		
		[Test]
		public void AddPackageSource_OneExistingPackageSources_FiresPropertyChangedEventForSelectedPackageSource()
		{
			CreateViewModelWithOnePackageSource();
			viewModel.Load();
			viewModel.SelectedPackageSourceViewModel = viewModel.PackageSourceViewModels[0];
			viewModel.NewPackageSourceUrl = "http://url";
			viewModel.NewPackageSourceName = "Test";
			
			List<string> propertyNames = new List<string>();
			viewModel.PropertyChanged += (sender, e) => propertyNames.Add(e.PropertyName);
			viewModel.AddPackageSource();
			
			Assert.Contains("SelectedPackageSourceViewModel", propertyNames);
		}
	}
}
