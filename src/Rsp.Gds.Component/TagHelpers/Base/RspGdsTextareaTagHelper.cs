namespace Rsp.Gds.Component.TagHelpers.Base
{
    [HtmlTargetElement("rsp-gds-textarea", Attributes = ForAttributeName)]
    public class RspGdsTextareaTagHelper : TagHelper
    {
        protected const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("label-text")]
        public string LabelText { get; set; }

        [HtmlAttributeName("width-class")]
        public string WidthClass { get; set; } = "govuk-!-width-full";

        [HtmlAttributeName("rows")]
        public int Rows { get; set; } = 5;

        [HtmlAttributeName("placeholder")]
        public string Placeholder { get; set; }

        [HtmlAttributeName("readonly")]
        public bool Readonly { get; set; } = false;

        [HtmlAttributeName("disabled")]
        public bool Disabled { get; set; } = false;

        [HtmlAttributeName("additional-attributes")]
        public IDictionary<string, string> AdditionalAttributes { get; set; } = new Dictionary<string, string>();

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        protected string GetTextareaHtml(bool hasFieldError)
        {
            var propertyName = For.Name;
            var value = For.Model?.ToString() ?? "";

            var extraAttributes = new Dictionary<string, string>(AdditionalAttributes);

            if (Readonly) extraAttributes["readonly"] = "readonly";
            if (Disabled) extraAttributes["disabled"] = "disabled";
            if (!string.IsNullOrEmpty(Placeholder)) extraAttributes["placeholder"] = Placeholder;

            extraAttributes["rows"] = Rows.ToString();

            var attrHtml = string.Join(" ", extraAttributes.Select(kvp => $"{kvp.Key}='{kvp.Value}'"));

            return $@"
                <textarea class='govuk-textarea {WidthClass} {(hasFieldError ? "govuk-textarea--error" : "")}'
                          id='{propertyName}'
                          name='{propertyName}'
                          {attrHtml}>{value}</textarea>";
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var propertyName = For.Name;

            ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var entry);
            var hasFieldError = entry != null && entry.Errors.Count > 0;

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"govuk-form-group {(hasFieldError ? "govuk-form-group--error" : "")}");

            var labelHtml = $@"
                <label class='govuk-label govuk-label--s' for='{propertyName}'>
                    {LabelText ?? propertyName}
                </label>";

            var fieldErrorsHtml = "";
            if (hasFieldError)
            {
                foreach (var error in entry.Errors)
                {
                    fieldErrorsHtml += $"<span class='govuk-error-message'>{error.ErrorMessage}</span>";
                }
            }

            var textareaHtml = GetTextareaHtml(hasFieldError);

            output.Content.SetHtmlContent(labelHtml + fieldErrorsHtml + textareaHtml);
        }
    }
}
