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
    ///     Provides access to the current view context, including ModelState for validation.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var value = For.Model?.ToString() ?? "";

        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var entry);
        var hasError = entry != null && entry.Errors.Count > 0;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"govuk-form-group {(hasError ? "govuk-form-group--error" : "")}");

        // Build label
        var labelHtml = $@"
                <label class='govuk-label govuk-label--s' for='{propertyName}'>
                    {LabelText ?? propertyName}
                </label>";

        // Build errors
        var errorsHtml = "";
        if (hasError)
        {
            foreach (var error in entry.Errors)
            {
                errorsHtml += $"<span class='govuk-error-message'>{error.ErrorMessage}</span>";
            }
        }

        // Build attributes
        var extraAttributes = new Dictionary<string, string>(AdditionalAttributes);

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

        if (!extraAttributes.ContainsKey("aria-invalid") && hasError)
        {
            extraAttributes["aria-invalid"] = "true";
        }

        var attrHtml = string.Join(" ", extraAttributes.Select(kvp => $"{kvp.Key}='{kvp.Value}'"));

        // Build input manually without asp-for
        var inputHtml = $@"
                <input class='govuk-input {WidthClass} {(hasError ? "govuk-input--error" : "")}'
                       id='{propertyName}'
                       name='{propertyName}'
                       type='{InputType}'
                       value='{value}'
                       {attrHtml} />";

        // Set final content
        output.Content.SetHtmlContent(labelHtml + errorsHtml + inputHtml);
    }
}