namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
/// Renders a GOV.UK-styled checkbox group with support for validation, conditional display, hints,
/// and custom attributes.
/// </summary>
[HtmlTargetElement("rsp-gds-checkbox-group", Attributes = ForAttributeName)]
public class RspGdsCheckboxGroupTagHelper : RspGdsTagHelperBase
{
    /// <summary>
    /// The list of string values to be rendered as checkbox options.
    /// </summary>
    [HtmlAttributeName("options")]
    public IEnumerable<string> Options { get; set; }

    [HtmlAttributeName("item-label-property")]
    public string ItemLabelProperty { get; set; }

    [HtmlAttributeName("item-value-property")]
    public string ItemValueProperty { get; set; }

    [HtmlAttributeName("item-hidden-properties")]
    public string ItemHiddenProperties { get; set; }

    [HtmlAttributeName("label-css-class")] public string LabelCssClass { get; set; }

    /// <summary>
    /// An optional CSS class applied for conditional display logic.
    /// </summary>
    [HtmlAttributeName("conditional-class")]
    public string ConditionalClass { get; set; }

    [HtmlAttributeName("legend-class")] public string LegendClass { get; set; } = "govuk-fieldset__legend--l";

    /// <summary>
    /// When true, all checkboxes are rendered as read-only (disabled).
    /// NOTE: disabled checkboxes do not post back, so hidden inputs are emitted to preserve model binding.
    /// </summary>
    [HtmlAttributeName("readonly")]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Comma-separated list of item identifiers that should be rendered as read-only. e.g.
    /// "organisation_admin,sponsor" or "Guid1,Guid2"
    /// </summary>
    [HtmlAttributeName("readonly-items")]
    public string? ReadOnlyItems { get; set; }

    /// <summary>
    /// Name of the property on complex items to match against the readonly-items list. Defaults to
    /// "Name" when not provided.
    /// </summary>
    [HtmlAttributeName("readonly-item-property")]
    public string? ReadOnlyItemProperty { get; set; }

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

        // Readonly selector set (case-insensitive)
        var readonlySet = (ReadOnlyItems ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        bool IsComplexItemReadOnly(object item)
        {
            if (ReadOnly)
            {
                return true;
            }

            if (readonlySet.Count == 0)
            {
                return false;
            }

            var type = item.GetType();

            // Match property: explicit attribute else "Name"
            var matchPropName = !string.IsNullOrWhiteSpace(ReadOnlyItemProperty)
                ? ReadOnlyItemProperty!
                : "Name";

            // Try requested prop -> Name -> label prop -> DisplayName -> Id
            var matchProp =
                type.GetProperty(matchPropName) ??
                type.GetProperty("Name") ??
                (!string.IsNullOrWhiteSpace(ItemLabelProperty) ? type.GetProperty(ItemLabelProperty) : null) ??
                type.GetProperty("DisplayName") ??
                type.GetProperty("Id");

            var matchValue = matchProp?.GetValue(item)?.ToString();

            return !string.IsNullOrWhiteSpace(matchValue) && readonlySet.Contains(matchValue);
        }

        bool IsOptionReadOnly(string optionValue)
        {
            if (ReadOnly) return true;
            if (readonlySet.Count == 0) return false;
            return readonlySet.Contains(optionValue);
        }

        // Case 1: Simple string list binding
        if (Options?.Any() == true && (For.Model == null || For.Model is IEnumerable<string>))
        {
            var selectedValues = For.Model as IEnumerable<string> ?? Enumerable.Empty<string>();

            foreach (var option in Options)
            {
                var isSelected = selectedValues.Contains(option, StringComparer.OrdinalIgnoreCase);
                var isCheckedAttr = isSelected ? "checked" : "";
                var safeId = $"{propertyName}_{option.Replace(" ", "_")}";

                var optionReadOnly = IsOptionReadOnly(option);
                var disabledAttr = optionReadOnly ? "disabled" : "";

                // Disabled inputs don't post, so preserve selected options with hidden inputs (only
                // needed when option is readonly AND selected)
                var hiddenSelected = optionReadOnly && isSelected
                    ? $"<input type='hidden' name='{propertyName}' value='{option}' />"
                    : "";

                checkboxesHtml.Add($@"
                    <div class='govuk-checkboxes__item'>
                        {hiddenSelected}
                        <input class='govuk-checkboxes__input' id='{safeId}' name='{propertyName}' type='checkbox' value='{option}' {isCheckedAttr} {disabledAttr} />
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
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
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
                var isCheckedBool = (bool?)valueProp?.GetValue(item) == true;
                var isCheckedAttr = isCheckedBool ? "checked" : "";

                var checkboxId = $"{propertyName}_{index}__{ItemValueProperty}";
                var checkboxName = $"{propertyName}[{index}].{ItemValueProperty}";

                var itemReadOnly = IsComplexItemReadOnly(item);
                var disabledAttr = itemReadOnly ? "disabled" : "";

                var inputs = new List<string>();

                // Hidden properties always posted
                foreach (var hiddenProp in hiddenProps)
                {
                    var prop = type.GetProperty(hiddenProp);
                    if (prop != null)
                    {
                        var value = prop.GetValue(item)?.ToString() ?? "";
                        var name = $"{propertyName}[{index}].{hiddenProp}";
                        inputs.Add($"<input type='hidden' name='{name}' value='{value}' />");
                    }
                }

                // Preserve checkbox value on post when disabled/read-only
                if (itemReadOnly)
                {
                    inputs.Add(
                        $"<input type='hidden' name='{checkboxName}' value='{(isCheckedBool ? "true" : "false")}' />");
                }

                inputs.Add(
                    $"<input class='govuk-checkboxes__input' id='{checkboxId}' name='{checkboxName}' type='checkbox' value='true' {isCheckedAttr} {disabledAttr} />");

                inputs.Add(
                    $"<label class='govuk-label govuk-checkboxes__label {LabelCssClass}' for='{checkboxId}'>{label}</label>");

                checkboxesHtml.Add($"<div class='govuk-checkboxes__item'>\n{string.Join("\n", inputs)}\n</div>");
                index++;
            }
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", !string.IsNullOrWhiteSpace(HtmlId) ? HtmlId : propertyName);

        output.Content.SetHtmlContent($@"
            <fieldset class=""govuk-fieldset"">
                <legend class='{LegendClass} {LabelCssClass}'>
                    {LabelText}
                </legend>
                {hintHtml}
                {errorHtml}
                <div class='govuk-checkboxes' data-module='govuk-checkboxes' id='{propertyName}_checkboxes'>
                    {string.Join("\n", checkboxesHtml)}
                </div>
            </fieldset>");
    }
}