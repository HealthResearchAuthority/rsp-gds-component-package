using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.Encodings.Web;

namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled
///     <textarea>
///         field with configurable rows, placeholder, and styling.
///         Displays validation errors and allows for additional attributes and readonly/disabled states.
/// </summary>
[HtmlTargetElement("rsp-gds-textarea", Attributes = ForAttributeName)]
public class RspGdsTextareaTagHelper : RspGdsTagHelperBase
{
    /// <summary>
    ///     The GOV.UK width utility class applied to the textarea.
    ///     Defaults to "govuk-!-width-full".
    /// </summary>
    [HtmlAttributeName("width-class")]
    public string WidthClass { get; set; } = "govuk-!-width-full";

    /// <summary>
    ///     The number of visible text lines in the textarea.
    ///     Defaults to 5.
    /// </summary>
    [HtmlAttributeName("rows")]
    public int Rows { get; set; } = 5;

    /// <summary>
    ///     Placeholder text displayed inside the textarea when empty.
    /// </summary>
    [HtmlAttributeName("placeholder")]
    public string Placeholder { get; set; }

    /// <summary>
    ///     Builds the raw HTML for the GOV.UK textarea element including validation class and any additional attributes.
    /// </summary>
    /// <param name="hasFieldError">Whether a validation error is present for this field.</param>
    /// <param name="propertyName">The name of the property to bind.</param>
    /// <returns>Raw HTML string for the textarea element.</returns>
    protected string GetTextareaHtml(bool hasFieldError, string propertyName)
    {
        var value = For.Model?.ToString() ?? "";

        var extraAttributes = new Dictionary<string, string>(AdditionalAttributes);

        if (Readonly)
        {
            extraAttributes["readonly"] = "readonly";
        }

        if (Disabled)
        {
            extraAttributes["disabled"] = "disabled";
        }

        if (!string.IsNullOrEmpty(Placeholder))
        {
            extraAttributes["placeholder"] = Placeholder;
        }

        extraAttributes["rows"] = Rows.ToString();

        var attrHtml = string.Join(" ", extraAttributes.Select(kvp => $"{kvp.Key}='{HtmlEncoder.Default.Encode(kvp.Value)}'"));

        return $@"
                <textarea class='govuk-textarea {WidthClass} {(hasFieldError ? "govuk-textarea--error" : "")}'
                          id='{propertyName}'
                          name='{propertyName}'
                          {attrHtml}>{HtmlEncoder.Default.Encode(value)}</textarea>";
    }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var fieldId = !string.IsNullOrEmpty(FieldId) ? FieldId : propertyName;

        SetContainerAttributes(output, propertyName);

        var hasError = ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var entry) && entry.Errors.Count > 0;

        var errorHtml = BuildErrorHtml(propertyName);
        var hintHtml = BuildHintHtml(fieldId);
        var labelHtml = BuildLabelHtml(propertyName, propertyName, fieldId);

        var textareaHtml = GetTextareaHtml(hasError, propertyName);

        output.Content.SetHtmlContent(labelHtml + hintHtml + errorHtml + textareaHtml);
    }
}