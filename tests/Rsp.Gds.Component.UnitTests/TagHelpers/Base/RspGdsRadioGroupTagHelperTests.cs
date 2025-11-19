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

    private static List<GdsOption> GetOptions() => new()
    {
        new GdsOption { Label = "Yes", Value = "yes" },
        new GdsOption { Label = "No", Value = "no" }
    };

    [Fact]
    public void Process_RendersRadioGroupWithSelectedOption()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsRadioGroupTagHelper
        {
            For = CreateModelExpression("AcceptTerms", "yes"),
            LabelText = "Do you accept?",
            Options = new List<GdsOption>
            {
                new GdsOption { Label = "Yes", Value = "yes" },
                new GdsOption { Label = "No", Value = "no" }
            },
            QuestionId = "AcceptTerms",
            ViewContext = CreateViewContext("AcceptTerms")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Ensure two radio inputs were rendered
        var radios = doc.DocumentNode.SelectNodes("//input[@type='radio']");
        radios.Count.ShouldBe(2);

        // Ensure the correct radio is marked as checked
        var checkedRadio = radios.FirstOrDefault(r => r.Attributes["checked"] != null);
        checkedRadio.ShouldNotBeNull();
        checkedRadio.Attributes["value"]?.Value.ShouldBe("yes");

        // Ensure the legend contains the label text (not wrapped in <label>)
        var legend = doc.DocumentNode.SelectSingleNode("//legend");
        legend.ShouldNotBeNull();
        legend.InnerText.ShouldContain("Do you accept?");
    }

    [Fact]
    public void Process_RendersError_WhenModelStateHasError()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsRadioGroupTagHelper
        {
            For = CreateModelExpression("Confirm", null),
            LabelText = "Please confirm",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Confirm", "Selection is required")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        output.Attributes["class"].Value.ToString().ShouldContain("govuk-form-group--error");
    }

    [Fact]
    public void Process_SelectsFirstValue_WhenModelIsList()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsRadioGroupTagHelper
        {
            For = CreateModelExpression("Options", new List<string> { "no", "yes" }),
            LabelText = "Pick an option",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Options")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var checkedRadio = doc.DocumentNode.SelectSingleNode("//input[@type='radio'][@checked]");
        checkedRadio.ShouldNotBeNull();
        checkedRadio.Attributes["value"]?.Value.ShouldBe("no");
    }
}