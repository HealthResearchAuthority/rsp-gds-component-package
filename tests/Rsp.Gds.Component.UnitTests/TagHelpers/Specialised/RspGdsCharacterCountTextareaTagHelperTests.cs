namespace Rsp.Gds.Component.UnitTests.TagHelpers.Specialised;

public class RspGdsCharacterCountTextareaTagHelperTests
{
    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "rsp-gds-character-count-textarea",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "rsp-gds-character-count-textarea",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    private static ViewContext CreateViewContext(string fieldName, string fieldError = null,
        string wordCountErrorField = null, string wordCountError = null)
    {
        var modelState = new ModelStateDictionary();
        if (fieldError != null)
        {
            modelState.AddModelError(fieldName, fieldError);
        }

        if (wordCountErrorField != null && wordCountError != null)
        {
            modelState.AddModelError(wordCountErrorField, wordCountError);
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
    public void Process_RendersTextareaWithCharacterCountWrapper()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCharacterCountTextareaTagHelper
        {
            For = CreateModelExpression("Feedback", "Some feedback"),
            LabelText = "Your feedback",
            ViewContext = CreateViewContext("Feedback")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var label = doc.DocumentNode.SelectSingleNode("//label");
        Assert.Contains("Your feedback", label.InnerHtml);

        var textarea = doc.DocumentNode.SelectSingleNode("//textarea");
        Assert.Equal("Feedback", textarea.Attributes["name"]?.Value);
        Assert.Contains("Some feedback", textarea.InnerHtml);

        var wrapperDiv = output.Attributes["class"].Value.ToString();
        Assert.Contains("govuk-character-count", wrapperDiv);
    }

    [Fact]
    public void Process_RendersFieldError_WhenModelStateHasError()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCharacterCountTextareaTagHelper
        {
            For = CreateModelExpression("Comments", ""),
            LabelText = "Comments",
            ViewContext = CreateViewContext("Comments", "This field is required")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("govuk-form-group--error", output.Attributes["class"].Value.ToString());
        Assert.Contains("This field is required", html);
        Assert.Contains("govuk-error-message", html);
    }

    [Fact]
    public void Process_RendersWordCountError_WhenWordCountModelStateHasError()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCharacterCountTextareaTagHelper
        {
            For = CreateModelExpression("Notes", "Some notes"),
            WordCountErrorProperty = "NotesWordCount",
            ViewContext = CreateViewContext("Notes", null, "NotesWordCount", "Word limit exceeded")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("Word limit exceeded", html);
        Assert.Contains("govuk-character-count__message", html);
        Assert.Contains("govuk-error-message", html);
    }
}