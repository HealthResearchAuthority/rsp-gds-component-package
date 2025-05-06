using System.Text.Encodings.Web;

namespace Rsp.Gds.Component.TagHelpers.Base;

[HtmlTargetElement("rsp-gds-date-input", Attributes = ForAttributeName)]
public class RspGdsDateInputTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)] public ModelExpression For { get; set; }

    [HtmlAttributeName("day-name")] public string DayName { get; set; }

    [HtmlAttributeName("month-name")] public string MonthName { get; set; }

    [HtmlAttributeName("year-name")] public string YearName { get; set; }

    [HtmlAttributeName("label-text")] public string LabelText { get; set; }

    [HtmlAttributeName("hint-html")] public string HintHtml { get; set; }

    [HtmlAttributeName("error-key")] public string ErrorKey { get; set; }

    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ViewContext.ViewData.ModelState.TryGetValue(ErrorKey ?? For.Name, out var entry);
        var hasError = entry?.ValidationState == ModelValidationState.Invalid;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"govuk-form-group{(hasError ? " govuk-form-group--error" : "")}");

        var encodedLabel = HtmlEncoder.Default.Encode(LabelText ?? For.Name);

        var labelHtml = $@"
                <label class='govuk-label' for='{For.Name}'>{encodedLabel}</label>";

        var hintHtml = !string.IsNullOrEmpty(HintHtml)
            ? $"<div class='govuk-hint'>{HintHtml}</div>"
            : "";

        var errorHtml = hasError
            ? $"<span class='govuk-error-message'>{entry.Errors[0].ErrorMessage}</span>"
            : "";

        var dayInput = $@"
                <div class='govuk-date-input__item'>
                    <div class='govuk-form-group'>
                        <label class='govuk-label govuk-date-input__label' for='{DayName}'>Day</label>
                        <input class='govuk-input govuk-date-input__input govuk-input--width-2{(hasError ? " govuk-input--error" : "")}'
                               id='{DayName}'
                               name='{DayName}'
                               type='text'
                               inputmode='numeric' />
                    </div>
                </div>";

        var monthInput = $@"
                <div class='govuk-date-input__item'>
                    <div class='govuk-form-group'>
                        <label class='govuk-label govuk-date-input__label' for='{MonthName}'>Month</label>
                        <input class='govuk-input govuk-date-input__input govuk-input--width-2{(hasError ? " govuk-input--error" : "")}'
                               id='{MonthName}'
                               name='{MonthName}'
                               type='text'
                               inputmode='numeric' />
                    </div>
                </div>";

        var yearInput = $@"
                <div class='govuk-date-input__item'>
                    <div class='govuk-form-group'>
                        <label class='govuk-label govuk-date-input__label' for='{YearName}'>Year</label>
                        <input class='govuk-input govuk-date-input__input govuk-input--width-4{(hasError ? " govuk-input--error" : "")}'
                               id='{YearName}'
                               name='{YearName}'
                               type='text'
                               inputmode='numeric' />
                    </div>
                </div>";

        var dateGroupHtml = $@"
                <div class='govuk-date-input' id='{For.Name}'>
                    {dayInput}
                    {monthInput}
                    {yearInput}
                </div>";

        output.Content.SetHtmlContent(labelHtml + hintHtml + errorHtml + dateGroupHtml);
    }
}