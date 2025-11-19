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
        // Use full field name so ModelState keys match when using templates/prefixes
        var fullName = ViewContext?.ViewData?.TemplateInfo?.GetFullHtmlFieldName(For.Name) ?? For.Name;

        // Stable/safe id to anchor hint/error IDs
        var baseId = !string.IsNullOrEmpty(FieldId) ? FieldId : fullName;
        var fieldId = TagBuilder.CreateSanitizedId(baseId, "_");

        SetContainerAttributes(output, fullName);

        // Get selected value from model
        var selectedValue = For.Model switch
        {
            string str => str,
            IEnumerable<string> list => list.FirstOrDefault(),
            _ => For.Model?.ToString()
        };

        // Validation state (lookup by fullName)
        ViewContext.ViewData.ModelState.TryGetValue(fullName, out var modelStateEntry);
        var hasError = modelStateEntry?.Errors?.Count > 0;

        // Build hint/error HTML (tie to fieldId so aria-describedby can reference them)
        var hintHtml = BuildHintHtml(fieldId);
        var hintId = !string.IsNullOrWhiteSpace(hintHtml) ? $"{fieldId}-hint" : null;

        var errorHtml = BuildErrorHtml(fullName);
        if (hasError && string.IsNullOrWhiteSpace(errorHtml))
        {
            var firstError = modelStateEntry!.Errors[0].ErrorMessage;
            errorHtml =
                $@"<p id=""{fieldId}-error"" class=""govuk-error-message"">
                  <span class=""govuk-visually-hidden"">Error:</span> {HtmlEncoder.Default.Encode(firstError)}
               </p>";
        }
        var errorId = hasError ? $"{fieldId}-error" : null;

        var formGroupClass = "govuk-form-group"
                             + (ConditionalField ? " conditional" : "")
                             + (hasError ? " govuk-form-group--error" : "");

        // Container
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", formGroupClass);
        output.Attributes.SetAttribute("id", !string.IsNullOrWhiteSpace(HtmlId) ? HtmlId : fieldId);

        if (!string.IsNullOrWhiteSpace(DataParentsAttr))
            output.Attributes.SetAttribute("data-parents", DataParentsAttr);

        if (!string.IsNullOrWhiteSpace(DataQuestionIdAttr))
            output.Attributes.SetAttribute("data-questionId", DataQuestionIdAttr);

        // Parse hidden props
        var hiddenProps = (ItemHiddenProperties ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        // Radios
        var radiosHtmlBuilder = new StringBuilder();
        var index = 0;

        // Shared described-by for the group
        var describedBy = string.Join(" ", new[] { hintId, errorId }.Where(s => !string.IsNullOrEmpty(s)));

        foreach (var option in Options ?? Enumerable.Empty<GdsOption>())
        {
            var safeVal = option.Value ?? string.Empty;
            var idBase = string.IsNullOrWhiteSpace(QuestionId) ? fieldId : QuestionId;
            var inputId = $"{idBase}_{TagBuilder.CreateSanitizedId(safeVal, "_")}";

            var isChecked = string.Equals(
                selectedValue?.Trim(),
                option.Value?.Trim(),
                StringComparison.OrdinalIgnoreCase
            );

            var radioHtml = $@"
        <div class='govuk-radios__item'>
            <input class='govuk-radios__input'
                   id='{inputId}'
                   name='{fullName}'
                   type='radio'
                   value='{HtmlEncoder.Default.Encode(safeVal)}'
                   {(isChecked ? "checked=\"checked\"" : "")}
                   {(hasError ? "aria-invalid=\"true\"" : "")}
                   {(string.IsNullOrEmpty(describedBy) ? "" : $@"aria-describedby=""{describedBy}""")} />
            <label class='govuk-label govuk-radios__label {LabelCssClass}' for='{inputId}'>
                {option.Label}
            </label>";

            // Per-item hidden inputs
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
                        var name = $"{fullName.Replace("SelectedOption", "Answers")}[{index}].{hiddenProp}";
                        radioHtml += $"\n<input type='hidden' name='{name}' value='{HtmlEncoder.Default.Encode(value)}' />";
                    }
                }
            }

            radioHtml += "\n</div>";
            radiosHtmlBuilder.AppendLine(radioHtml);
            index++;
        }

        var radiosHtml = radiosHtmlBuilder.ToString();
        var radioWrapperClass = !string.IsNullOrWhiteSpace(DivInlineClass) ? DivInlineClass : "govuk-radios";

        // Final markup (keep your existing elements)
        var fieldsetHtml = $@"
            <fieldset class=""govuk-fieldset"" {(string.IsNullOrEmpty(describedBy) ? "" : $@"aria-describedby=""{describedBy}""")}>
                <legend class='{LegendClass} {LabelCssClass}'>
                    {LabelText ?? fullName}
                </legend>
                {hintHtml}
                {errorHtml}
                <div class='{radioWrapperClass}' data-module='{DataModule}'>
                    {radiosHtml}
                </div>
            </fieldset>";

        output.Content.SetHtmlContent(fieldsetHtml);
    }
}