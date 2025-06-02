namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled checkbox group with support for validation,
///     conditional display, hints, and custom attributes.
/// </summary>
[HtmlTargetElement("rsp-gds-checkbox-group", Attributes = ForAttributeName)]
public class RspGdsCheckboxGroupTagHelper : RspGdsTagHelperBase
{
    /// <summary>
    ///     The list of string values to be rendered as checkbox options.
    /// </summary>
    [HtmlAttributeName("options")]
    public IEnumerable<string> Options { get; set; }

    [HtmlAttributeName("item-label-property")]
    public string ItemLabelProperty { get; set; }

    [HtmlAttributeName("item-value-property")]
    public string ItemValueProperty { get; set; }

    [HtmlAttributeName("item-hidden-properties")]
    public string ItemHiddenProperties { get; set; }

    [HtmlAttributeName("label-css-class")]
    public string LabelCssClass { get; set; }

    /// <summary>
    ///     An optional CSS class applied for conditional display logic.
    /// </summary>
    [HtmlAttributeName("conditional-class")]

    public string ConditionalClass { get; set; }

    [HtmlAttributeName("legend-class")]
    public string LegendClass { get; set; } = "govuk-fieldset__legend--l";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var fieldId = !string.IsNullOrWhiteSpace(HtmlId) ? HtmlId : propertyName;

        SetContainerAttributes(output, propertyName);
        if (!string.IsNullOrWhiteSpace(ConditionalClass))
        {
            output.Attributes.SetAttribute("conditional-class", ConditionalClass);
        }

        var errorHtml = BuildErrorHtml(propertyName);
        var hintHtml = BuildHintHtml(fieldId);
        var checkboxesHtml = new List<string>();

        // Case 1: Simple string list binding
        if (Options?.Any() == true && (For.Model == null || For.Model is IEnumerable<string>))
        {
            var selectedValues = For.Model as IEnumerable<string> ?? Enumerable.Empty<string>();

            foreach (var option in Options)
            {
                var isChecked = selectedValues.Contains(option, StringComparer.OrdinalIgnoreCase) ? "checked" : "";
                var safeId = $"{propertyName}_{option.Replace(" ", "_")}";

                checkboxesHtml.Add($@"
                    <div class='govuk-checkboxes__item'>
                        <input class='govuk-checkboxes__input' id='{safeId}' name='{propertyName}' type='checkbox' value='{option}' {isChecked} />
                        <label class='govuk-label govuk-checkboxes__label {LabelCssClass}' for='{safeId}'>{option}</label>
                    </div>");
            }
        }
        // Case 2: Complex object model binding
        else if (For.Model is IEnumerable<object> complexItems &&
                 !string.IsNullOrEmpty(ItemLabelProperty) &&
                 !string.IsNullOrEmpty(ItemValueProperty))
        {
            var hiddenProps = (ItemHiddenProperties ?? "")
                .Split(',')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            var index = 0;
            foreach (var item in complexItems)
            {
                var type = item.GetType();
                var labelProp = type.GetProperty(ItemLabelProperty);
                var valueProp = type.GetProperty(ItemValueProperty);

                var label = labelProp?.GetValue(item)?.ToString()?.Replace("_", " ") ?? $"Item {index}";
                var isChecked = (bool?)valueProp?.GetValue(item) == true ? "checked" : "";

                var checkboxId = $"{propertyName}_{index}__{ItemValueProperty}";
                var checkboxName = $"{propertyName}[{index}].{ItemValueProperty}";

                var inputs = new List<string>
                {
                    $"<input class='govuk-checkboxes__input' id='{checkboxId}' name='{checkboxName}' type='checkbox' value='true' {isChecked} />",
                    $"<label class='govuk-label govuk-checkboxes__label {LabelCssClass}' for='{checkboxId}'>{label}</label>"
                };

                foreach (var hiddenProp in hiddenProps)
                {
                    var prop = type.GetProperty(hiddenProp);
                    if (prop != null)
                    {
                        var value = prop.GetValue(item)?.ToString() ?? "";
                        var name = $"{propertyName}[{index}].{hiddenProp}";
                        inputs.Insert(0, $"<input type='hidden' name='{name}' value='{value}' />");
                    }
                }

                checkboxesHtml.Add($"<div class='govuk-checkboxes__item'>\n{string.Join("\n", inputs)}\n</div>");
                index++;
            }
        }

        output.Content.SetHtmlContent($@"
<govuk-fieldset>
    <govuk-fieldset-legend class='{LegendClass}'>
        {LabelText}
    </govuk-fieldset-legend>
    {hintHtml}
    {errorHtml}
    <div class='govuk-checkboxes' data-module='govuk-checkboxes' id='{propertyName}_checkboxes'>
        {string.Join("\n", checkboxesHtml)}
    </div>
</govuk-fieldset>");
    }
}
