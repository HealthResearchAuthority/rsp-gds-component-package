namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled date input field with separate Day, Month, and Year inputs.
///     Supports hint text, label customization, and validation error display.
/// </summary>
[HtmlTargetElement("rsp-gds-date-input", Attributes = ForAttributeName)]
public class RspGdsDateInputTagHelper : RspGdsTagHelperBase
{
    /// <summary>
    ///     The name attribute for the "Day" input field.
    ///     Also used as the input's HTML id.
    /// </summary>
    [HtmlAttributeName("day-name")]
    public string DayName { get; set; }

    /// <summary>
    ///     Gets or sets the pre-filled value for the "Day" input field.
    /// </summary>
    [HtmlAttributeName("day-value")]
    public string? DayValue { get; set; }

    /// <summary>
    ///     The name attribute for the "Month" input field.
    ///     Also used as the input's HTML id.
    /// </summary>
    [HtmlAttributeName("month-name")]
    public string MonthName { get; set; }

    /// <summary>
    ///     Gets or sets the pre-filled value for the "Month" input field.
    /// </summary>
    [HtmlAttributeName("month-value")]
    public string? MonthValue { get; set; }

    /// <summary>
    ///     The name attribute for the "Year" input field.
    ///     Also used as the input's HTML id.
    /// </summary>
    [HtmlAttributeName("year-name")]
    public string YearName { get; set; }

    /// <summary>
    ///     Gets or sets the pre-filled value for the "Year" input field.
    /// </summary>
    [HtmlAttributeName("year-value")]
    public string? YearValue { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;
        var fieldId = !string.IsNullOrEmpty(FieldId) ? FieldId : propertyName.Replace(".", "_");
        var containerId = !string.IsNullOrWhiteSpace(HtmlId) ? HtmlId : propertyName;

        SetContainerAttributes(output, propertyName);

        var encodedLabel = HtmlEncoder.Default.Encode(LabelText ?? propertyName);
        var labelHtml = $@"<label class='govuk-label' for='{propertyName}'>{encodedLabel}</label>";

        var hintHtml = BuildHintHtml(fieldId);
        var errorHtml = BuildErrorHtml(propertyName);
        var hasError = HasError(propertyName);

        var dayInput = $@"
        <div class='govuk-date-input__item'>
            <div class='govuk-form-group'>
                <label class='govuk-label govuk-date-input__label' for='{DayName}'>Day</label>
                <input class='govuk-input govuk-date-input__input govuk-input--width-2{(hasError ? " govuk-input--error" : "")}'
                       id='{DayName}'
                       name='{DayName}'
                       type='text'
                       inputmode='numeric'
                       value='{HtmlEncoder.Default.Encode(DayValue ?? "")}'>
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
                       inputmode='numeric'
                       value='{HtmlEncoder.Default.Encode(MonthValue ?? "")}'>
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
                       inputmode='numeric'
                       value='{HtmlEncoder.Default.Encode(YearValue ?? "")}'>
            </div>
        </div>";

        var dateGroupHtml = $@"
        <div class='govuk-date-input' id='{fieldId}_date'>
            {dayInput}
            {monthInput}
            {yearInput}
        </div>";

        output.Content.SetHtmlContent(labelHtml + hintHtml + errorHtml + dateGroupHtml);
    }
}
