namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled single input field, with support for validation messages,
///     placeholder text, width styling, readonly/disabled modes, and additional attributes.
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
    [HtmlAttributeName("conditional-field")]
    public bool ConditionalField { get; set; } = false;

    /// <summary>
    ///     Provides access to the current view context, including ModelState for validation.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        // Get the current value from the model to prefill the input
        var value = For.Model?.ToString() ?? "";

        // Look up model state for validation info
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var entry);
        var hasError = entry != null && entry.Errors.Count > 0;

        // Build form group CSS classes based on conditional/error state
        var formGroupClass = "govuk-form-group"
                             + (ConditionalField ? " conditional-field" : "")
                             + (hasError ? " govuk-form-group--error" : "");

        // Define outer container element
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", propertyName); // Add unique ID
        output.Attributes.SetAttribute("class", formGroupClass); // Add govuk-form-group with modifiers

        // Render the label above the input
        var labelHtml = $@"
            <label class='govuk-label govuk-label--s' for='{propertyName}'>
                {LabelText ?? propertyName}
            </label>";

        // Build error HTML if any validation errors exist
        var errorsHtml = "";
        if (hasError)
        {
            foreach (var error in entry.Errors)
            {
                errorsHtml += $"<span class='govuk-error-message'>{error.ErrorMessage}</span>";
            }
        }

        // Compose additional attributes from TagHelper inputs
        var extraAttributes = new Dictionary<string, string>(AdditionalAttributes);

        // Append readonly/disabled/autocomplete/placeholder if specified
        if (Readonly)
        {
            extraAttributes["readonly"] = "readonly";
        }

        if (Disabled)
        {
            extraAttributes["disabled"] = "disabled";
        }

        if (!string.IsNullOrEmpty(Autocomplete))
        {
            extraAttributes["autocomplete"] = Autocomplete;
        }

        if (!string.IsNullOrEmpty(Placeholder))
        {
            extraAttributes["placeholder"] = Placeholder;
        }

        // Add ARIA invalid marker if error exists and not already specified
        if (!extraAttributes.ContainsKey("aria-invalid") && hasError)
        {
            extraAttributes["aria-invalid"] = "true";
        }

        // Flatten additional attributes into HTML string format
        var attrHtml = string.Join(" ", extraAttributes.Select(kvp => $"{kvp.Key}='{kvp.Value}'"));

        // Build the final <input> element
        var inputHtml = $@"
            <input class='govuk-input {WidthClass} {(hasError ? "govuk-input--error" : "")}'
                   id='{propertyName}'
                   name='{propertyName}'
                   type='{InputType}'
                   value='{value}'
                   {attrHtml} />";

        // Set final output HTML
        output.Content.SetHtmlContent(labelHtml + errorsHtml + inputHtml);
    }
}