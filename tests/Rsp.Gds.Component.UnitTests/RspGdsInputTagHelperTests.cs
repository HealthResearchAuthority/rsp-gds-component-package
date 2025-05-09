namespace Rsp.Gds.Component.UnitTests;

public class RspGdsInputTagHelperTests
{
    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "rsp-gds-input",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "rsp-gds-input",
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
    public void Process_GeneratesExpectedHtml_ForValidInput()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsInputTagHelper
        {
            For = CreateModelExpression("FirstName", "John"),
            LabelText = "Your name",
            ViewContext = CreateViewContext("FirstName")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var label = doc.DocumentNode.SelectSingleNode("//label");
        Assert.Contains("Your name", label.InnerHtml);

        var input = doc.DocumentNode.SelectSingleNode("//input");
        Assert.Equal("FirstName", input.Attributes["name"]?.Value);
        Assert.Equal("John", input.Attributes["value"]?.Value);
        Assert.DoesNotContain("govuk-input--error", input.Attributes["class"]?.Value);
    }

    [Fact]
    public void Process_AddsErrorClass_WhenModelStateHasError()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsInputTagHelper
        {
            For = CreateModelExpression("Email", ""),
            LabelText = "Email",
            ViewContext = CreateViewContext("Email", "", "Email is required")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("govuk-input--error", html);
        Assert.Contains("govuk-error-message", html);
        Assert.Contains("Email is required", html);
    }

    [Fact]
    public void Process_SetsCustomAttributesCorrectly()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsInputTagHelper
        {
            For = CreateModelExpression("Phone", "123"),
            LabelText = "Phone Number",
            Placeholder = "e.g. 07123456789",
            Autocomplete = "tel",
            Readonly = true,
            Disabled = true,
            AdditionalAttributes = new Dictionary<string, string> { { "data-test", "phone" } },
            ViewContext = CreateViewContext("Phone")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("readonly='readonly'", html);
        Assert.Contains("disabled='disabled'", html);
        Assert.Contains("autocomplete='tel'", html);
        Assert.Contains("placeholder='e.g. 07123456789'", html);
        Assert.Contains("data-test='phone'", html);
    }
}