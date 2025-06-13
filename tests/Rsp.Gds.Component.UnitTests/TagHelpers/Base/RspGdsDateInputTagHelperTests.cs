﻿namespace Rsp.Gds.Component.UnitTests.TagHelpers.Base;

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
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsDateInputTagHelper
        {
            For = CreateModelExpression("BirthDate", null),
            DayName = "BirthDate.Day",
            MonthName = "BirthDate.Month",
            IsMonthADropdown = false,
            YearName = "BirthDate.Year",
            LabelText = "Date of birth",
            HintHtml = "For example, 31 3 1980",
            ViewContext = CreateViewContext()
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        html.ShouldContain("Date of birth");
        html.ShouldContain("For example, 31 3 1980");

        doc.DocumentNode.SelectSingleNode("//input[@name='BirthDate.Day']").ShouldNotBeNull();
        doc.DocumentNode.SelectSingleNode("//input[@name='BirthDate.Month']").ShouldNotBeNull();
        doc.DocumentNode.SelectSingleNode("//input[@name='BirthDate.Year']").ShouldNotBeNull();
    }

    [Fact]
    public void Process_RendersErrorMessage_WhenModelStateHasError()
    {
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

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        html.ShouldContain("Invalid date");
        output.Attributes["class"].Value.ToString().ShouldContain("govuk-form-group--error");

        var dayInput = doc.DocumentNode.SelectSingleNode("//input[@name='StartDate.Day']");
        dayInput.ShouldNotBeNull();
        dayInput.Attributes["class"].Value.ShouldContain("govuk-input--error");
    }

    [Fact]
    public void Process_UsesForNameAsFallback_WhenErrorKeyIsNull()
    {
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

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        html.ShouldContain("Required");
    }
}