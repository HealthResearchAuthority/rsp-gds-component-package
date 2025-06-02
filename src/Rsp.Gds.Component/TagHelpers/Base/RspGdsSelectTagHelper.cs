using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.AspNetCore.Components.Forms;

namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled
///     <select>
///         dropdown for a model-bound property.
///         Supports optional default option, validation error display, custom label text, and conditional styling.
/// </summary>
[HtmlTargetElement("rsp-gds-select", Attributes = ForAttributeName)]
public class RspGdsSelectTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     The model expression this select input is bound to.
    ///     Used to determine the selected value and input name/id.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    ///     The label text displayed above the select dropdown.
    ///     If not provided, the model property name is used.
    /// </summary>
    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    /// <summary>
    ///     The list of options to populate the dropdown with.
    ///     Each option includes a Value and a Label.
    /// </summary>
    [HtmlAttributeName("options")]
    public IEnumerable<GdsOption> Options { get; set; }

    /// <summary>
    ///     Whether to include a default disabled option prompting user selection.
    ///     Defaults to true.
    /// </summary>
    [HtmlAttributeName("include-default-option")]
    public bool IncludeDefaultOption { get; set; } = true;

    /// <summary>
    ///     The text to display for the default option.
    ///     Only used if IncludeDefaultOption is true.
    ///     Defaults to "Please select...".
    /// </summary>
    [HtmlAttributeName("default-option-text")]
    public string DefaultOptionText { get; set; } = "Please select...";

    /// <summary>
    ///     Indicates whether this select field is conditionally shown.
    ///     Adds a <c>conditional-field</c> CSS class to the form group container.
    /// </summary>
    [HtmlAttributeName("conditional-field")]
    public bool ConditionalField { get; set; } = false;

    /// <summary>
    ///     Custom validation message. Defaults to the first model error.
    /// </summary>
    [HtmlAttributeName("validation-message")]
    public string ValidationMessage { get; set; }

    /// <summary>
    ///     Provides access to the current view context, including model state for validation.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        // Get the selected value from the model (used to mark <option> as selected)
        var selectedValue = For.Model?.ToString();

        // Try to get the model state for validation (using custom error key if provided)
        ViewContext.ViewData.ModelState.TryGetValue( propertyName, out var entry);
        var hasError = entry != null && entry.Errors.Count > 0;

        // Build the CSS class string for the outer form group container
        var formGroupClass = "govuk-form-group"
                             + (ConditionalField ? " conditional-field" : "")
                             + (hasError ? " govuk-form-group--error" : "");

        // Set up the outer <div> that wraps the label, error, and select
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", formGroupClass); // Apply conditional and error classes

        // Render the label HTML, falling back to property name if label text is not provided
        var labelHtml = $@"
            <label class='govuk-label govuk-label--s' for='{propertyName}'>
                {LabelText ?? propertyName}
            </label>";

        // Render a validation error message span if applicable
        string errorMessage = null;

        if (!string.IsNullOrWhiteSpace(ValidationMessage))
        {
            errorMessage = HtmlEncoder.Default.Encode(ValidationMessage);
        }
        else if (entry is { Errors.Count: > 0 })
        {
            var allErrors = entry.Errors
                .Select(e => HtmlEncoder.Default.Encode(e.ErrorMessage))
                .Where(e => !string.IsNullOrWhiteSpace(e));
            errorMessage = string.Join("<br/>", allErrors);
        }

        var errorHtml = hasError && !string.IsNullOrWhiteSpace(errorMessage)
            ? $"<span class='govuk-error-message'>{errorMessage}</span>"
            : "";

        // Optionally add a default placeholder option (e.g., "Please select...")
        var optionsHtml = IncludeDefaultOption
            ? $"<option value='' disabled {(string.IsNullOrEmpty(selectedValue) ? "selected" : "")}>{DefaultOptionText}</option>"
            : "";

        // Build <option> elements from the supplied list
        optionsHtml += string.Join("\n", Options.Select(option =>
        {
            // Determine if this option is the selected one
            var selectedAttr = string.Equals(
                selectedValue?.Trim(),
                option.Value?.Trim(),
                StringComparison.OrdinalIgnoreCase
            )
                ? "selected"
                : "";

            // Create a unique ID for the option (not required for HTML but helpful for accessibility/testing)
            var optionId = $"{propertyName}_{option.Value.Replace(" ", "_")}";

            return $@"<option id='{optionId}' value='{option.Value}' {selectedAttr}>{option.Label}</option>";
        }));

        // Compose the <select> element with the generated <option>s
        var selectHtml = $@"
            <select id='{propertyName}' name='{propertyName}' class='govuk-select'>
                {optionsHtml}
            </select>";

        // Inject the combined HTML into the output
        output.Content.SetHtmlContent(labelHtml + errorHtml + selectHtml);
    }
}