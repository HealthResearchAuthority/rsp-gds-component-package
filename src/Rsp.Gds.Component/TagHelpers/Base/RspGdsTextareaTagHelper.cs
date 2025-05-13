namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled
///     <textarea>
///         field with configurable rows, placeholder, and styling.
///         Displays validation errors and allows for additional attributes and readonly/disabled states.
/// </summary>
[HtmlTargetElement("rsp-gds-textarea", Attributes = ForAttributeName)]
public class RspGdsTextareaTagHelper : TagHelper
{
    protected const string ForAttributeName = "asp-for";

    /// <summary>
    ///     The model expression this textarea is bound to.
    ///     Used for setting the field name, id, and initial value.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    ///     The label text displayed above the textarea.
    ///     If not provided, the model property name is used.
    /// </summary>
    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

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
    ///     If true, the textarea is marked as readonly.
    /// </summary>
    [HtmlAttributeName("readonly")]
    public bool Readonly { get; set; } = false;

    /// <summary>
    ///     If true, the textarea is disabled and cannot be interacted with.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; } = false;

    /// <summary>
    ///     Any additional HTML attributes to include in the textarea element.
    ///     These override or extend the default attributes.
    /// </summary>
    [HtmlAttributeName("additional-attributes")]
    public IDictionary<string, string> AdditionalAttributes { get; set; } = new Dictionary<string, string>();

    /// <summary>
    ///     Provides access to the current view context, including ModelState for validation.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    /// <summary>
    ///     Builds the raw HTML for the GOV.UK textarea element including validation class and any additional attributes.
    /// </summary>
    /// <param name="hasFieldError">Whether a validation error is present for this field.</param>
    /// <returns>Raw HTML string for the textarea element.</returns>
    protected string GetTextareaHtml(bool hasFieldError)
    {
        var propertyName = For.Name;
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

        var attrHtml = string.Join(" ", extraAttributes.Select(kvp => $"{kvp.Key}='{kvp.Value}'"));

        return $@"
                <textarea class='govuk-textarea {WidthClass} {(hasFieldError ? "govuk-textarea--error" : "")}'
                          id='{propertyName}'
                          name='{propertyName}'
                          {attrHtml}>{value}</textarea>";
    }

    /// <summary>
    ///     Generates the final GOV.UK form group markup, including label, errors, and textarea.
    /// </summary>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var entry);
        var hasFieldError = entry != null && entry.Errors.Count > 0;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"govuk-form-group {(hasFieldError ? "govuk-form-group--error" : "")}");

        var labelHtml = $@"
                <label class='govuk-label govuk-label--s' for='{propertyName}'>
                    {LabelText ?? propertyName}
                </label>";

        var fieldErrorsHtml = "";
        if (hasFieldError)
        {
            foreach (var error in entry.Errors)
            {
                fieldErrorsHtml += $"<span class='govuk-error-message'>{error.ErrorMessage}</span>";
            }
        }

        var textareaHtml = GetTextareaHtml(hasFieldError);

        output.Content.SetHtmlContent(labelHtml + fieldErrorsHtml + textareaHtml);
    }
}