using FluentAssertions;
using Xunit;
using SecurityAwareness.Application.Services;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;
using SecurityAwareness.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SecurityAwareness.Tests.Unit;

/// <summary>
/// Tests for CsvHelper.Parse and Escape — RFC 4180 basics.
/// </summary>
public class CsvHelperTests
{
    [Fact]
    public void Parse_SimpleRows_ReturnsExpectedFields()
    {
        var csv = "a,b,c\r\n1,2,3\r\n4,5,6";
        var rows = CsvHelper.Parse(csv);
        rows.Should().HaveCount(3);
        rows[0].Should().BeEquivalentTo(new[] { "a", "b", "c" });
        rows[1].Should().BeEquivalentTo(new[] { "1", "2", "3" });
        rows[2].Should().BeEquivalentTo(new[] { "4", "5", "6" });
    }

    [Fact]
    public void Parse_QuotedFieldWithComma_KeepsCommaInField()
    {
        var csv = "name,desc\r\n\"Smith, John\",\"a,b,c\"";
        var rows = CsvHelper.Parse(csv);
        rows[1][0].Should().Be("Smith, John");
        rows[1][1].Should().Be("a,b,c");
    }

    [Fact]
    public void Parse_EscapedDoubleQuote_IsUnescaped()
    {
        var csv = "a\r\n\"He said \"\"hi\"\"\"";
        var rows = CsvHelper.Parse(csv);
        rows[1][0].Should().Be("He said \"hi\"");
    }

    [Fact]
    public void Parse_LfOnlyLineEndings_Handled()
    {
        var csv = "a,b\n1,2\n3,4";
        var rows = CsvHelper.Parse(csv);
        rows.Should().HaveCount(3);
        rows[2].Should().BeEquivalentTo(new[] { "3", "4" });
    }

    [Fact]
    public void Escape_FieldWithComma_WrappedInQuotes()
    {
        CsvHelper.Escape("Smith, John").Should().Be("\"Smith, John\"");
    }

    [Fact]
    public void Escape_FieldWithDoubleQuote_DoubledInsideQuotes()
    {
        CsvHelper.Escape("He said \"hi\"").Should().Be("\"He said \"\"hi\"\"\"");
    }

    [Fact]
    public void Escape_PlainField_NoQuotes()
    {
        CsvHelper.Escape("hello").Should().Be("hello");
    }
}
