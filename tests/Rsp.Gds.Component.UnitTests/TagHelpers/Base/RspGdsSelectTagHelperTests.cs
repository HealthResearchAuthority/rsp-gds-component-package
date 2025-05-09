namespace Rsp.Gds.Component.UnitTests.TagHelpers.Base;

public class RspGdsSelectTagHelperTests
{
    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "rsp-gds-select",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "rsp-gds-select",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    private static ViewContext CreateViewContext(string fieldName, string fieldError = null)
    {
        var modelState = new ModelStateDictionary();
        if (fieldError != null)
        {
            modelState.AddModelError(fieldName, fieldError);
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

    private static List<GdsOption> GetOptions()
    {
        return new List<GdsOption>
        {
            new GdsOption { Label = "Option A", Value = "A" },
            new GdsOption { Label = "Option B", Value = "B" }
        };
    }

    [Fact]
    public void Process_RendersExpectedHtml_WithDefaultOption()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsSelectTagHelper
        {
            For = CreateModelExpression("Category", ""),
            LabelText = "Choose category",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Category")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var label = doc.DocumentNode.SelectSingleNode("//label");
        Assert.Contains("Choose category", label.InnerHtml);

        var select = doc.DocumentNode.SelectSingleNode("//select");
        Assert.Equal("Category", select.Attributes["name"]?.Value);

        var defaultOption = doc.DocumentNode.SelectSingleNode("//option[@value='']");
        Assert.Contains("Please select...", defaultOption.InnerHtml);
        Assert.Equal("", defaultOption.Attributes["selected"]?.Value);
    }

    [Fact]
    public void Process_RendersSelectedOption_WhenModelHasValue()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsSelectTagHelper
        {
            For = CreateModelExpression("Type", "B"),
            LabelText = "Select type",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Type")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var selected = doc.DocumentNode.SelectSingleNode("//option[@selected]");
        Assert.Equal("B", selected.Attributes["value"]?.Value);
        Assert.Contains("Option B", selected.InnerHtml);
    }

    [Fact]
    public void Process_RendersErrorMessage_WhenModelStateHasError()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsSelectTagHelper
        {
            For = CreateModelExpression("Status", ""),
            LabelText = "Status",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Status", "Required field")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("govuk-form-group--error", output.Attributes["class"].Value.ToString());
        Assert.Contains("Required field", html);
        Assert.Contains("govuk-error-message", html);
    }

    [Fact]
    public void Process_SkipsDefaultOption_WhenIncludeDefaultOptionIsFalse()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsSelectTagHelper
        {
            For = CreateModelExpression("Priority", "A"),
            LabelText = "Priority",
            Options = GetOptions(),
            IncludeDefaultOption = false,
            ViewContext = CreateViewContext("Priority")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var defaultOption = doc.DocumentNode.SelectSingleNode("//option[@value='']");
        Assert.Null(defaultOption);
    }
}