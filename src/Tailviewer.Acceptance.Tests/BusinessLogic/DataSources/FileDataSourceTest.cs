using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;
using Tailviewer.Api.Tests;
using Tailviewer.BusinessLogic.DataSources;
using Tailviewer.BusinessLogic.Searches;
using Tailviewer.Core;
using Tailviewer.Settings;
using Tailviewer.Tests;

namespace Tailviewer.Acceptance.Tests.BusinessLogic.DataSources
{
	[TestFixture]
	public sealed class FileDataSourceTest
	{
		[SetUp]
		public void SetUp()
		{
			_scheduler = new ManualTaskScheduler();
			_logSourceFactory = new SimplePluginLogSourceFactory(_scheduler);
		}

		private ManualTaskScheduler _scheduler;
		private ILogSourceFactory _logSourceFactory;

		[Test]
		public void TestConstruction1()
		{
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, new DataSource(@"E:\somelogfile.txt") { Id = DataSourceId.CreateNew() }))
			{
				source.FullFileName.Should().Be(@"E:\somelogfile.txt");
				source.LevelFilter.Should().Be(LevelFlags.All);
				source.SearchTerm.Should().BeNull();
				source.FollowTail.Should().BeFalse();
			}
		}

		[Test]
		public void TestConstruction2()
		{
			var settings = new DataSource(@"E:\somelogfile.txt")
			{
				Id = DataSourceId.CreateNew(),
				SelectedLogLines = new HashSet<LogLineIndex> {1, 2}
			};
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, settings))
			{
				source.SelectedLogLines.Should().BeEquivalentTo(new LogLineIndex[] {1, 2});
			}
		}

		[Test]
		public void TestConstruction3([Values(true, false)] bool showDeltaTimes)
		{
			var settings = new DataSource(@"E:\somelogfile.txt")
			{
				Id = DataSourceId.CreateNew(),
				ShowDeltaTimes = showDeltaTimes
			};
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, settings))
			{
				source.ShowDeltaTimes.Should().Be(showDeltaTimes);
			}
		}
		
		[Test]
		public void TestConstruction4([Values(true, false)] bool showElapsedTime)
		{
			var settings = new DataSource(@"E:\somelogfile.txt")
			{
				Id = DataSourceId.CreateNew(),
				ShowElapsedTime = showElapsedTime
			};
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, settings))
			{
				source.ShowElapsedTime.Should().Be(showElapsedTime);
			}
		}

		[Test]
		public void TestChangeShowElapsedTime([Values(true, false)] bool showElapsedTime)
		{
			var settings = new DataSource(@"E:\somelogfile.txt")
			{
				Id = DataSourceId.CreateNew()
			};
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, settings))
			{
				source.ShowElapsedTime = showElapsedTime;
				settings.ShowElapsedTime.Should().Be(showElapsedTime);

				source.ShowElapsedTime = !showElapsedTime;
				settings.ShowElapsedTime.Should().Be(!showElapsedTime);
			}
		}

		[Test]
		public void TestChangeShowDeltaTimes([Values(true, false)] bool showDeltaTimes)
		{
			var settings = new DataSource(@"E:\somelogfile.txt")
			{
				Id = DataSourceId.CreateNew()
			};
			using (var source = new FileDataSource(_logSourceFactory, _scheduler, settings))
			{
				source.ShowDeltaTimes = showDeltaTimes;
				settings.ShowDeltaTimes.Should().Be(showDeltaTimes);

				source.ShowDeltaTimes = !showDeltaTimes;
				settings.ShowDeltaTimes.Should().Be(!showDeltaTimes);
			}
		}

		[Test]
		[Description("Verifies that the data source disposes of all of its resources")]
		public void TestDispose1()
		{
			LogSourceProxy permanentLogSource;
			LogSourceSearchProxy permanentSearch;

			LogSourceProxy permanentFindAllLogSource;
			LogSourceSearchProxy permanentFindAllSearch;

			FileDataSource source;
			using (source = new FileDataSource(_logSourceFactory, _scheduler, new DataSource(@"E:\somelogfile.txt") {Id = DataSourceId.CreateNew()}))
			{
				permanentLogSource = (LogSourceProxy) source.FilteredLogSource;
				permanentLogSource.IsDisposed.Should().BeFalse();

				permanentSearch = (LogSourceSearchProxy) source.Search;
				permanentSearch.IsDisposed.Should().BeFalse();

				permanentFindAllLogSource = (LogSourceProxy) source.FindAllLogSource;
				permanentFindAllLogSource.IsDisposed.Should().BeFalse();

				permanentFindAllSearch = (LogSourceSearchProxy) source.FindAllSearch;
				permanentFindAllSearch.IsDisposed.Should().BeFalse();
			}
			source.IsDisposed.Should().BeTrue();
			permanentLogSource.IsDisposed.Should().BeTrue();
			permanentSearch.IsDisposed.Should().BeTrue();
			permanentFindAllLogSource.IsDisposed.Should().BeTrue();
			permanentFindAllSearch.IsDisposed.Should().BeTrue();
		}

		[Test]
		[Description("Verifies that the data source stops all periodic tasks upon being disposed of")]
		public void TestDispose2()
		{
			FileDataSource source = new FileDataSource(_logSourceFactory, _scheduler,
				new DataSource(@"E:\somelogfile.txt") {Id = DataSourceId.CreateNew()});
			_scheduler.PeriodicTaskCount.Should().BeGreaterThan(0);
			source.Dispose();
			_scheduler.PeriodicTaskCount.Should().Be(0, "because all tasks should've been removed");
		}

		[Test]
		public void TestSearch1()
		{
			var logFile = new InMemoryLogSource();
			using (var dataSource = new FileDataSource(_scheduler, CreateDataSource(), logFile, TimeSpan.Zero))
			{
				logFile.AddEntry("Hello foobar world!");
				_scheduler.RunOnce();
				dataSource.SearchTerm = "foobar";
				_scheduler.Run(10);
				dataSource.Search.Count.Should().Be(1);
				var matches = dataSource.Search.Matches.ToList();
				matches.Should().Equal(new LogMatch(0, new LogLineMatch(6, 6)));
			}
		}

		[Test]
		public void TestHideEmptyLines1()
		{
			var logFile = new InMemoryLogSource();
			var settings = CreateDataSource();
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				dataSource.HideEmptyLines.Should().BeFalse();
				dataSource.HideEmptyLines = true;
				settings.HideEmptyLines.Should().BeTrue("because the data source should modify the settings object when changed");

				dataSource.HideEmptyLines = false;
				settings.HideEmptyLines.Should().BeFalse("because the data source should modify the settings object when changed");
			}
		}

		[Test]
		public void TestIsSingleLine()
		{
			var logFile = new InMemoryLogSource();
			var settings = CreateDataSource();
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				dataSource.IsSingleLine.Should().BeFalse();
				dataSource.IsSingleLine = true;
				settings.IsSingleLine.Should().BeTrue("because the data source should modify the settings object when changed");

				dataSource.IsSingleLine = false;
				settings.IsSingleLine.Should().BeFalse("because the data source should modify the settings object when changed");
			}
		}

		[Test]
		[Description("Verifies that ClearScreen() filters all entries of the log file")]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/215")]
		public void TestClearScreen()
		{
			var settings = CreateDataSource();
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Foo");
			logFile.AddEntry("Bar");
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				_scheduler.Run(3);
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(2);

				dataSource.ClearScreen();
				_scheduler.Run(3);
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(0, "because we've just cleared the screen");

				logFile.AddEntry("Hello!");
				_scheduler.Run(3);
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(1, "because newer log entries should still appear");
				dataSource.FilteredLogSource.GetEntry(0).RawContent.Should().Be("Hello!");
			}
		}

		[Test]
		[Description("Verifies that ShowAll() shows all entries of the log file again")]
		[Issue("https://github.com/Kittyfisto/Tailviewer/issues/215")]
		public void TestClearScreenShowAll()
		{
			var settings = CreateDataSource();
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Foo");
			logFile.AddEntry("Bar");
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				_scheduler.RunOnce();

				dataSource.ClearScreen();
				_scheduler.RunOnce();
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(0, "because we've just cleared the screen");

				dataSource.ShowAll();
				_scheduler.RunOnce();
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(2, "because we've just shown everything again");
			}
		}

		[Test]
		[Description("Verifies that FilteredLogSource works directly with InMemoryLogSource")]
		public void TestFilteredLogSourceDirect()
		{
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Hello registered world");
			logFile.AddEntry("Some other line");
			logFile.AddEntry("Another registered entry");
			logFile.AddEntry("Not matching");

			var filter = new SubstringFilter("registered", true);
			using (var filteredSource = new FilteredLogSource(_scheduler, TimeSpan.Zero, logFile, null, filter))
			{
				_scheduler.Run(10);
				filteredSource.GetProperty(Properties.LogEntryCount).Should().Be(2,
					"because only 2 entries contain 'registered'");
			}
		}

		[Test]
		[Description("Verifies that FilteredLogSource works via proxy chain like real data source")]
		public void TestFilteredLogSourceViaProxy()
		{
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Hello registered world");
			logFile.AddEntry("Some other line");
			logFile.AddEntry("Another registered entry");
			logFile.AddEntry("Not matching");

			// Create proxy like FileDataSource does for UnfilteredLogSource
			using (var unfilteredProxy = new LogSourceProxy(_scheduler, TimeSpan.Zero, logFile))
			{
				_scheduler.Run(10);
				unfilteredProxy.GetProperty(Properties.LogEntryCount).Should().Be(4);

				// Create FilteredLogSource wrapping the proxy, like CreateFilteredLogFile does
				var filter = new SubstringFilter("registered", true);
				using (var filteredSource = new FilteredLogSource(_scheduler, TimeSpan.Zero, unfilteredProxy, null, filter))
				{
					_scheduler.Run(10);
					filteredSource.GetProperty(Properties.LogEntryCount).Should().Be(2,
						"because only 2 entries contain 'registered'");

					// Now wrap in another proxy like the data source does
					using (var outerProxy = new LogSourceProxy(_scheduler, TimeSpan.Zero))
					{
						outerProxy.InnerLogSource = filteredSource;
						_scheduler.Run(10);
						outerProxy.GetProperty(Properties.LogEntryCount).Should().Be(2,
							"because the outer proxy should report the filtered count");
					}
				}
			}
		}

		[Test]
		[Description("Verifies that setting QuickFilterChain filters log entries")]
		public void TestQuickFilterChain_HideMode()
		{
			var settings = CreateDataSource();
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Hello registered world", LevelFlags.Info);
			logFile.AddEntry("Some other line", LevelFlags.Info);
			logFile.AddEntry("Another registered entry", LevelFlags.Info);
			logFile.AddEntry("Not matching", LevelFlags.Info);
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				_scheduler.Run(10);
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(4,
					"because no filter is set yet");

				// Set a filter chain (simulating "Hide non-matching lines" mode)
				var filter = Filter.Create("registered", FilterMatchType.SubstringFilter, true, false);
				dataSource.QuickFilterChain = new List<ILogEntryFilter> { filter };
				_scheduler.Run(100);

				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(2,
					"because only two entries contain 'registered'");
			}
		}

		[Test]
		[Description("Verifies that toggling from hide to highlight and back works")]
		public void TestQuickFilterChain_ToggleHideHighlightHide()
		{
			var settings = CreateDataSource();
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Hello registered world", LevelFlags.Info);
			logFile.AddEntry("Some other line", LevelFlags.Info);
			logFile.AddEntry("Another registered entry", LevelFlags.Info);
			logFile.AddEntry("Not matching", LevelFlags.Info);
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				_scheduler.Run(3);
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(4);

				// Step 1: Enable hide-mode filter
				var filter = Filter.Create("registered", FilterMatchType.SubstringFilter, true, false);
				dataSource.QuickFilterChain = new List<ILogEntryFilter> { filter };
				_scheduler.Run(10);
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(2,
					"because hide-mode filter should hide non-matching entries");

				// Step 2: Switch to highlight mode (remove from filter chain)
				dataSource.QuickFilterChain = null;
				_scheduler.Run(10);
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(4,
					"because highlight mode means no filtering, all entries visible");

				// Step 3: Switch back to hide mode
				var filter2 = Filter.Create("registered", FilterMatchType.SubstringFilter, true, false);
				dataSource.QuickFilterChain = new List<ILogEntryFilter> { filter2 };
				_scheduler.Run(10);
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(2,
					"because switching back to hide mode should filter again");
			}
		}

		[Test]
		[Description("Verifies that hide-mode filtering works for plain text entries without log levels (the root cause bug scenario)")]
		public void TestQuickFilterChain_HideMode_NoLogLevels()
		{
			var settings = CreateDataSource();
			// Plain text entries WITHOUT log levels — MultiLineLogSource will group these into one entry
			var logFile = new InMemoryLogSource();
			logFile.AddEntry("Hello registered world", LevelFlags.None);
			logFile.AddEntry("Some other line", LevelFlags.None);
			logFile.AddEntry("Another registered entry", LevelFlags.None);
			logFile.AddEntry("Not matching", LevelFlags.None);
			using (var dataSource = new FileDataSource(_scheduler, settings, logFile, TimeSpan.Zero))
			{
				_scheduler.Run(10);
				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(4,
					"because no filter is set yet");

				// Set a filter chain (simulating "Hide non-matching lines" mode)
				var filter = Filter.Create("registered", FilterMatchType.SubstringFilter, true, false);
				dataSource.QuickFilterChain = new List<ILogEntryFilter> { filter };
				_scheduler.Run(100);

				dataSource.FilteredLogSource.GetProperty(Properties.LogEntryCount).Should().Be(2,
					"because the line-level filter should hide non-matching lines even when MultiLineLogSource groups them into one entry");
			}
		}

		private DataSource CreateDataSource()
		{
			return new DataSource("ffff") {Id = DataSourceId.CreateNew()};
		}
	}
}