namespace Rsp.Gds.Component.TagHelpers.Specialised;

/// <summary>
///     Renders a GOV.UK-styled character count textarea with a character or word limit indicator.
///     Extends <c>RspGdsTextareaTagHelper</c> with support for displaying separate word count validation errors.
/// </summary>
[HtmlTargetElement("rsp-gds-character-count-textarea", Attributes = ForAttributeName)]
public class RspGdsCharacterCountTextareaTagHelper : RspGdsTextareaTagHelper
{
    /// <summary>
    ///     The name of the model property to check for word/character count validation errors.
    ///     If set, a separate validation message will appear under the textarea.
    /// </summary>
    [HtmlAttributeName("word-count-error-for")]
    public string WordCountErrorProperty { get; set; }

    /// <summary>
    ///     Generates the final GOV.UK form group markup including character count module,
    ///     label, validation messages, textarea, and optional word count error message.
    /// </summary>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        // Get standard validation errors for the field (e.g. required, max length, etc.)
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var fieldEntry);
        var hasFieldError = fieldEntry != null && fieldEntry.Errors.Count > 0;

        // Optionally check a separate property for character/word count errors
        ModelStateEntry wordCountEntry = null;
        if (!string.IsNullOrEmpty(WordCountErrorProperty))
        {
            ViewContext.ViewData.ModelState.TryGetValue(WordCountErrorProperty, out wordCountEntry);
        }

        var hasWordCountError = wordCountEntry != null && wordCountEntry.Errors.Count > 0;

        // Construct the form group class string, including GOV.UK character count module,
        // conditional field class, and error styling if applicable
        var formGroupClass = "govuk-form-group govuk-character-count"
                             + (ConditionalField ? " conditional-field" : "")
                             + (hasFieldError ? " govuk-form-group--error" : "");

        // Configure outer <div> wrapper
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", formGroupClass); // Apply conditional/error styling
        output.Attributes.SetAttribute("data-module", "govuk-character-count"); // Enables JS character count behavior

        // Build the GOV.UK label
        var labelHtml = $@"
            <div class='govuk-label-wrapper'>
                <label class='govuk-label govuk-label--s' for='{propertyName}'>
                    {LabelText ?? propertyName}
                </label>
            </div>";

        // Render all field-level errors (e.g. required, too long)
        var fieldErrorsHtml = "";
        if (hasFieldError)
        {
            foreach (var error in fieldEntry.Errors)
            {
                fieldErrorsHtml += $"<span class='govuk-error-message'>{error.ErrorMessage}</span>";
            }
        }

        // Render the textarea input, applying error classes if necessary
        var textareaHtml = GetTextareaHtml(hasFieldError);

        // Render additional word/character count validation error if present
        var wordCountErrorHtml = "";
        if (hasWordCountError)
        {
            wordCountErrorHtml = $@"
                <div class='govuk-character-count__message govuk-error-message'>
                    {wordCountEntry.Errors[0].ErrorMessage}
                </div>";
        }

        // Output the final HTML: label, standard errors, textarea, and word count error
        output.Content.SetHtmlContent(labelHtml + fieldErrorsHtml + textareaHtml + wordCountErrorHtml);
    }
}