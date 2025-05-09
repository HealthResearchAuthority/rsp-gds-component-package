namespace Rsp.Gds.Component.UnitTests;

public class RspGdsTextareaTagHelperTests
{
    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "rsp-gds-textarea",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "rsp-gds-textarea",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    private static ViewContext CreateViewContext(string fieldName, string value = null, string errorMessage = null)
    {
        var modelState = new ModelStateDictionary();
        if (errorMessage != null)
        {
            modelState.AddModelError(fieldName, errorMessage);
        }

        var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), modelState)
        {
            Model = null
        };

        return new ViewContext
        {
            ViewData = viewData
        };
    }

    private static ModelExpression CreateModelExpression(string name, object value)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForType(value?.GetType() ?? typeof(string));
        var modelExplorer = new ModelExplorer(provider, metadata, value);
        return new ModelExpression(name, modelExplorer);
    }

    [Fact]
    public void Process_GeneratesExpectedHtml_ForValidTextarea()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsTextareaTagHelper
        {
            For = CreateModelExpression("Comments", "Initial text"),
            LabelText = "Your comment",
            ViewContext = CreateViewContext("Comments")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var label = doc.DocumentNode.SelectSingleNode("//label");
        Assert.Contains("Your comment", label.InnerHtml);

        var textarea = doc.DocumentNode.SelectSingleNode("//textarea");
        Assert.Equal("Comments", textarea.Attributes["name"]?.Value);
        Assert.Contains("Initial text", textarea.InnerHtml);
        Assert.Equal("5", textarea.Attributes["rows"]?.Value);
        Assert.DoesNotContain("govuk-textarea--error", textarea.Attributes["class"]?.Value);
    }

    [Fact]
    public void Process_AddsErrorClass_WhenModelStateHasError()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsTextareaTagHelper
        {
            For = CreateModelExpression("Notes", ""),
            LabelText = "Notes",
            ViewContext = CreateViewContext("Notes", "", "This field is required")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("govuk-textarea--error", html);
        Assert.Contains("govuk-error-message", html);
        Assert.Contains("This field is required", html);
    }

    [Fact]
    public void Process_SetsCustomAttributesCorrectly()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsTextareaTagHelper
        {
            For = CreateModelExpression("Message", "Some message"),
            Placeholder = "Type here...",
            Readonly = true,
            Disabled = true,
            Rows = 7,
            AdditionalAttributes = new Dictionary<string, string> { { "data-test", "msg" } },
            ViewContext = CreateViewContext("Message")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var textarea = doc.DocumentNode.SelectSingleNode("//textarea");
        Assert.Equal("7", textarea.Attributes["rows"]?.Value);
        Assert.Equal("readonly", textarea.Attributes["readonly"]?.Value);
        Assert.Equal("disabled", textarea.Attributes["disabled"]?.Value);
        Assert.Equal("Type here...", textarea.Attributes["placeholder"]?.Value);
        Assert.Equal("msg", textarea.Attributes["data-test"]?.Value);
    }
}