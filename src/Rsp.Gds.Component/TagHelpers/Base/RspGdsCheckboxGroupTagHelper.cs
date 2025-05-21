using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;

namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled checkbox group with support for validation,
///     conditional display, hints, and custom attributes.
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

    [HtmlAttributeName("conditional-class")]
    public string ConditionalClass { get; set; }

    [HtmlAttributeName("dataparents-attr")]
    public string DataParentsAttr { get; set; }

    [HtmlAttributeName("dataquestionid-attr")]
    public string DataQuestionIdAttr { get; set; }

    [HtmlAttributeName("validation-message")]
    public string ValidationMessage { get; set; }

    [HtmlAttributeName("hint-html")]
    public string HintHtml { get; set; }

    [HtmlAttributeName("id")]
    public string HtmlId { get; set; }
    /// <summary>
    /// Optional override for the legend size class. Defaults to 'govuk-fieldset__legend--l'.
    /// </summary>
    [HtmlAttributeName("legend-class")]
    public string LegendClass { get; set; } = "govuk-fieldset__legend--l";


    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
      
        // Validation
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var modelStateEntry);
        var hasError = modelStateEntry?.Errors?.Count > 0;

        // Error message
        var errorMessage = !string.IsNullOrEmpty(ValidationMessage)
            ? ValidationMessage
            : modelStateEntry?.Errors.FirstOrDefault()?.ErrorMessage;

        var errorHtml = hasError && !string.IsNullOrWhiteSpace(errorMessage)
            ? $"<span class='govuk-error-message'>{HtmlEncoder.Default.Encode(errorMessage)}</span>"
            : "";

        var hintHtml = !string.IsNullOrWhiteSpace(HintHtml)
            ? $"<div class='govuk-hint'>{HintHtml}</div>"
            : "";

        var formGroupClass = "govuk-form-group"
                             + (ConditionalField ? " conditional-field" : "")
                             + (hasError ? " govuk-form-group--error" : "");

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
       
        output.Attributes.SetAttribute("class", formGroupClass);

        var fieldId = !string.IsNullOrWhiteSpace(HtmlId) ? HtmlId : propertyName;
        output.Attributes.SetAttribute("id", fieldId);

        if (!string.IsNullOrWhiteSpace(ConditionalClass))
        {
            output.Attributes.SetAttribute("conditional-class", ConditionalClass);
        }

        if (!string.IsNullOrWhiteSpace(DataParentsAttr))
        {
            output.Attributes.SetAttribute("data-parents", DataParentsAttr);
        }

        if (!string.IsNullOrWhiteSpace(DataQuestionIdAttr))
        {
            output.Attributes.SetAttribute("data-questionId", DataQuestionIdAttr);
        }

        var checkboxesHtml = new List<string>();

        // Simple list (string values)
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
        // Complex model object list
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