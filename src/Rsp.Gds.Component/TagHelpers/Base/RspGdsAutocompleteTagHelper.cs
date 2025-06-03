namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled autocomplete input field with accessible autocomplete behavior.
///     Supports conditional display, dynamic API endpoints, validation messages, and hint text.
/// </summary>
[HtmlTargetElement("rsp-gds-autocomplete", Attributes = ForAttributeName)]
public class RspGdsAutocompleteTagHelper : RspGdsTagHelperBase
{
    /// <summary>
    ///     The API endpoint URL used to fetch autocomplete suggestions.
    /// </summary>
    [HtmlAttributeName("api-url")]
    public string ApiUrl { get; set; }

    /// <summary>
    ///     GOV.UK input width class. Defaults to 'govuk-!-width-three-quarters'.
    /// </summary>
    [HtmlAttributeName("width-class")]
    public string WidthClass { get; set; } = "govuk-!-width-three-quarters";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var fieldId = !string.IsNullOrEmpty(FieldId) ? FieldId : propertyName.Replace(".", "_");
        var hiddenInputId = fieldId;
        var autoInputId = fieldId + "_autocomplete";
        var containerId = fieldId + "_autocomplete_container";
        var value = For.Model?.ToString() ?? string.Empty;

        SetContainerAttributes(output, propertyName);

        var labelHtml = BuildLabelHtml(propertyName, autoInputId, hiddenInputId, fieldId);
        var hintHtml = BuildHintHtml(fieldId);
        var errorHtml = BuildErrorHtml(propertyName);

        // This input will be shown if JavaScript is disabled (by default) and hidden when JS runs
        var hiddenInputHtml = $@"
<input id='{hiddenInputId}'
       name='{propertyName}'
       type='text'
       class='govuk-input {WidthClass}'
       value='{HtmlEncoder.Default.Encode(value)}' />";

        var containerHtml = $"<div id='{containerId}'></div>";

        var initScript = $@"<script>
document.addEventListener('DOMContentLoaded', function () {{
    initAutocomplete('{autoInputId}', '{hiddenInputId}', '{HtmlEncoder.Default.Encode(value)}', '{ApiUrl}', '{containerId}');
}});
</script>";

        output.Content.SetHtmlContent(labelHtml + hintHtml + errorHtml + hiddenInputHtml + containerHtml + initScript);
    }
}