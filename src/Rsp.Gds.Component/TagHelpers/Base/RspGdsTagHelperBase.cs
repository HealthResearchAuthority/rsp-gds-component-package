using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Base class for GOV.UK-styled TagHelpers.
///     Provides shared logic for validation, hint rendering, label generation, conditional display, and container attribute setup.
/// </summary>
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

    [HtmlAttributeName("conditional-field")]
    public bool ConditionalField { get; set; }

    [HtmlAttributeName("conditional-class")]
    public string ConditionalClass { get; set; }

    [HtmlAttributeName("dataparents-attr")]
    public string DataParentsAttr { get; set; }

    [HtmlAttributeName("dataquestionid-attr")]
    public string DataQuestionIdAttr { get; set; }

    [HtmlAttributeName("readonly")]
    public bool Readonly { get; set; } = false;

    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; } = false;

    [HtmlAttributeName("additional-attributes")]
    public IDictionary<string, string> AdditionalAttributes { get; set; } = new Dictionary<string, string>();

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
        var describedById = !string.IsNullOrEmpty(LabelAriaDescribedBy)
            ? LabelAriaDescribedBy
            : !string.IsNullOrEmpty(HintId)
                ? HintId
                : fieldId + "-hint";

        return !string.IsNullOrWhiteSpace(HintHtml)
            ? $"<div id='{describedById}' class='govuk-hint'>{HintHtml}</div>"
            : string.Empty;
    }

    /// <summary>
    ///     Builds the GOV.UK label HTML markup with screen reader and visible versions.
    /// </summary>
    protected string BuildLabelHtml(string propertyName, string autoInputId, string hiddenInputId, string fieldId)
    {
        var describedById = !string.IsNullOrEmpty(LabelAriaDescribedBy)
            ? LabelAriaDescribedBy
            : !string.IsNullOrEmpty(HintId)
                ? HintId
                : fieldId + "-hint";

        var encodedLabel = HtmlEncoder.Default.Encode(LabelText ?? propertyName);

        return $@"
<label class='govuk-label' for='{autoInputId}' aria-describedby='{describedById}'>{encodedLabel}</label>";
    }

    /// <summary>
    ///     Constructs the full CSS class string for the form group container.
    /// </summary>
    protected virtual string BuildFormGroupClass(string propertyName)
    {
        var classList = "govuk-form-group";

        if (ConditionalField && !string.IsNullOrEmpty(ConditionalClass))
        {
            classList += $" {ConditionalClass}";
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
