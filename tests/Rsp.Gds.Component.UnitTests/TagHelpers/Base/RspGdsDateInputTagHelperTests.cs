namespace Rsp.Gds.Component.UnitTests.TagHelpers.Base;

public class RspGdsDateInputTagHelperTests
{
    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "rsp-gds-date-input",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "rsp-gds-date-input",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    private static ViewContext CreateViewContext(string errorKey = null, string errorMessage = null)
    {
        var modelState = new ModelStateDictionary();
        if (errorKey != null && errorMessage != null)
        {
            modelState.AddModelError(errorKey, errorMessage);
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
    public void Process_RendersExpectedHtml_WithHintAndInputs()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsDateInputTagHelper
        {
            For = CreateModelExpression("BirthDate", null),
            DayName = "BirthDate.Day",
            MonthName = "BirthDate.Month",
            YearName = "BirthDate.Year",
            LabelText = "Date of birth",
            HintHtml = "For example, 31 3 1980",
            ViewContext = CreateViewContext()
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        Assert.Contains("Date of birth", html);
        Assert.Contains("For example, 31 3 1980", html);

        Assert.NotNull(doc.DocumentNode.SelectSingleNode("//input[@name='BirthDate.Day']"));
        Assert.NotNull(doc.DocumentNode.SelectSingleNode("//input[@name='BirthDate.Month']"));
        Assert.NotNull(doc.DocumentNode.SelectSingleNode("//input[@name='BirthDate.Year']"));
    }

    [Fact]
    public void Process_RendersErrorMessage_WhenModelStateHasError()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsDateInputTagHelper
        {
            For = CreateModelExpression("StartDate", null),
            DayName = "StartDate.Day",
            MonthName = "StartDate.Month",
            YearName = "StartDate.Year",
            LabelText = "Start date",
            ErrorKey = "StartDate",
            ViewContext = CreateViewContext("StartDate", "Invalid date")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        Assert.Contains("Invalid date", html);
        Assert.Contains("govuk-form-group--error", output.Attributes["class"].Value.ToString());

        var dayInput = doc.DocumentNode.SelectSingleNode("//input[@name='StartDate.Day']");
        Assert.Contains("govuk-input--error", dayInput.Attributes["class"].Value);
    }

    [Fact]
    public void Process_UsesForNameAsFallback_WhenErrorKeyIsNull()
    {
        // Arrange
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsDateInputTagHelper
        {
            For = CreateModelExpression("EndDate", null),
            DayName = "EndDate.Day",
            MonthName = "EndDate.Month",
            YearName = "EndDate.Year",
            LabelText = "End date",
            ErrorKey = null,
            ViewContext = CreateViewContext("EndDate", "Required")
        };

        // Act
        tagHelper.Process(context, output);

        // Assert
        var html = output.Content.GetContent();
        Assert.Contains("Required", html);
    }
}