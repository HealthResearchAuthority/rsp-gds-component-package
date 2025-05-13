using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Rsp.Gds.Component.TagHelpers.Base;
using Shouldly;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

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

    private static List<GdsOption> GetOptions() => new()
    {
        new GdsOption { Label = "Option A", Value = "A" },
        new GdsOption { Label = "Option B", Value = "B" }
    };

    [Fact]
    public void Process_RendersExpectedHtml_WithDefaultOption()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsSelectTagHelper
        {
            For = CreateModelExpression("Category", ""),
            LabelText = "Choose category",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Category")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var label = doc.DocumentNode.SelectSingleNode("//label");
        label.InnerHtml.ShouldContain("Choose category");

        var select = doc.DocumentNode.SelectSingleNode("//select");
        select.Attributes["name"]?.Value.ShouldBe("Category");

        var defaultOption = doc.DocumentNode.SelectSingleNode("//option[@value='']");
        defaultOption.InnerHtml.ShouldContain("Please select...");
        defaultOption.Attributes["selected"]?.Value.ShouldBe(""); // no 'selected' attribute means it is not selected
    }

    [Fact]
    public void Process_RendersSelectedOption_WhenModelHasValue()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsSelectTagHelper
        {
            For = CreateModelExpression("Type", "B"),
            LabelText = "Select type",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Type")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var selected = doc.DocumentNode.SelectSingleNode("//option[@selected]");
        selected.Attributes["value"]?.Value.ShouldBe("B");
        selected.InnerHtml.ShouldContain("Option B");
    }

    [Fact]
    public void Process_RendersErrorMessage_WhenModelStateHasError()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsSelectTagHelper
        {
            For = CreateModelExpression("Status", ""),
            LabelText = "Status",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Status", "Required field")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        output.Attributes["class"].Value.ToString().ShouldContain("govuk-form-group--error");
        html.ShouldContain("Required field");
        html.ShouldContain("govuk-error-message");
    }

    [Fact]
    public void Process_SkipsDefaultOption_WhenIncludeDefaultOptionIsFalse()
    {
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

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var defaultOption = doc.DocumentNode.SelectSingleNode("//option[@value='']");
        defaultOption.ShouldBeNull();
    }
}
