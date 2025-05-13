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
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsInputTagHelper
        {
            For = CreateModelExpression("FirstName", "John"),
            LabelText = "Your name",
            ViewContext = CreateViewContext("FirstName")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var label = doc.DocumentNode.SelectSingleNode("//label");
        label.InnerHtml.ShouldContain("Your name");

        var input = doc.DocumentNode.SelectSingleNode("//input");
        input.Attributes["name"]?.Value.ShouldBe("FirstName");
        input.Attributes["value"]?.Value.ShouldBe("John");
        input.Attributes["class"]?.Value.ShouldNotContain("govuk-input--error");
    }

    [Fact]
    public void Process_AddsErrorClass_WhenModelStateHasError()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsInputTagHelper
        {
            For = CreateModelExpression("Email", ""),
            LabelText = "Email",
            ViewContext = CreateViewContext("Email", "", "Email is required")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        html.ShouldContain("govuk-input--error");
        html.ShouldContain("govuk-error-message");
        html.ShouldContain("Email is required");
    }

    [Fact]
    public void Process_SetsCustomAttributesCorrectly()
    {
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

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        html.ShouldContain("readonly='readonly'");
        html.ShouldContain("disabled='disabled'");
        html.ShouldContain("autocomplete='tel'");
        html.ShouldContain("placeholder='e.g. 07123456789'");
        html.ShouldContain("data-test='phone'");
    }
}
