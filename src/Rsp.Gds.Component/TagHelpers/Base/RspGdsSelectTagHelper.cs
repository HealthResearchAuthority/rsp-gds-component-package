using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.Encodings.Web;

namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled
///     <select>
///         dropdown for a model-bound property.
///         Supports optional default option, validation error display, custom label text, and conditional styling.
/// </summary>
[HtmlTargetElement("rsp-gds-select", Attributes = ForAttributeName)]
public class RspGdsSelectTagHelper : RspGdsTagHelperBase
{
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

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var fieldId = !string.IsNullOrEmpty(FieldId) ? FieldId : propertyName;

        SetContainerAttributes(output, propertyName);

        var selectedValue = For.Model?.ToString();
        var errorHtml = BuildErrorHtml(propertyName);
        var hintHtml = BuildHintHtml(fieldId);

        var optionsHtml = IncludeDefaultOption
            ? $"<option value='' disabled {(string.IsNullOrEmpty(selectedValue) ? "selected" : "")}>{DefaultOptionText}</option>"
            : "";

        optionsHtml += string.Join("\n", Options.Select(option =>
        {
            var selectedAttr = string.Equals(
                selectedValue?.Trim(),
                option.Value?.Trim(),
                StringComparison.OrdinalIgnoreCase
            ) ? "selected" : "";

            var optionId = $"{propertyName}_{option.Value.Replace(" ", "_")}";

            return $"<option id='{optionId}' value='{option.Value}' {selectedAttr}>{option.Label}</option>";
        }));

        var labelHtml = $@"
            <label class='govuk-label govuk-label--s' for='{propertyName}'>
                {LabelText ?? propertyName}
            </label>";

        var selectHtml = $@"
            <select id='{propertyName}' name='{propertyName}' class='govuk-select'>
                {optionsHtml}
            </select>";

        output.Content.SetHtmlContent(labelHtml + hintHtml + errorHtml + selectHtml);
    }
}
