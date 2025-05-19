namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled radio button group for a model-bound property.
///     Supports label text, dynamic options, conditional visibility, and validation errors.
/// </summary>
[HtmlTargetElement("rsp-gds-radio-group", Attributes = ForAttributeName)]
public class RspGdsRadioGroupTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     The model expression this radio group is bound to.
    ///     Used for setting the field name, selected value, and validation.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    ///     The label text displayed above the radio button group.
    /// </summary>
    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    /// <summary>
    ///     A list of options to be rendered as individual radio buttons.
    ///     Each option contains a Value and a Label.
    /// </summary>
    [HtmlAttributeName("options")]
    public IEnumerable<GdsOption> Options { get; set; }

    /// <summary>
    ///     Indicates whether this radio group is conditionally shown.
    ///     Adds a <c>conditional-field</c> CSS class to the form group container.
    /// </summary>
    [HtmlAttributeName("conditional-field")]
    public bool ConditionalField { get; set; } = false;

    /// <summary>
    ///     Provides access to the current view context, including ModelState for validation.
    /// </summary>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        // Attempt to extract the selected value from the bound model.
        // Supports string, list of strings, or generic object.
        var selectedValue = For.Model switch
        {
            string str => str,
            IEnumerable<string> list => list.FirstOrDefault(),
            _ => For.Model?.ToString()
        };

        // Get validation errors for the field, if present.
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var modelStateEntry);
        var hasError = modelStateEntry != null && modelStateEntry.Errors.Count > 0;

        // If validation errors exist, render the first one in a GOV.UK error span.
        var errorHtml = hasError
            ? $"<span class='govuk-error-message'>{modelStateEntry.Errors[0].ErrorMessage}</span>"
            : "";

        // Construct the form group class string, appending conditional and error modifiers if needed.
        var formGroupClass = "govuk-form-group"
                             + (ConditionalField ? " conditional-field" : "")
                             + (hasError ? " govuk-form-group--error" : "");

        // Define the outer <div> that wraps the radio group.
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", formGroupClass); // Apply styling classes

        // Build the HTML for each individual radio item.
        var radiosHtml = string.Join("\n", Options.Select(option =>
        {
            // Generate a unique input ID for each radio option
            var inputId = $"{propertyName}_{option.Value.Replace(" ", "_")}";

            // Mark the radio as checked if the current value matches the model value
            var isChecked = string.Equals(
                selectedValue?.Trim(),
                option.Value?.Trim(),
                StringComparison.OrdinalIgnoreCase
            )
                ? "checked=\"checked\""
                : "";

            return $@"
                <div class='govuk-radios__item'>
                    <input class='govuk-radios__input'
                           id='{inputId}'
                           name='{propertyName}'
                           type='radio'
                           value='{option.Value}'
                           {isChecked} />
                    <label class='govuk-label govuk-radios__label' for='{inputId}'>
                        {option.Label}
                    </label>
                </div>";
        }));

        // Combine the label, error message, and radio items inside a GOV.UK fieldset
        var fieldsetHtml = $@"
            <fieldset class='govuk-fieldset'>
                <legend class='govuk-fieldset__legend govuk-fieldset__legend--m'>
                    <label class='govuk-label govuk-label--s' for='{propertyName}'>{LabelText}</label>
                </legend>
                {errorHtml}
                <div class='govuk-radios' data-module='govuk-radios'>
                    {radiosHtml}
                </div>
            </fieldset>";

        // Set the final HTML content into the output
        output.Content.SetHtmlContent(fieldsetHtml);
    }
}