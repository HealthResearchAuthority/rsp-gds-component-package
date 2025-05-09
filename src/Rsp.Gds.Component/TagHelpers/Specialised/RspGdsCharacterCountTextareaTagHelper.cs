namespace Rsp.Gds.Component.TagHelpers.Specialised;

[HtmlTargetElement("rsp-gds-character-count-textarea", Attributes = ForAttributeName)]
public class RspGdsCharacterCountTextareaTagHelper : RspGdsTextareaTagHelper
{
    [HtmlAttributeName("word-count-error-for")]
    public string WordCountErrorProperty { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var fieldEntry);
        var hasFieldError = fieldEntry != null && fieldEntry.Errors.Count > 0;

        ModelStateEntry wordCountEntry = null;
        if (!string.IsNullOrEmpty(WordCountErrorProperty))
        {
            ViewContext.ViewData.ModelState.TryGetValue(WordCountErrorProperty, out wordCountEntry);
        }

        var hasWordCountError = wordCountEntry != null && wordCountEntry.Errors.Count > 0;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class",
            $"govuk-form-group govuk-character-count {(hasFieldError ? "govuk-form-group--error" : "")}");
        output.Attributes.SetAttribute("data-module", "govuk-character-count");

        var labelHtml = $@"
                <div class='govuk-label-wrapper'>
                    <label class='govuk-label govuk-label--s' for='{propertyName}'>
                        {LabelText ?? propertyName}
                    </label>
                </div>";

        var fieldErrorsHtml = "";
        if (hasFieldError)
        {
            foreach (var error in fieldEntry.Errors)
            {
                fieldErrorsHtml += $"<span class='govuk-error-message'>{error.ErrorMessage}</span>";
            }
        }

        var textareaHtml = GetTextareaHtml(hasFieldError);

        var wordCountErrorHtml = "";
        if (hasWordCountError)
        {
            wordCountErrorHtml = $@"
                    <div class='govuk-character-count__message govuk-error-message'>
                        {wordCountEntry.Errors[0].ErrorMessage}
                    </div>";
        }

        output.Content.SetHtmlContent(labelHtml + fieldErrorsHtml + textareaHtml + wordCountErrorHtml);
    }
}