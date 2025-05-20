using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled single input field, with support for validation messages,
///     placeholder text, conditional display, additional attributes, and frontend logic hooks.
/// </summary>
[HtmlTargetElement("rsp-gds-input", Attributes = ForAttributeName)]
public class RspGdsInputTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     The model expression this input is bound to.
    ///     Used for setting the input's name, id, and value.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    ///     The label text displayed above the input field.
    ///     If not specified, the model property name is used.
    /// </summary>
    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    /// <summary>
    ///     GOV.UK width class to control input width.
    ///     Defaults to 'govuk-!-width-one-half'.
    /// </summary>
    [HtmlAttributeName("width-class")]
    public string WidthClass { get; set; } = "govuk-!-width-one-half";

    /// <summary>
    ///     The HTML input type (e.g. "text", "email", "number").
    ///     Defaults to "text".
    /// </summary>
    [HtmlAttributeName("input-type")]
    public string InputType { get; set; } = "text";

    /// <summary>
    ///     Sets the input's autocomplete attribute.
    ///     Useful for browser autofill support.
    /// </summary>
    [HtmlAttributeName("autocomplete")]
    public string Autocomplete { get; set; }

    /// <summary>
    ///     Placeholder text displayed in the input when it's empty.
    /// </summary>
    [HtmlAttributeName("placeholder")]
    public string Placeholder { get; set; }

    /// <summary>
    ///     If true, the input will be marked as readonly.
    /// </summary>
    [HtmlAttributeName("readonly")]
    public bool Readonly { get; set; } = false;

    /// <summary>
    ///     If true, the input will be disabled and not editable.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; } = false;

    /// <summary>
    ///     Additional HTML attributes to include in the input element.
    ///     These override default values if keys match.
    /// </summary>
    [HtmlAttributeName("additional-attributes")]
    public IDictionary<string, string> AdditionalAttributes { get; set; } = new Dictionary<string, string>();

    /// <summary>
    ///     Indicates whether this input field is conditionally shown.
    ///     Adds a <c>conditional-field</c> CSS class to the form group container.
    /// </summary>
    [HtmlAttributeName("conditional")]
    public bool ConditionalField { get; set; } = false;

    /// <summary>
    ///     An optional error key for model state validation lookup.
    ///     If omitted, the 'asp-for' name will be used.
    /// </summary>
    [HtmlAttributeName("error-key")]
    public string ErrorKey { get; set; }

    /// <summary>
    ///     Optional key for frontend JS styling or conditional validation logic.
    /// </summary>
    [HtmlAttributeName("error-class-for")]
    public string ErrorClassFor { get; set; }



    /// <summary>
    ///     A class name or token to apply when a conditional rule matches.
    /// </summary>
    [HtmlAttributeName("conditional-class")]
    public string ConditionalClass { get; set; }

    /// <summary>
    ///     The list of parent question IDs this input depends on.
    ///     Rendered as a 'data-parents' attribute.
    /// </summary>
    [HtmlAttributeName("dataparents-attr")]
    public string DataParentsAttr { get; set; }

    /// <summary>
    ///     The current question's ID used for conditional tracking.
    ///     Rendered as a 'data-questionId' attribute.
    /// </summary>
    [HtmlAttributeName("dataquestionid-attr")]
    public string DataQuestionIdAttr { get; set; }

    /// <summary>
    ///     Hint HTML content (typically rule descriptions) displayed below the label.
    /// </summary>
    [HtmlAttributeName("hint-html")]
    public string HintHtml { get; set; }

    /// <summary>
    ///     Sets the outer container ID.
    ///     If not specified, defaults to the 'asp-for' property name.
    /// </summary>
    [HtmlAttributeName("id")]
    public string HtmlId { get; set; }

    /// <summary>
    ///     Sets the outer container ID.
    ///     If not specified, defaults to the 'asp-for' property name.
    /// </summary>
    [HtmlAttributeName("field-id")]
    public string FieldId { get; set; }

    [HtmlAttributeName("label-aria-describedby")]
    public string LabelAriaDescribedBy { get; set; }

    [HtmlAttributeName("hint-id")]
    public string HintId { get; set; }

    /// <summary>
    ///     Optional override for validation message text.
    ///     If not provided, model validation error is used.
    /// </summary>
    [HtmlAttributeName("validation-message")]
    public string ValidationMessage { get; set; }


    /// <summary>
    ///     Provides access to the current view context, including ModelState for validation.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var fieldId = !string.IsNullOrEmpty(FieldId) ? FieldId : propertyName.Replace(".", "_");
        var value = For.Model?.ToString() ?? "";

        // Check for validation errors
        ViewContext.ViewData.ModelState.TryGetValue(ErrorKey ?? propertyName, out var entry);
        var hasError = entry != null && entry.Errors.Count > 0;

        // Build the CSS class for the form group
        var formGroupClass = "govuk-form-group"
                             + (ConditionalField ? " conditional-field" : "")
                             + (hasError ? " govuk-form-group--error" : "");

        // Outer container
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", formGroupClass);

        // Set optional attributes
        if (!string.IsNullOrWhiteSpace(HtmlId))
            output.Attributes.SetAttribute("id", HtmlId);
        if (!string.IsNullOrWhiteSpace(ConditionalClass))
            output.Attributes.SetAttribute("conditional-class", ConditionalClass);
        if (!string.IsNullOrWhiteSpace(DataParentsAttr))
            output.Attributes.SetAttribute("data-parents", DataParentsAttr);
        if (!string.IsNullOrWhiteSpace(DataQuestionIdAttr))
            output.Attributes.SetAttribute("data-questionId", DataQuestionIdAttr);

        // Compose input classes
        var inputClass = $"govuk-input {WidthClass}";
        if (!string.IsNullOrWhiteSpace(ConditionalClass))
            inputClass += $" {ConditionalClass}";
        if (hasError)
            inputClass += " govuk-input--error";

        // Compose extra attributes
        var extraAttributes = new Dictionary<string, string>(AdditionalAttributes);
        if (Readonly) extraAttributes["readonly"] = "readonly";
        if (Disabled) extraAttributes["disabled"] = "disabled";
        if (!string.IsNullOrEmpty(Autocomplete)) extraAttributes["autocomplete"] = Autocomplete;
        if (!string.IsNullOrEmpty(Placeholder)) extraAttributes["placeholder"] = Placeholder;
        if (hasError && !extraAttributes.ContainsKey("aria-invalid")) extraAttributes["aria-invalid"] = "true";

        var attrHtml = string.Join(" ", extraAttributes.Select(kvp => $"{kvp.Key}='{kvp.Value}'"));

        var labelAriaDescribedBy = !string.IsNullOrEmpty(LabelAriaDescribedBy) ? LabelAriaDescribedBy : propertyName;
        var hintId = !string.IsNullOrEmpty(HintId) ? HintId : propertyName;

        var labelHtml = $@"
    <label class='govuk-label' for='{fieldId}' aria-describedby='{labelAriaDescribedBy}'>
        {LabelText ?? propertyName}
    </label>";

        var hintHtml = !string.IsNullOrEmpty(HintHtml)
            ? $"<div id='{hintId}' class='govuk-hint'>{HintHtml}</div>"
            : string.Empty;

        // Render a validation error message span if applicable
        var errorMessage = !string.IsNullOrWhiteSpace(ValidationMessage)
            ? ValidationMessage
            : entry.Errors[0].ErrorMessage;

        var errorHtml = hasError && !string.IsNullOrWhiteSpace(errorMessage)
            ? $"<span class='govuk-error-message'>{HtmlEncoder.Default.Encode(errorMessage)}</span>"
            : "";

        // Build input field
        var inputHtml = $@"
            <input class='{inputClass}'
                   id='{fieldId}'
                   name='{propertyName}'
                   type='{InputType}'
                   value='{value}'
                   {attrHtml} />";

        // Final output
        output.Content.SetHtmlContent(labelHtml + hintHtml + errorHtml + inputHtml);
    }
}
