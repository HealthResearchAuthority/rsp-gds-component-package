namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled radio button group for a model-bound property.
///     Supports label text, dynamic options, conditional visibility, validation, hints, and more.
/// </summary>
[HtmlTargetElement("rsp-gds-radio-group", Attributes = ForAttributeName)]
public class RspGdsRadioGroupTagHelper : RspGdsTagHelperBase
{
    /// <summary>
    ///     The radio options to render.
    /// </summary>
    [HtmlAttributeName("options")]
    public IEnumerable<GdsOption> Options { get; set; }

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
    ///     Used to generate unique IDs per radio item (appended to option value).
    /// </summary>
    [HtmlAttributeName("question-id")]
    public string QuestionId { get; set; }

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
        var fieldId = !string.IsNullOrEmpty(FieldId) ? FieldId : propertyName;

        SetContainerAttributes(output, propertyName);

        // Get the selected value from the model (string or first from list)
        var selectedValue = For.Model switch
        {
            string str => str,
            IEnumerable<string> list => list.FirstOrDefault(),
            _ => For.Model?.ToString()
        };

        // Try to get validation state
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var modelStateEntry);
        var hasError = modelStateEntry != null && modelStateEntry.Errors.Count > 0;

        // Determine error message
        var errorHtml = BuildErrorHtml(propertyName);
        var hintHtml = BuildHintHtml(fieldId);

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
        {
            output.Attributes.SetAttribute("data-parents", DataParentsAttr);
        }

        if (!string.IsNullOrWhiteSpace(DataQuestionIdAttr))
        {
            output.Attributes.SetAttribute("data-questionId", DataQuestionIdAttr);
        }

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
                <govuk-fieldset-legend class='{LegendClass} {LabelCssClass}'>
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
