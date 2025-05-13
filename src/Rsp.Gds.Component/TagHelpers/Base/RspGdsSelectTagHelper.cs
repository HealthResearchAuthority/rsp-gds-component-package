using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
/// Renders a GOV.UK-styled <select> dropdown for a model-bound property.
/// Supports optional default option, validation error display, and custom label text.
/// </summary>
[HtmlTargetElement("rsp-gds-select", Attributes = ForAttributeName)]
public class RspGdsSelectTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    /// <summary>
    /// The model expression this select input is bound to.
    /// Used to determine the selected value and input name/id.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    /// The label text displayed above the select dropdown.
    /// If not provided, the model property name is used.
    /// </summary>
    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    /// <summary>
    /// The list of options to populate the dropdown with.
    /// Each option includes a Value and a Label.
    /// </summary>
    [HtmlAttributeName("options")]
    public IEnumerable<GdsOption> Options { get; set; }

    /// <summary>
    /// Whether to include a default disabled option prompting user selection.
    /// Defaults to true.
    /// </summary>
    [HtmlAttributeName("include-default-option")]
    public bool IncludeDefaultOption { get; set; } = true;

    /// <summary>
    /// The text to display for the default option.
    /// Only used if IncludeDefaultOption is true.
    /// Defaults to "Please select...".
    /// </summary>
    [HtmlAttributeName("default-option-text")]
    public string DefaultOptionText { get; set; } = "Please select...";

    /// <summary>
    /// Provides access to the current view context, including model state for validation.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var selectedValue = For.Model?.ToString();

        // Retrieve model state to determine if there are validation errors
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var modelStateEntry);
        var hasError = modelStateEntry?.Errors?.Count > 0;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"govuk-form-group {(hasError ? "govuk-form-group--error" : "")}");

        // Label HTML
        var labelHtml = $@"
                <label class='govuk-label govuk-label--s' for='{propertyName}'>
                    {LabelText ?? propertyName}
                </label>";

        // Error message HTML if applicable
        var errorHtml = hasError
            ? $"<span class='govuk-error-message'>{modelStateEntry.Errors[0].ErrorMessage}</span>"
            : "";

        // Build <option> tags
        var optionsHtml = IncludeDefaultOption
            ? $"<option value='' disabled {(string.IsNullOrEmpty(selectedValue) ? "selected" : "")}>{DefaultOptionText}</option>"
            : "";

        optionsHtml += string.Join("\n", Options.Select(option =>
        {
            var selectedAttr = string.Equals(
                selectedValue?.Trim(),
                option.Value?.Trim(),
                StringComparison.OrdinalIgnoreCase
            )
                ? "selected"
                : "";

            var optionId = $"{propertyName}_{option.Value.Replace(" ", "_")}";

            return $@"<option id='{optionId}' value='{option.Value}' {selectedAttr}>{option.Label}</option>";
        }));

        // Final <select> HTML
        var selectHtml = $@"
                <select id='{propertyName}' name='{propertyName}' class='govuk-select'>
                    {optionsHtml}
                </select>";

        output.Content.SetHtmlContent(labelHtml + errorHtml + selectHtml);
    }
}
