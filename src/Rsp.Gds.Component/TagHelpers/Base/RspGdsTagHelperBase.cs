namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Base class for GOV.UK-styled TagHelpers.
///     Provides shared logic for validation, hint rendering, label generation, conditional display, and container attribute setup.
/// </summary>
public abstract class RspGdsTagHelperBase : TagHelper
{
    protected const string ForAttributeName = "asp-for";

    /// <summary>
    ///     The model expression this input is bound to.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    ///     The label text to render.
    /// </summary>
    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    /// <summary>
    ///     Optional HTML content rendered below the label.
    /// </summary>
    [HtmlAttributeName("hint-html")]
    public string HintHtml { get; set; }

    /// <summary>
    ///     Optional ID to assign to the hint container.
    /// </summary>
    [HtmlAttributeName("hint-id")]
    public string HintId { get; set; }

    /// <summary>
    ///     Optional aria-describedby ID used in the label.
    /// </summary>
    [HtmlAttributeName("label-aria-describedby")]
    public string LabelAriaDescribedBy { get; set; }

    /// <summary>
    ///     Optional override message for model validation errors.
    /// </summary>
    [HtmlAttributeName("validation-message")]
    public string ValidationMessage { get; set; }

    /// <summary>
    ///     Optional override key for model state validation lookup.
    /// </summary>
    [HtmlAttributeName("error-key")]
    public string ErrorKey { get; set; }

    /// <summary>
    ///     Optional override for the input field ID.
    /// </summary>
    [HtmlAttributeName("field-id")]
    public string FieldId { get; set; }

    /// <summary>
    ///     Optional ID for the outer container.
    /// </summary>
    [HtmlAttributeName("id")]
    public string HtmlId { get; set; }

    /// <summary>
    ///     If true, applies conditional CSS class for field visibility control.
    /// </summary>
    [HtmlAttributeName("conditional-field")]
    public bool ConditionalField { get; set; }

    /// <summary>
    ///     Optional additional CSS class for conditionally rendered fields.
    /// </summary>
    [HtmlAttributeName("conditional-class")]
    public string ConditionalClass { get; set; }

    /// <summary>
    ///     Used to set data-parents attribute for conditional logic.
    /// </summary>
    [HtmlAttributeName("dataparents-attr")]
    public string DataParentsAttr { get; set; }

    /// <summary>
    ///     Used to set data-questionId attribute for conditional logic.
    /// </summary>
    [HtmlAttributeName("dataquestionid-attr")]
    public string DataQuestionIdAttr { get; set; }

    /// <summary>
    ///     If true, the field will be rendered readonly.
    /// </summary>
    [HtmlAttributeName("readonly")]
    public bool Readonly { get; set; } = false;

    /// <summary>
    ///     If true, the field will be rendered disabled.
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
    ///     ViewContext for model state and validation.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    /// <summary>
    ///     Attempts to retrieve the model state entry for the specified property.
    /// </summary>
    protected bool TryGetModelState(string propertyName, out ModelStateEntry entry)
    {
        entry = null;

        var keyToUse = !string.IsNullOrWhiteSpace(ErrorKey) &&
                       ViewContext?.ViewData?.ModelState?.ContainsKey(ErrorKey) == true
            ? ErrorKey
            : propertyName;

        return ViewContext?.ViewData?.ModelState?.TryGetValue(keyToUse, out entry) == true;
    }

    /// <summary>
    ///     Builds validation error HTML markup if an error exists.
    /// </summary>
    protected string BuildErrorHtml(string propertyName)
    {
        if (TryGetModelState(propertyName, out var entry) && entry.Errors.Count > 0)
        {
            return entry.GetGovUkErrorHtml(ValidationMessage);
        }

        return string.Empty;
    }

    /// <summary>
    ///     Returns true if the model state contains errors for the given property.
    /// </summary>
    protected bool HasError(string propertyName)
    {
        return TryGetModelState(propertyName, out var entry) && entry.Errors.Count > 0;
    }

    /// <summary>
    ///     Builds the hint HTML using the appropriate ID.
    /// </summary>
    protected string BuildHintHtml(string fieldId)
    {
        string describedById = (LabelAriaDescribedBy, HintId) switch
        {
            var (label, _) when !string.IsNullOrEmpty(label) => label, // Use label aria-describedby if provided
            var (_, hint) when !string.IsNullOrEmpty(hint) => hint, // Use hint ID if provided
            _ => fieldId + "-hint" // Default to field ID with '-hint' suffix
        };

        return !string.IsNullOrWhiteSpace(HintHtml)
            ? $"<div id='{describedById}' class='govuk-hint'>{HintHtml}</div>"
            : string.Empty;
    }

    /// <summary>
    ///     Builds the GOV.UK label HTML markup with screen reader and visible versions.
    /// </summary>
    protected string BuildLabelHtml(string propertyName, string autoInputId, string fieldId)
    {
        string describedById = (LabelAriaDescribedBy, HintId) switch
        {
            var (label, _) when !string.IsNullOrEmpty(label) => label, // Use label aria-describedby if provided
            var (_, hint) when !string.IsNullOrEmpty(hint) => hint, // Use hint ID if provided
            _ => fieldId + "-hint" // Default to field ID with '-hint' suffix
        };

        var encodedLabel = HtmlEncoder.Default.Encode(LabelText ?? propertyName);

        return $@"
<label class='govuk-label govuk-label--s' for='{autoInputId}' aria-describedby='{describedById}' style='display:none'>{encodedLabel}</label>";
    }

    /// <summary>
    ///     Constructs the full CSS class string for the form group container.
    /// </summary>
    protected virtual string BuildFormGroupClass(string propertyName)
    {
        var classList = "govuk-form-group";

        if (ConditionalField)
        {
            classList += " conditional-field";

            if (!string.IsNullOrEmpty(ConditionalClass))
            {
                classList += $" {ConditionalClass}";
            }
        }

        if (HasError(propertyName))
        {
            classList += " govuk-form-group--error";
        }

        return classList;
    }

    /// <summary>
    ///     Applies common container-level attributes to the tag helper output.
    /// </summary>
    protected void SetContainerAttributes(TagHelperOutput output, string propertyName)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", BuildFormGroupClass(propertyName));

        if (!string.IsNullOrWhiteSpace(HtmlId))
        {
            output.Attributes.SetAttribute("id", HtmlId);
        }

        if (!string.IsNullOrWhiteSpace(DataParentsAttr))
        {
            output.Attributes.SetAttribute("data-parents", DataParentsAttr);
        }

        if (!string.IsNullOrWhiteSpace(DataQuestionIdAttr))
        {
            output.Attributes.SetAttribute("data-questionId", DataQuestionIdAttr);
        }
    }
}
