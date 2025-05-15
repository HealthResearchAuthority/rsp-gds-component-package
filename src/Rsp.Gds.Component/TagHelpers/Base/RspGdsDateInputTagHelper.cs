namespace Rsp.Gds.Component.TagHelpers.Base;

/// <summary>
///     Renders a GOV.UK-styled date input field with separate Day, Month, and Year inputs.
///     Supports hint text, label customization, and validation error display.
/// </summary>
[HtmlTargetElement("rsp-gds-date-input", Attributes = ForAttributeName)]
public class RspGdsDateInputTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    /// <summary>
    ///     The model expression this date input group is bound to.
    ///     Used for binding the field and referencing validation state.
    /// </summary>
    [HtmlAttributeName(ForAttributeName)]
    public ModelExpression For { get; set; }

    /// <summary>
    ///     The name attribute for the "Day" input field.
    ///     Also used as the input's HTML id.
    /// </summary>
    [HtmlAttributeName("day-name")]
    public string DayName { get; set; }

    /// <summary>
    ///     The name attribute for the "Month" input field.
    ///     Also used as the input's HTML id.
    /// </summary>
    [HtmlAttributeName("month-name")]
    public string MonthName { get; set; }

    /// <summary>
    ///     The name attribute for the "Year" input field.
    ///     Also used as the input's HTML id.
    /// </summary>
    [HtmlAttributeName("year-name")]
    public string YearName { get; set; }

    /// <summary>
    ///     The label text to be displayed above the date input.
    ///     If not specified, the bound property name is used.
    /// </summary>
    [HtmlAttributeName("label-text")]
    public string LabelText { get; set; }

    /// <summary>
    ///     Optional HTML content rendered as hint text below the label.
    /// </summary>
    [HtmlAttributeName("hint-html")]
    public string HintHtml { get; set; }

    /// <summary>
    ///     Optional override key to lookup validation errors.
    ///     Defaults to the model property name if not provided.
    /// </summary>
    [HtmlAttributeName("error-key")]
    public string ErrorKey { get; set; }

    /// <summary>
    ///     Indicates whether the checkbox group is conditionally shown or toggled based on another form input.
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
        ViewContext.ViewData.ModelState.TryGetValue(ErrorKey ?? propertyName, out var modelStateEntry);
        var hasError = modelStateEntry != null && modelStateEntry.Errors.Count > 0;

        var formGroupClass = "govuk-form-group"
                             + (ConditionalField ? " conditional-field" : "")
                             + (hasError ? " govuk-form-group--error" : "");

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", propertyName); // Added ID here
        output.Attributes.SetAttribute("class", formGroupClass);

        var encodedLabel = HtmlEncoder.Default.Encode(LabelText ?? propertyName);

        var labelHtml = $@"
            <label class='govuk-label' for='{propertyName}'>{encodedLabel}</label>";

        var hintHtml = !string.IsNullOrEmpty(HintHtml)
            ? $"<div class='govuk-hint'>{HintHtml}</div>"
            : "";

        var errorHtml = hasError
            ? $"<span class='govuk-error-message'>{modelStateEntry.Errors[0].ErrorMessage}</span>"
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
            <div class='govuk-date-input' id='{propertyName}_date'>
                {dayInput}
                {monthInput}
                {yearInput}
            </div>";

        output.Content.SetHtmlContent(labelHtml + hintHtml + errorHtml + dateGroupHtml);
    }
}