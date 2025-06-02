namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled single input field, with support for validation messages,
///     placeholder text, conditional display, additional attributes, and frontend logic hooks.
/// </summary>
[HtmlTargetElement("rsp-gds-input", Attributes = ForAttributeName)]
public class RspGdsInputTagHelper : RspGdsTagHelperBase
{
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
    ///     A class name or token to apply when a conditional rule matches.
    /// </summary>
    [HtmlAttributeName("conditional-class")]
    public string ConditionalClass { get; set; }

    /// <summary>
    ///     Placeholder text displayed inside the textarea when empty.
    /// </summary>
    [HtmlAttributeName("placeholder")]
    public string Placeholder { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var fieldId = !string.IsNullOrEmpty(FieldId) ? FieldId : propertyName.Replace(".", "_");
        var value = For.Model?.ToString() ?? string.Empty;

        SetContainerAttributes(output, propertyName);
        if (!string.IsNullOrWhiteSpace(ConditionalClass))
        {
            output.Attributes.SetAttribute("conditional-class", ConditionalClass);
        }

        var hasError = HasError(propertyName);
        var inputClass = $"govuk-input {WidthClass}" +
                         (!string.IsNullOrWhiteSpace(ConditionalClass) ? $" {ConditionalClass}" : string.Empty) +
                         (hasError ? " govuk-input--error" : string.Empty);

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

        if (hasError && !extraAttributes.ContainsKey("aria-invalid"))
        {
            extraAttributes["aria-invalid"] = "true";
        }

        var attrHtml = string.Join(" ", extraAttributes
            .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
            .Select(kvp => $"{kvp.Key}='{HtmlEncoder.Default.Encode(kvp.Value)}'"));

        var labelAriaDescribedBy = !string.IsNullOrEmpty(LabelAriaDescribedBy) ? LabelAriaDescribedBy : propertyName;
        var hintId = !string.IsNullOrEmpty(HintId) ? HintId : propertyName;

        var labelHtml = $@"
    <label class='govuk-label' for='{fieldId}' aria-describedby='{labelAriaDescribedBy}'>
        {LabelText ?? propertyName}
    </label>";

        var hintHtml = BuildHintHtml(fieldId);
        var errorHtml = BuildErrorHtml(propertyName);

        var inputHtml = $@"
            <input class='{inputClass}'
                   id='{fieldId}'
                   name='{propertyName}'
                   type='{InputType}'
                   value='{HtmlEncoder.Default.Encode(value)}'
                   {attrHtml} />";

        output.Content.SetHtmlContent(labelHtml + hintHtml + errorHtml + inputHtml);
    }
}