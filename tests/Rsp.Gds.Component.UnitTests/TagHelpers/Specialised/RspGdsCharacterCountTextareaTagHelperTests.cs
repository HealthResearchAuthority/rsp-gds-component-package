using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Rsp.Gds.Component.TagHelpers.Specialised;
using Shouldly;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Rsp.Gds.Component.UnitTests.TagHelpers.Specialised;

public class RspGdsCharacterCountTextareaTagHelperTests
{
    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "rsp-gds-character-count-textarea",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "rsp-gds-character-count-textarea",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    private static ViewContext CreateViewContext(string fieldName, string fieldError = null,
        string wordCountErrorField = null, string wordCountError = null)
    {
        var modelState = new ModelStateDictionary();
        if (fieldError != null)
        {
            modelState.AddModelError(fieldName, fieldError);
        }

        if (wordCountErrorField != null && wordCountError != null)
        {
            modelState.AddModelError(wordCountErrorField, wordCountError);
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
    public void Process_RendersTextareaWithCharacterCountWrapper()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCharacterCountTextareaTagHelper
        {
            For = CreateModelExpression("Feedback", "Some feedback"),
            LabelText = "Your feedback",
            ViewContext = CreateViewContext("Feedback")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var label = doc.DocumentNode.SelectSingleNode("//label");
        label.InnerHtml.ShouldContain("Your feedback");

        var textarea = doc.DocumentNode.SelectSingleNode("//textarea");
        textarea.Attributes["name"]?.Value.ShouldBe("Feedback");
        textarea.InnerHtml.ShouldContain("Some feedback");

        var wrapperDivClass = output.Attributes["class"].Value.ToString();
        wrapperDivClass.ShouldContain("govuk-character-count");
    }

    [Fact]
    public void Process_RendersFieldError_WhenModelStateHasError()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCharacterCountTextareaTagHelper
        {
            For = CreateModelExpression("Comments", ""),
            LabelText = "Comments",
            ViewContext = CreateViewContext("Comments", "This field is required")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        output.Attributes["class"].Value.ToString().ShouldContain("govuk-form-group--error");
        html.ShouldContain("This field is required");
        html.ShouldContain("govuk-error-message");
    }

    [Fact]
    public void Process_RendersWordCountError_WhenWordCountModelStateHasError()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCharacterCountTextareaTagHelper
        {
            For = CreateModelExpression("Notes", "Some notes"),
            WordCountErrorProperty = "NotesWordCount",
            ViewContext = CreateViewContext("Notes", null, "NotesWordCount", "Word limit exceeded")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        html.ShouldContain("Word limit exceeded");
        html.ShouldContain("govuk-character-count__message");
        html.ShouldContain("govuk-error-message");
    }
}
