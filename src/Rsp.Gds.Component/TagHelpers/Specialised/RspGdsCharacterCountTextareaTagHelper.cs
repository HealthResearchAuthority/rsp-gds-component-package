using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Rsp.Gds.Component.TagHelpers.Specialised;

/// <summary>
/// Renders a GOV.UK-styled character count textarea with a character or word limit indicator.
/// Extends <c>RspGdsTextareaTagHelper</c> with support for displaying separate word count validation errors.
/// </summary>
[HtmlTargetElement("rsp-gds-character-count-textarea", Attributes = ForAttributeName)]
public class RspGdsCharacterCountTextareaTagHelper : RspGdsTextareaTagHelper
{
    /// <summary>
    /// The name of the model property to check for word/character count validation errors.
    /// If set, a separate validation message will appear under the textarea.
    /// </summary>
    [HtmlAttributeName("word-count-error-for")]
    public string WordCountErrorProperty { get; set; }

    /// <summary>
    /// Generates the final GOV.UK form group markup including character count module,
    /// label, validation messages, textarea, and optional word count error message.
    /// </summary>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        // Standard field-level error
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var fieldEntry);
        var hasFieldError = fieldEntry != null && fieldEntry.Errors.Count > 0;

        // Optional word count error (separate from the field)
        ModelStateEntry wordCountEntry = null;
        if (!string.IsNullOrEmpty(WordCountErrorProperty))
        {
            ViewContext.ViewData.ModelState.TryGetValue(WordCountErrorProperty, out wordCountEntry);
        }

        var hasWordCountError = wordCountEntry != null && wordCountEntry.Errors.Count > 0;

        // Configure outer form group div
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class",
            $"govuk-form-group govuk-character-count {(hasFieldError ? "govuk-form-group--error" : "")}");
        output.Attributes.SetAttribute("data-module", "govuk-character-count");

        // Label
        var labelHtml = $@"
                <div class='govuk-label-wrapper'>
                    <label class='govuk-label govuk-label--s' for='{propertyName}'>
                        {LabelText ?? propertyName}
                    </label>
                </div>";

        // Field validation errors
        var fieldErrorsHtml = "";
        if (hasFieldError)
        {
            foreach (var error in fieldEntry.Errors)
            {
                fieldErrorsHtml += $"<span class='govuk-error-message'>{error.ErrorMessage}</span>";
            }
        }

        // Textarea (from base class)
        var textareaHtml = GetTextareaHtml(hasFieldError);

        // Word count error
        var wordCountErrorHtml = "";
        if (hasWordCountError)
        {
            wordCountErrorHtml = $@"
                    <div class='govuk-character-count__message govuk-error-message'>
                        {wordCountEntry.Errors[0].ErrorMessage}
                    </div>";
        }

        output.Content.SetHtmlContent(labelHtml + fieldErrorsHtml + textareaHtml + wordCountErrorHtml);
    }
}
