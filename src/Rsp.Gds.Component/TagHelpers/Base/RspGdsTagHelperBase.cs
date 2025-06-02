
namespace Rsp.Gds.Component.TagHelpers.Base;
public abstract class RspGdsTagHelperBase : TagHelper
{
    protected const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    [HtmlAttributeName("hint-html")]
    public string HintHtml { get; set; }

    [HtmlAttributeName("hint-id")]
    public string HintId { get; set; }

    [HtmlAttributeName("label-aria-describedby")]
    public string LabelAriaDescribedBy { get; set; }

    [HtmlAttributeName("validation-message")]
    public string ValidationMessage { get; set; }

    [HtmlAttributeName("error-key")]
    public string ErrorKey { get; set; }

    [HtmlAttributeName("field-id")]
    public string FieldId { get; set; }

    [HtmlAttributeName("id")]
    public string HtmlId { get; set; }

    [HtmlAttributeName("conditional")]
    public bool ConditionalField { get; set; }

    [HtmlAttributeName("dataparents-attr")]
    public string DataParentsAttr { get; set; }

    [HtmlAttributeName("dataquestionid-attr")]
    public string DataQuestionIdAttr { get; set; }
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

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    protected bool TryGetModelState(string propertyName, out ModelStateEntry entry)
    {
        entry = null;
        return ViewContext?.ViewData?.ModelState?.TryGetValue(ErrorKey ?? propertyName, out entry) == true;
    }

    protected string BuildErrorHtml(string propertyName)
    {
        if (TryGetModelState(propertyName, out var entry))
        {
            return entry.GetGovUkErrorHtml(ValidationMessage);
        }

        return string.Empty;
    }

    protected bool HasError(string propertyName)
    {
        return TryGetModelState(propertyName, out var entry) && entry.Errors.Count > 0;
    }

    protected string BuildHintHtml(string fieldId)
    {
        var describedById = !string.IsNullOrEmpty(LabelAriaDescribedBy)
            ? LabelAriaDescribedBy
            : !string.IsNullOrEmpty(HintId)
                ? HintId
                : fieldId + "-hint";

        return !string.IsNullOrWhiteSpace(HintHtml)
            ? $"<div id='{describedById}' class='govuk-hint'>{HintHtml}</div>"
            : string.Empty;
    }

    protected string BuildLabelHtml(string propertyName, string autoInputId, string hiddenInputId, string fieldId)
    {
        var describedById = !string.IsNullOrEmpty(LabelAriaDescribedBy)
            ? LabelAriaDescribedBy
            : !string.IsNullOrEmpty(HintId)
                ? HintId
                : fieldId + "-hint";

        var encodedLabel = HtmlEncoder.Default.Encode(LabelText ?? propertyName);

        return $@"
<label class='govuk-label js-hidden' for='{hiddenInputId}' aria-describedby='{describedById}'>{encodedLabel}</label>
<label class='govuk-label' for='{autoInputId}' aria-describedby='{describedById}'>{encodedLabel}</label>";
    }

    protected string BuildFormGroupClass(string propertyName)
    {
        var classList = "govuk-form-group";
        if (ConditionalField)
        {
            classList += " conditional-field";
        }

        if (HasError(propertyName))
        {
            classList += " govuk-form-group--error";
        }
        return classList;
    }

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