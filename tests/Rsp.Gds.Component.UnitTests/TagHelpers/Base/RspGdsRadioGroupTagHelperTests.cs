namespace Rsp.Gds.Component.UnitTests.TagHelpers.Base;

public class RspGdsRadioGroupTagHelperTests
{
    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "rsp-gds-radio-group",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "rsp-gds-radio-group",
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
            new GdsOption { Label = "Yes", Value = "yes" },
            new GdsOption { Label = "No", Value = "no" }
        };
    }

    [Fact]
    public void Process_RendersRadioGroupWithSelectedOption()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsRadioGroupTagHelper
        {
            For = CreateModelExpression("AcceptTerms", "yes"),
            LabelText = "Do you accept?",
            Options = GetOptions(),
            ViewContext = CreateViewContext("AcceptTerms")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var radios = doc.DocumentNode.SelectNodes("//input[@type='radio']");
        Assert.Equal(2, radios.Count);

        var checkedRadio = radios.FirstOrDefault(r => r.Attributes["checked"] != null);
        Assert.NotNull(checkedRadio);
        Assert.Equal("yes", checkedRadio.Attributes["value"]?.Value);

        var label = doc.DocumentNode.SelectSingleNode("//legend//label");
        Assert.Contains("Do you accept?", label.InnerHtml);
    }

    [Fact]
    public void Process_RendersError_WhenModelStateHasError()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsRadioGroupTagHelper
        {
            For = CreateModelExpression("Confirm", null),
            LabelText = "Please confirm",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Confirm", "Selection is required")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("govuk-form-group--error", output.Attributes["class"].Value.ToString());
        Assert.Contains("Selection is required", html);
        Assert.Contains("govuk-error-message", html);
    }

    [Fact]
    public void Process_SelectsFirstValue_WhenModelIsList()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsRadioGroupTagHelper
        {
            For = CreateModelExpression("Options", new List<string> { "no", "yes" }),
            LabelText = "Pick an option",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Options")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var checkedRadio = doc.DocumentNode.SelectSingleNode("//input[@type='radio'][@checked]");
        Assert.NotNull(checkedRadio);
        Assert.Equal("no", checkedRadio.Attributes["value"]?.Value);
    }
}