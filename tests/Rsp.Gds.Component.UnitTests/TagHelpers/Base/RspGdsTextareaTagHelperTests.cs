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

public class RspGdsTextareaTagHelperTests
{
    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "rsp-gds-textarea",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "rsp-gds-textarea",
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
    public void Process_GeneratesExpectedHtml_ForValidTextarea()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsTextareaTagHelper
        {
            For = CreateModelExpression("Comments", "Initial text"),
            LabelText = "Your comment",
            ViewContext = CreateViewContext("Comments")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var label = doc.DocumentNode.SelectSingleNode("//label");
        label.InnerHtml.ShouldContain("Your comment");

        var textarea = doc.DocumentNode.SelectSingleNode("//textarea");
        textarea.Attributes["name"]?.Value.ShouldBe("Comments");
        textarea.InnerHtml.ShouldContain("Initial text");
        textarea.Attributes["rows"]?.Value.ShouldBe("5");
        textarea.Attributes["class"]?.Value.ShouldNotContain("govuk-textarea--error");
    }

    [Fact]
    public void Process_AddsErrorClass_WhenModelStateHasError()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsTextareaTagHelper
        {
            For = CreateModelExpression("Notes", ""),
            LabelText = "Notes",
            ViewContext = CreateViewContext("Notes", "", "This field is required")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        html.ShouldContain("govuk-textarea--error");
        html.ShouldContain("govuk-error-message");
        html.ShouldContain("This field is required");
    }

    [Fact]
    public void Process_SetsCustomAttributesCorrectly()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsTextareaTagHelper
        {
            For = CreateModelExpression("Message", "Some message"),
            Placeholder = "Type here...",
            Readonly = true,
            Disabled = true,
            Rows = 7,
            AdditionalAttributes = new Dictionary<string, string> { { "data-test", "msg" } },
            ViewContext = CreateViewContext("Message")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var textarea = doc.DocumentNode.SelectSingleNode("//textarea");
        textarea.Attributes["rows"]?.Value.ShouldBe("7");
        textarea.Attributes["readonly"]?.Value.ShouldBe("readonly");
        textarea.Attributes["disabled"]?.Value.ShouldBe("disabled");
        textarea.Attributes["placeholder"]?.Value.ShouldBe("Type here...");
        textarea.Attributes["data-test"]?.Value.ShouldBe("msg");
    }
}
