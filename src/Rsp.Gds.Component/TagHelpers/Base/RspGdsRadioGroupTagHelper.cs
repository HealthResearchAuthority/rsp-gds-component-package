using Castle.Components.DictionaryAdapter.Xml;
using Rsp.Gds.Component.ModelStateExtensions;

namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled radio button group for a model-bound property.
///     Supports label text, dynamic options, conditional visibility, validation, hints, and more.
/// </summary>
[HtmlTargetElement("rsp-gds-radio-group", Attributes = ForAttributeName)]
public class RspGdsRadioGroupTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     The model expression this radio group is bound to.
    ///     Used to generate name, value and validation bindings.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    ///     Text to display as the group label above the radio options.
    /// </summary>
    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    /// <summary>
    ///     The radio options to render.
    /// </summary>
    [HtmlAttributeName("options")]
    public IEnumerable<GdsOption> Options { get; set; }

    /// <summary>
    ///     Adds a conditional field class for toggling visibility via JS.
    /// </summary>
    [HtmlAttributeName("conditional-field")]
    public bool ConditionalField { get; set; } = false;

    /// <summary>
    ///     Optional HTML hint displayed below the label.
    /// </summary>
    [HtmlAttributeName("hint-html")]
    public string HintHtml { get; set; }

    /// <summary>
    ///     Custom validation message. Defaults to the first model error.
    /// </summary>
    [HtmlAttributeName("validation-message")]
    public string ValidationMessage { get; set; }

    /// <summary>
    ///     CSS class for the legend element (e.g. govuk-fieldset__legend--l).
    /// </summary>
    [HtmlAttributeName("legend-class")]
    public string LegendClass { get; set; } = "govuk-fieldset__legend--l";

    /// <summary>
    ///     Optional CSS class for each radio label.
    /// </summary>
    [HtmlAttributeName("label-css-class")]
    public string LabelCssClass { get; set; }

    /// <summary>
    ///     Additional CSS class applied to the container when conditional logic is used.
    /// </summary>
    [HtmlAttributeName("conditional-class")]
    public string ConditionalClass { get; set; }

    /// <summary>
    ///     Sets the data-parents attribute on the container for client-side logic.
    /// </summary>
    [HtmlAttributeName("dataparents-attr")]
    public string DataParentsAttr { get; set; }

    /// <summary>
    ///     Sets the data-questionId attribute on the container.
    /// </summary>
    [HtmlAttributeName("dataquestionid-attr")]
    public string DataQuestionIdAttr { get; set; }

    /// <summary>
    ///     Used to generate unique IDs per radio item (appended to option value).
    /// </summary>
    [HtmlAttributeName("question-id")]
    public string QuestionId { get; set; }

    /// <summary>
    ///     HTML ID override for the form group container.
    /// </summary>
    [HtmlAttributeName("id")]
    public string HtmlId { get; set; }

    /// <summary>
    ///     Optional override class for the radio wrapper div (e.g. govuk-radios--inline).
    /// </summary>
    [HtmlAttributeName("div-inline-class")]
    public string DivInlineClass { get; set; }

    /// <summary>
    ///     JS module name for govuk radio interaction. Defaults to 'govuk-radios'.
    /// </summary>
    [HtmlAttributeName("datamodule")]
    public string DataModule { get; set; } = "govuk-radios";

    /// <summary>
    ///     Comma-separated list of hidden property names to render as hidden fields alongside each radio item.
    /// </summary>
    [HtmlAttributeName("item-hidden-properties")]
    public string ItemHiddenProperties { get; set; }

    /// <summary>
    ///     A model collection to pull values from for hidden fields (indexed per radio option).
    /// </summary>
    [HtmlAttributeName("hidden-model")]
    public IEnumerable<object> HiddenModel { get; set; }

    /// <summary>
    ///     Injected view context.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        // Get the selected value from the model (string or first from list)
        var selectedValue = For.Model switch
        {
            string str => str,
            IEnumerable<string> list => list.FirstOrDefault(),
            _ => For.Model?.ToString()
        };

        // Check for validation errors
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var entry);
        var hasError = entry != null && entry.Errors.Count > 0;

        // Render a validation error message span if applicable
        var errorHtml = hasError ? entry.GetGovUkErrorHtml(ValidationMessage) : "";

        var hintHtml = !string.IsNullOrWhiteSpace(HintHtml)
            ? $"<div class='govuk-hint'>{HintHtml}</div>"
            : "";

        // Compose form group class
        var formGroupClass = "govuk-form-group"
                             + (ConditionalField ? " conditional govuk-radios__conditional" : "")
                             + (hasError ? " govuk-form-group--error" : "");

        // Configure output container
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", formGroupClass);
        output.Attributes.SetAttribute("id", !string.IsNullOrWhiteSpace(HtmlId) ? HtmlId : propertyName);

        if (!string.IsNullOrWhiteSpace(DataParentsAttr))
            output.Attributes.SetAttribute("data-parents", DataParentsAttr);

        if (!string.IsNullOrWhiteSpace(DataQuestionIdAttr))
            output.Attributes.SetAttribute("data-questionId", DataQuestionIdAttr);

        // Parse any hidden properties to render per option
        var hiddenProps = (ItemHiddenProperties ?? "")
            .Split(',')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();

        var radiosHtmlBuilder = new StringBuilder();
        var index = 0;

        foreach (var option in Options)
        {
            var inputId = $"{QuestionId}_{option.Value.Replace(" ", "_")}";
            var isChecked = string.Equals(
                selectedValue?.Trim(),
                option.Value?.Trim(),
                StringComparison.OrdinalIgnoreCase
            ) ? "checked=\"checked\"" : "";

            var radioHtml = $@"
        <div class='govuk-radios__item'>
            <input class='govuk-radios__input'
                   id='{inputId}'
                   name='{propertyName}'
                   type='radio'
                   value='{option.Value}'
                   {isChecked} />
            <label class='govuk-label govuk-radios__label {LabelCssClass}' for='{inputId}'>
                {option.Label}
            </label>";

            // Render hidden inputs if provided
            if (HiddenModel != null && HiddenModel.Count() > index && hiddenProps.Any())
            {
                var modelItem = HiddenModel.ElementAt(index);
                var modelType = modelItem.GetType();

                foreach (var hiddenProp in hiddenProps)
                {
                    var prop = modelType.GetProperty(hiddenProp);
                    if (prop != null)
                    {
                        var value = prop.GetValue(modelItem)?.ToString() ?? "";
                        var name = $"{propertyName.Replace("SelectedOption", "Answers")}[{index}].{hiddenProp}";
                        radioHtml += $"\n<input type='hidden' name='{name}' value='{HtmlEncoder.Default.Encode(value)}' />";
                    }
                }
            }

            radioHtml += "\n</div>";
            radiosHtmlBuilder.AppendLine(radioHtml);
            index++;
        }

        var radiosHtml = radiosHtmlBuilder.ToString();

        // Determine wrapper class (e.g. inline or standard radio list)
        var radioWrapperClass = !string.IsNullOrWhiteSpace(DivInlineClass)
            ? DivInlineClass
            : "govuk-radios";

        // Build final fieldset HTML
        var fieldsetHtml = $@"
            <govuk-fieldset>
                <govuk-fieldset-legend class='{LegendClass}'>
                    {LabelText ?? propertyName}
                </govuk-fieldset-legend>
                {hintHtml}
                {errorHtml}
                <div class='{radioWrapperClass}' data-module='{DataModule}'>
                    {radiosHtml}
                </div>
            </govuk-fieldset>";

        output.Content.SetHtmlContent(fieldsetHtml);
    }
}
