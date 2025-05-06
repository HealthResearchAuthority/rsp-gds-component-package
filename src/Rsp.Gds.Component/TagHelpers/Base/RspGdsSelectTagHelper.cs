using System;

namespace Rsp.Gds.Component.TagHelpers.Base;

[HtmlTargetElement("rsp-gds-select", Attributes = ForAttributeName)]
public class RspGdsSelectTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)] public ModelExpression For { get; set; }

    [HtmlAttributeName("label-text")] public string LabelText { get; set; }

    [HtmlAttributeName("options")] public IEnumerable<GdsOption> Options { get; set; }

    [HtmlAttributeName("include-default-option")]
    public bool IncludeDefaultOption { get; set; } = true;

    [HtmlAttributeName("default-option-text")]
    public string DefaultOptionText { get; set; } = "Please select...";

    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var selectedValue = For.Model?.ToString();

        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var modelStateEntry);
        var hasError = modelStateEntry?.Errors?.Count > 0;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"govuk-form-group {(hasError ? "govuk-form-group--error" : "")}");

        var labelHtml = $@"
                <label class='govuk-label govuk-label--s' for='{propertyName}'>
                    {LabelText ?? propertyName}
                </label>";

        var errorHtml = hasError
            ? $"<span class='govuk-error-message'>{modelStateEntry.Errors[0].ErrorMessage}</span>"
            : "";

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

        var selectHtml = $@"
                <select id='{propertyName}' name='{propertyName}' class='govuk-select'>
                    {optionsHtml}
                </select>";

        output.Content.SetHtmlContent(labelHtml + errorHtml + selectHtml);
    }
}