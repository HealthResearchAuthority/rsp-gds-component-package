namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled autocomplete input field with accessible autocomplete behavior.
///     Supports conditional display, dynamic API endpoints, validation messages, and hint text.
/// </summary>
[HtmlTargetElement("rsp-gds-autocomplete", Attributes = ForAttributeName)]
public class RspGdsAutocompleteTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     The model expression this input is bound to.
    ///     Used to set the input's name, id, and value.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    ///     The label text displayed above the input field.
    ///     If not provided, the model property name is used.
    /// </summary>
    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    /// <summary>
    ///     The API endpoint URL used to fetch autocomplete suggestions.
    /// </summary>
    [HtmlAttributeName("api-url")]
    public string ApiUrl { get; set; }

    /// <summary>
    ///     Optional override for validation error message text.
    ///     If not provided, the first model error is shown.
    /// </summary>
    [HtmlAttributeName("validation-message")]
    public string ValidationMessage { get; set; }

    /// <summary>
    ///     The list of parent question IDs this field depends on.
    ///     Rendered as a 'data-parents' attribute on the outer container.
    /// </summary>
    [HtmlAttributeName("dataparents-attr")]
    public string DataParentsAttr { get; set; }

    /// <summary>
    ///     Sets the data-questionId attribute manually (cannot bind to data-* directly).
    /// </summary>
    [HtmlAttributeName("dataquestionid-attr")]
    public string DataQuestionIdAttr { get; set; }

    /// <summary>
    ///     If true, the input will be wrapped with the 'conditional-field' class to support conditional show/hide logic.
    /// </summary>
    [HtmlAttributeName("conditional")]
    public bool ConditionalField { get; set; }

    /// <summary>
    ///     GOV.UK input width class. Defaults to 'govuk-!-width-three-quarters'.
    /// </summary>
    [HtmlAttributeName("width-class")]
    public string WidthClass { get; set; } = "govuk-!-width-three-quarters";

    /// <summary>
    ///     Optional HTML content displayed beneath the input label as a hint.
    /// </summary>
    [HtmlAttributeName("hint-html")]
    public string HintHtml { get; set; }

    /// <summary>
    ///     The ID for the hint element (used for accessibility with aria-describedby).
    /// </summary>
    [HtmlAttributeName("hint-id")]
    public string HintId { get; set; }

    /// <summary>
    ///     The aria-describedby attribute for the label (used to reference the hint).
    /// </summary>
    [HtmlAttributeName("label-aria-describedby")]
    public string LabelAriaDescribedBy { get; set; }

    /// <summary>
    ///     Optional ID for the outer container.
    /// </summary>
    [HtmlAttributeName("id")]
    public string HtmlId { get; set; }

    /// <summary>
    ///     Optionally override the generated input id (defaults to asp-for name).
    /// </summary>
    [HtmlAttributeName("field-id")]
    public string FieldId { get; set; }

    /// <summary>
    ///     Provides access to the current view context, including model state for validation.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var fieldId = !string.IsNullOrEmpty(FieldId) ? FieldId : propertyName.Replace(".", "_");
        var hiddenInputId = fieldId;
        var autoInputId = fieldId + "_autocomplete";
        var containerId = fieldId + "_autocomplete_container";
        var value = For.Model?.ToString() ?? "";

        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var modelState);
        var hasError = modelState != null && modelState.Errors.Any();

        var formGroupClass = "govuk-form-group";
        if (ConditionalField)
        {
            formGroupClass += " conditional-field";
        }

        if (hasError)
        {
            formGroupClass += " govuk-form-group--error";
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", formGroupClass);

        if (!string.IsNullOrWhiteSpace(HtmlId))
        {
            output.Attributes.SetAttribute("id", HtmlId);
        }

        if (!string.IsNullOrWhiteSpace(DataParentsAttr))
            output.Attributes.SetAttribute("data-parents", DataParentsAttr);

        if (!string.IsNullOrWhiteSpace(DataQuestionIdAttr))
            output.Attributes.SetAttribute("data-questionId", DataQuestionIdAttr);

        var labelDescribedBy = !string.IsNullOrEmpty(LabelAriaDescribedBy)
            ? LabelAriaDescribedBy
            : !string.IsNullOrEmpty(HintId)
                ? HintId
                : fieldId + "-hint";

        var encodedLabel = HtmlEncoder.Default.Encode(LabelText ?? propertyName);

        var labelHtml = $@"
<label class='govuk-label js-hidden' for='{hiddenInputId}' aria-describedby='{labelDescribedBy}'>{encodedLabel}</label>
<label class='govuk-label' for='{autoInputId}' aria-describedby='{labelDescribedBy}'>{encodedLabel}</label>";

        var hintHtml = !string.IsNullOrWhiteSpace(HintHtml)
            ? $"<div id='{labelDescribedBy}' class='govuk-hint'>{HintHtml}</div>"
            : string.Empty;

        var errorHtml = hasError
            ? $"<span class='govuk-error-message'>{HtmlEncoder.Default.Encode(ValidationMessage ?? modelState.Errors[0].ErrorMessage)}</span>"
            : string.Empty;

        var hiddenInputHtml = $@"
<input id='{hiddenInputId}'
       name='{propertyName}'
       type='text'
       class='govuk-input {WidthClass} js-hidden'
       value='{HtmlEncoder.Default.Encode(value)}' />";

        var containerHtml = $"<div id='{containerId}'></div>";

        var initScript = $@"<script>
document.addEventListener('DOMContentLoaded', function () {{
    initAutocomplete('{autoInputId}', '{hiddenInputId}', '{HtmlEncoder.Default.Encode(value)}', '{ApiUrl}', '{containerId}');
}});
</script>";

        output.Content.SetHtmlContent(labelHtml + hintHtml + errorHtml + hiddenInputHtml + containerHtml + initScript);
    }
}
