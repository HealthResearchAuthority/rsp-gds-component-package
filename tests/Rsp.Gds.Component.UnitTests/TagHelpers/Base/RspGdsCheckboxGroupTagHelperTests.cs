namespace Rsp.Gds.Component.UnitTests.TagHelpers.Base;

public class RspGdsCheckboxGroupTagHelperTests
{
    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "rsp-gds-checkbox-group",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "rsp-gds-checkbox-group",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    private static ViewContext CreateViewContext(string fieldName, string errorMessage = null)
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
        var metadata = provider.GetMetadataForType(value?.GetType() ?? typeof(IEnumerable<string>));
        var modelExplorer = new ModelExplorer(provider, metadata, value);
        return new ModelExpression(name, modelExplorer);
    }

    private static List<string> GetOptions()
    {
        return new List<string> { "Email", "Phone", "Post" };
    }

    [Fact]
    public void Process_RendersAllCheckboxes_WithLabels()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("ContactMethods", new List<string>()),
            LabelText = "Preferred contact methods",
            Options = GetOptions(),
            ViewContext = CreateViewContext("ContactMethods")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var inputs = doc.DocumentNode.SelectNodes("//input[@type='checkbox']");
        inputs.Count.ShouldBe(3);

        foreach (var option in GetOptions())
        {
            html.ShouldContain(option);
        }
    }

    [Fact]
    public void Process_ChecksSelectedCheckboxes()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("ContactMethods", new List<string> { "Email", "Post" }),
            LabelText = "Contact",
            Options = GetOptions(),
            ViewContext = CreateViewContext("ContactMethods")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var checkedInputs = doc.DocumentNode.SelectNodes("//input[@checked]");
        var values = checkedInputs.Select(i => i.GetAttributeValue("value", "")).ToList();

        values.ShouldContain("Email");
        values.ShouldContain("Post");
        values.ShouldNotContain("Phone");
    }

    [Fact]
    public void Process_RendersErrorMessage_WhenModelStateIsInvalid()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("Interests", null),
            LabelText = "Choose at least one",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Interests", "You must choose at least one")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        html.ShouldContain("You must choose at least one");
        output.Attributes["class"].Value.ToString().ShouldContain("govuk-form-group--error");
    }

    [Fact]
    public void Process_RendersUnchecked_WhenModelIsNull()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("Alerts", null),
            LabelText = "Alert options",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Alerts")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var checkedInputs = doc.DocumentNode.SelectNodes("//input[@type='checkbox'][@checked]");
        (checkedInputs == null || checkedInputs.Count == 0).ShouldBeTrue();
    }
}