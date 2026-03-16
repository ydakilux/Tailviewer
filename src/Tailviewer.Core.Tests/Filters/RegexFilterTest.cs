using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Tailviewer.Api;

namespace Tailviewer.Core.Tests.Filters
{
	[TestFixture]
	public sealed class RegexFilterTest
	{
		[Test]
		public void TestMatchWithNullContent()
		{
			var filter = new RegexFilter(".*", true);
			var matches = new List<LogLineMatch>();
			new Action(() => filter.Match(new LogEntry(Core.Columns.Minimum){RawContent = null}, matches)).Should().NotThrow("because null content should be handled gracefully");
			matches.Should().BeEmpty("because null content should not match");
		}

		[Test]
		public void TestMatchWithValidContent()
		{
			var filter = new RegexFilter("Foo", true);
			var matches = new List<LogLineMatch>();
			filter.Match(new LogEntry(Core.Columns.Minimum){RawContent = "Foobar"}, matches);
			matches.Count.Should().Be(1);
			matches[0].Index.Should().Be(0);
			matches[0].Count.Should().Be(3);
		}

		[Test]
		public void TestPassesFilterWithNullContent()
		{
			var filter = new RegexFilter(".*", true);
			filter.PassesFilter(new LogEntry(Core.Columns.Minimum){RawContent = null}).Should().BeFalse("because null content should not pass the filter");
		}

		[Test]
		public void TestPassesFilterWithValidContent()
		{
			var filter = new RegexFilter("foo", true);
			filter.PassesFilter(new LogEntry(Core.Columns.Minimum){RawContent = "foobar"}).Should().BeTrue();
		}

		[Test]
		public void TestPassesFilterCaseSensitive()
		{
			var filter = new RegexFilter("foo", false);
			filter.PassesFilter(new LogEntry(Core.Columns.Minimum){RawContent = "Foobar"}).Should().BeFalse("because case should be ignored when isCaseSensitive is false");
		}
	}
}
