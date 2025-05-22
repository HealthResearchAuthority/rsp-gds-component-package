using System.Text;

namespace Rsp.Gds.Component.TagHelpers.Base;

[HtmlTargetElement("rsp-gds-radio-group", Attributes = ForAttributeName)]
public class RspGdsRadioGroupTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    [HtmlAttributeName("options")]
    public IEnumerable<GdsOption> Options { get; set; }

    [HtmlAttributeName("conditional-field")]
    public bool ConditionalField { get; set; } = false;

    [HtmlAttributeName("hint-html")]
    public string HintHtml { get; set; }

    [HtmlAttributeName("validation-message")]
    public string ValidationMessage { get; set; }

    [HtmlAttributeName("legend-class")]
    public string LegendClass { get; set; } = "govuk-fieldset__legend--l";

    [HtmlAttributeName("label-css-class")]
    public string LabelCssClass { get; set; }

    [HtmlAttributeName("conditional-class")]
    public string ConditionalClass { get; set; }

    [HtmlAttributeName("dataparents-attr")]
    public string DataParentsAttr { get; set; }

    [HtmlAttributeName("dataquestionid-attr")]
    public string DataQuestionIdAttr { get; set; }

    [HtmlAttributeName("question-id")]
    public string QuestionId { get; set; }

    [HtmlAttributeName("id")]
    public string HtmlId { get; set; }

    [HtmlAttributeName("div-inline-class")]
    public string DivInlineClass { get; set; }

    [HtmlAttributeName("datamodule")]
    public string DataModule { get; set; } = "govuk-radios";

    [HtmlAttributeName("item-hidden-properties")]
    public string ItemHiddenProperties { get; set; }

    [HtmlAttributeName("hidden-model")]
    public IEnumerable<object> HiddenModel { get; set; }

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var selectedValue = For.Model switch
        {
            string str => str,
            IEnumerable<string> list => list.FirstOrDefault(),
            _ => For.Model?.ToString()
        };

        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var modelStateEntry);
        var hasError = modelStateEntry != null && modelStateEntry.Errors.Count > 0;
        var errorText = !string.IsNullOrEmpty(ValidationMessage)
            ? ValidationMessage
            : modelStateEntry?.Errors.FirstOrDefault()?.ErrorMessage;

        var errorHtml = hasError && !string.IsNullOrWhiteSpace(errorText)
            ? $"<span class='govuk-error-message'>{HtmlEncoder.Default.Encode(errorText)}</span>"
            : "";

        var hintHtml = !string.IsNullOrWhiteSpace(HintHtml)
            ? $"<div class='govuk-hint'>{HintHtml}</div>"
            : "";

        var formGroupClass = "govuk-form-group"
                             + (ConditionalField ? " conditional govuk-radios__conditional" : "")
                             + (hasError ? " govuk-form-group--error" : "");

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", formGroupClass);
        output.Attributes.SetAttribute("id", !string.IsNullOrWhiteSpace(HtmlId) ? HtmlId : propertyName);

        if (!string.IsNullOrWhiteSpace(DataParentsAttr))
            output.Attributes.SetAttribute("data-parents", DataParentsAttr);

        if (!string.IsNullOrWhiteSpace(DataQuestionIdAttr))
            output.Attributes.SetAttribute("data-questionId", DataQuestionIdAttr);

        var hiddenProps = (ItemHiddenProperties ?? "")
            .Split(',')
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();

        var radiosHtmlBuilder = new StringBuilder();
        var index = 0;

        foreach (var option in Options)
        {
            var inputId = $"{QuestionId}_{option.Value.Replace(" ", "_")}";
            var isChecked = string.Equals(
                selectedValue?.Trim(),
                option.Value?.Trim(),
                StringComparison.OrdinalIgnoreCase
            ) ? "checked=\"checked\"" : "";

            var radioHtml = $@"
        <div class='govuk-radios__item'>
            <input class='govuk-radios__input'
                   id='{inputId}'
                   name='{propertyName}'
                   type='radio'
                   value='{option.Value}'
                   {isChecked} />
            <label class='govuk-label govuk-radios__label {LabelCssClass}' for='{inputId}'>
                {option.Label}
            </label>";

            if (HiddenModel != null && HiddenModel.Count() > index && hiddenProps.Any())
            {
                var modelItem = HiddenModel.ElementAt(index);
                var modelType = modelItem.GetType();

                foreach (var hiddenProp in hiddenProps)
                {
                    var prop = modelType.GetProperty(hiddenProp);
                    if (prop != null)
                    {
                        var value = prop.GetValue(modelItem)?.ToString() ?? "";
                        var name = $"{propertyName.Replace("SelectedOption", "Answers")}[{index}].{hiddenProp}";
                        radioHtml += $"\n<input type='hidden' name='{name}' value='{HtmlEncoder.Default.Encode(value)}' />";
                    }
                }
            }

            radioHtml += "\n</div>";
            radiosHtmlBuilder.AppendLine(radioHtml);
            index++;
        }

        var radiosHtml = radiosHtmlBuilder.ToString();

        var radioWrapperClass = !string.IsNullOrWhiteSpace(DivInlineClass)
            ? DivInlineClass
            : "govuk-radios";

        var fieldsetHtml = $@"
            <govuk-fieldset>
                <govuk-fieldset-legend class='{LegendClass}'>
                    {LabelText ?? propertyName}
                </govuk-fieldset-legend>
                {hintHtml}
                {errorHtml}
                <div class='{radioWrapperClass}' data-module='{DataModule}'>
                    {radiosHtml}
                </div>
            </govuk-fieldset>";

        output.Content.SetHtmlContent(fieldsetHtml);
    }
}
