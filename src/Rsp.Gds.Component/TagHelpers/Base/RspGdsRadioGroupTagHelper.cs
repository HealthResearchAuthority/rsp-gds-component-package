using System;

namespace Rsp.Gds.Component.TagHelpers.Base;

[HtmlTargetElement("rsp-gds-radio-group", Attributes = ForAttributeName)]
public class RspGdsRadioGroupTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)] public ModelExpression For { get; set; }

    [HtmlAttributeName("label-text")] public string LabelText { get; set; }

    [HtmlAttributeName("options")] public IEnumerable<GdsOption> Options { get; set; }

    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        // ✅ Safely extract selected value even if model is a List<string>
        var selectedValue = For.Model switch
        {
            string str => str,
            IEnumerable<string> list => list.FirstOrDefault(),
            _ => For.Model?.ToString()
        };

        // ✅ Error handling
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var modelStateEntry);
        var hasError = modelStateEntry != null && modelStateEntry.Errors.Count > 0;

        var errorHtml = hasError
            ? $"<span class='govuk-error-message'>{modelStateEntry.Errors[0].ErrorMessage}</span>"
            : "";

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"govuk-form-group {(hasError ? "govuk-form-group--error" : "")}");

        // ✅ Render radio options
        var radiosHtml = string.Join("\n", Options.Select(option =>
        {
            var inputId = $"{propertyName}_{option.Value.Replace(" ", "_")}";
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

        output.Content.SetHtmlContent(fieldsetHtml);
    }
}