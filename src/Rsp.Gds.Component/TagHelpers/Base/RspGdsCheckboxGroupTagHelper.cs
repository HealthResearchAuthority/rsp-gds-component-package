namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled checkbox group based on a model-bound property and a list of options.
///     Displays validation errors and applies appropriate GOV.UK form styling.
/// </summary>
[HtmlTargetElement("rsp-gds-checkbox-group", Attributes = ForAttributeName)]
public class RspGdsCheckboxGroupTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     The model expression this checkbox group is bound to.
    ///     Used to get the selected values and bind input names.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    ///     The text label displayed above the checkbox group.
    ///     If not provided, the property name will be used.
    /// </summary>
    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    /// <summary>
    ///     The list of string values to be rendered as checkbox options.
    /// </summary>
    [HtmlAttributeName("options")]
    public IEnumerable<string> Options { get; set; }

    /// <summary>
    ///     The name of the property to use as the checkbox label when rendering a list of complex objects.
    ///     Each checkbox label will use this property’s value from the bound model item.
    /// </summary>
    [HtmlAttributeName("item-label-property")]
    public string ItemLabelProperty { get; set; }

    /// <summary>
    ///     The name of the boolean property that determines whether a checkbox is checked for each complex object.
    ///     Each object in the bound model should have this property for binding.
    /// </summary>
    [HtmlAttributeName("item-value-property")]
    public string ItemValueProperty { get; set; }

    /// <summary>
    ///     A comma-separated list of property names on each complex object that should be rendered as hidden fields.
    ///     These fields will be preserved during form submissions.
    /// </summary>
    [HtmlAttributeName("item-hidden-properties")]
    public string ItemHiddenProperties { get; set; }

    /// <summary>
    ///     An optional CSS class to apply to each checkbox label element.
    ///     Useful for custom styling or GOV.UK design system extensions.
    /// </summary>
    [HtmlAttributeName("label-css-class")]
    public string LabelCssClass { get; set; }

    /// <summary>
    ///     Indicates whether the checkbox group is conditionally shown or toggled based on another form input.
    ///     Adds a <c>conditional-field</c> CSS class to the form group container.
    /// </summary>
    [HtmlAttributeName("conditional-field")]
    public bool ConditionalField { get; set; } = false;

    /// <summary>
    ///     Provides context about the current view, including model state for validation messages.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        // Retrieve model state entry for validation errors
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var modelStateEntry);
        var hasError = modelStateEntry != null && modelStateEntry.Errors.Count > 0;

        // Render error message span if applicable
        var errorHtml = hasError
            ? $"<span class='govuk-error-message'>{modelStateEntry.Errors[0].ErrorMessage}</span>"
            : "";

        // Build the form group class with conditional and error styling
        var formGroupClass = "govuk-form-group"
                             + (ConditionalField ? " conditional-field" : "")
                             + (hasError ? " govuk-form-group--error" : "");

        // Set up container element
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", propertyName);
        output.Attributes.SetAttribute("class", formGroupClass);

        var checkboxesHtml = new List<string>();

        // Case 1: Simple checkbox list using string options
        if (Options?.Any() == true && (For.Model == null || For.Model is IEnumerable<string>))
        {
            var selectedStrings = For.Model as IEnumerable<string> ?? Enumerable.Empty<string>();

            checkboxesHtml.AddRange(Options.Select(option =>
            {
                var isChecked = selectedStrings.Any(s => string.Equals(s, option, StringComparison.OrdinalIgnoreCase))
                    ? "checked"
                    : "";

                // Sanitize ID for HTML
                var safeId = $"{propertyName}_{option.Replace(" ", "_".ToLower())}";

                return $@"
                    <div class='govuk-checkboxes__item'>
                        <input class='govuk-checkboxes__input' id='{safeId}' name='{propertyName}' type='checkbox' value='{option}' {isChecked} />
                        <label class='govuk-label govuk-checkboxes__label {LabelCssClass}' for='{safeId}'>{option}</label>
                    </div>";
            }));
        }
        // Case 2: Complex object checkbox list
        else if (For.Model is IEnumerable<object> complexItems &&
                 !string.IsNullOrEmpty(ItemLabelProperty) &&
                 !string.IsNullOrEmpty(ItemValueProperty))
        {
            // Parse and clean hidden field definitions
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

                // Format input names/IDs to support model binding
                var checkboxId = $"{propertyName}_{index}__{ItemValueProperty}";
                var checkboxName = $"{propertyName}[{index}].{ItemValueProperty}";

                var inputs = new List<string>
                {
                    $"<input class='govuk-checkboxes__input' id='{checkboxId}' name='{checkboxName}' type='checkbox' value='true' {isChecked} />",
                    $"<label class='govuk-label govuk-checkboxes__label {LabelCssClass}' for='{checkboxId}'>{label}</label>"
                };

                // Add any hidden fields required for postback
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

        // Final HTML output with fieldset/legend/checkboxes
        output.Content.SetHtmlContent($@"
            <fieldset class='govuk-fieldset'>
                <legend class='govuk-fieldset__legend govuk-fieldset__legend--l'>
                    <label class='govuk-label govuk-label--s' for='{propertyName}'>
                        {LabelText ?? propertyName}
                    </label>
                </legend>
                {errorHtml}
                <div class='govuk-checkboxes' data-module='govuk-checkboxes' id='{propertyName}_checkboxes'>
                    {string.Join("\n", checkboxesHtml)}
                </div>
            </fieldset>");
    }
}