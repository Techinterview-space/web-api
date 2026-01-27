using Infrastructure.Services.Html;
using Xunit;

namespace InfrastructureTests.Services.Html;

public class MarkdownToTelegramHtmlTests
{
    [Fact]
    public void Constructor_WithNullSource_ReturnsEmptyHtml()
    {
        // Arrange & Act
        var converter = new MarkdownToTelegramHtml(null);

        // Assert
        Assert.Equal(string.Empty, converter.Source);
        Assert.Equal(string.Empty, converter.Html);
    }

    [Fact]
    public void Constructor_WithEmptySource_ReturnsEmptyHtml()
    {
        // Arrange & Act
        var converter = new MarkdownToTelegramHtml(string.Empty);

        // Assert
        Assert.Equal(string.Empty, converter.Source);
        Assert.Equal(string.Empty, converter.Html);
    }

    [Theory]
    [InlineData("# Header 1", "<b>Header 1</b>")]
    [InlineData("## Header 2", "<b>Header 2</b>")]
    [InlineData("### Header 3", "<b>Header 3</b>")]
    [InlineData("#### Header 4", "<b>Header 4</b>")]
    [InlineData("##### Header 5", "<b>Header 5</b>")]
    [InlineData("###### Header 6", "<b>Header 6</b>")]
    public void ConvertHeadersToBold_SingleHeader_ConvertsCorrectly(string source, string expected)
    {
        // Arrange & Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(expected, converter.Html);
    }

    [Fact]
    public void ConvertHeadersToBold_MultipleHeaders_ConvertsCorrectly()
    {
        // Arrange
        var source = @"# Main Title
## Subtitle
### Section";
        var expected = @"<b>Main Title</b>
<b>Subtitle</b>
<b>Section</b>";

        // Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(expected, converter.Html);
    }

    [Theory]
    [InlineData("**bold text**", "<b>bold text</b>")]
    [InlineData("This is **bold** text", "This is <b>bold</b> text")]
    [InlineData("**multiple** **bold** words", "<b>multiple</b> <b>bold</b> words")]
    public void ConvertBoldText_ValidMarkdown_ConvertsCorrectly(string source, string expected)
    {
        // Arrange & Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(expected, converter.Html);
    }

    [Theory]
    [InlineData("*italic text*", "<i>italic text</i>")]
    [InlineData("This is *italic* text", "This is <i>italic</i> text")]
    [InlineData("*multiple* *italic* words", "<i>multiple</i> <i>italic</i> words")]
    public void ConvertItalicText_ValidMarkdown_ConvertsCorrectly(string source, string expected)
    {
        // Arrange & Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(expected, converter.Html);
    }

    [Fact]
    public void ConvertItalicText_DoesNotInterfereWithBold_ConvertsCorrectly()
    {
        // Arrange
        var source = "This is **bold** and *italic* text";
        var expected = "This is <b>bold</b> and <i>italic</i> text";

        // Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(expected, converter.Html);
    }

    [Theory]
    [InlineData("```code```", "<pre>code</pre>")]
    [InlineData("```console.log('hello');```", "<pre>console.log('hello');</pre>")]
    public void ConvertCodeBlocks_SingleLine_ConvertsCorrectly(string source, string expected)
    {
        // Arrange & Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(expected, converter.Html);
    }

    [Theory]
    [InlineData("```javascript\nconsole.log('hello');```", "<pre language=\"javascript\">console.log('hello');</pre>")]
    [InlineData("```cpp\nint main() {}```", "<pre language=\"cpp\">int main() {}</pre>")]
    [InlineData("```python\nprint('hello')```", "<pre language=\"python\">print('hello')</pre>")]
    public void ConvertCodeBlocks_WithLanguage_ConvertsCorrectly(string source, string expected)
    {
        // Arrange & Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(expected, converter.Html);
    }

    [Fact]
    public void ConvertCodeBlocks_MultiLine_ConvertsCorrectly()
    {
        // Arrange
        var source = @"```
function hello() {
    console.log('Hello World');
}
```";
        var expected = @"<pre>
function hello() {
    console.log('Hello World');
}
</pre>";

        // Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(expected, converter.Html);
    }

    [Fact]
    public void ConvertMarkdown_PreservesNewlines_CorrectFormat()
    {
        // Arrange
        var source = @"# Title

This is a paragraph.

Another paragraph.";
        var expected = @"<b>Title</b>

This is a paragraph.

Another paragraph.";

        // Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(expected, converter.Html);
    }

    [Fact]
    public void ConvertMarkdown_DoesNotTouchLists_PreservesOriginal()
    {
        // Arrange
        var source = @"- Item 1
- Item 2
- Item 3";

        // Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(source, converter.Html);
    }

    [Fact]
    public void ConvertMarkdown_ComplexExample_ConvertsCorrectly()
    {
        // Arrange
        var source = @"# Interview Report

## Candidate Information
- Name: **John Doe**
- Position: *Senior Developer*

### Technical Assessment
The candidate demonstrated **excellent** knowledge in:
- JavaScript
- *React framework*
- ```TypeScript```

```
const example = {
    name: 'test',
    value: 42
};
```";

        var expected = @"<b>Interview Report</b>

<b>Candidate Information</b>
- Name: <b>John Doe</b>
- Position: <i>Senior Developer</i>

<b>Technical Assessment</b>
The candidate demonstrated <b>excellent</b> knowledge in:
- JavaScript
- <i>React framework</i>
- <pre>TypeScript</pre>

<pre>
const example = {
    name: 'test',
    value: 42
};
</pre>";

        // Act
        var converter = new MarkdownToTelegramHtml(source);

        // Assert
        Assert.Equal(expected, converter.Html);
    }

    [Fact]
    public void ToString_ReturnsHtmlProperty()
    {
        // Arrange
        var source = "**Bold text**";
        var converter = new MarkdownToTelegramHtml(source);

        // Act
        var result = converter.ToString();

        // Assert
        Assert.Equal(converter.Html, result);
        Assert.Equal("<b>Bold text</b>", result);
    }
}