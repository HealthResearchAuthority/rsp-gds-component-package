namespace Rsp.Gds.Component.TagHelpers
{
    [HtmlTargetElement("rsp-gds-input", Attributes = ForAttributeName)]
    public class RspGdsInputTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)] public ModelExpression For { get; set; }

        [HtmlAttributeName("label-text")] public string LabelText { get; set; }

        [HtmlAttributeName("width-class")] public string WidthClass { get; set; } = "govuk-!-width-one-half";

        [HtmlAttributeName("input-type")] public string InputType { get; set; } = "text";

        [HtmlAttributeName("autocomplete")] public string Autocomplete { get; set; }

        [HtmlAttributeName("placeholder")] public string Placeholder { get; set; }

        [HtmlAttributeName("readonly")] public bool Readonly { get; set; } = false;

        [HtmlAttributeName("disabled")] public bool Disabled { get; set; } = false;

        [HtmlAttributeName("additional-attributes")]
        public IDictionary<string, string> AdditionalAttributes { get; set; } = new Dictionary<string, string>();

        [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var propertyName = For.Name;
            var value = For.Model?.ToString() ?? "";

            ViewContext.ViewData.ModelState.TryGetValue(propertyName, out ModelStateEntry entry);
            var hasError = entry != null && entry.Errors.Count > 0;

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"govuk-form-group {(hasError ? "govuk-form-group--error" : "")}");

            // Build label
            var labelHtml = $@"
                <label class='govuk-label govuk-label--s' for='{propertyName}'>
                    {LabelText ?? propertyName}
                </label>";

            // Build errors
            var errorsHtml = "";
            if (hasError)
            {
                foreach (var error in entry.Errors)
                {
                    errorsHtml += $"<span class='govuk-error-message'>{error.ErrorMessage}</span>";
                }
            }

            // Build attributes
            var extraAttributes = new Dictionary<string, string>(AdditionalAttributes);

            if (Readonly) extraAttributes["readonly"] = "readonly";
            if (Disabled) extraAttributes["disabled"] = "disabled";
            if (!string.IsNullOrEmpty(Autocomplete)) extraAttributes["autocomplete"] = Autocomplete;
            if (!string.IsNullOrEmpty(Placeholder)) extraAttributes["placeholder"] = Placeholder;
            if (!extraAttributes.ContainsKey("aria-invalid") && hasError)
                extraAttributes["aria-invalid"] = "true";

            var attrHtml = string.Join(" ", extraAttributes.Select(kvp => $"{kvp.Key}='{kvp.Value}'"));

            // Build input manually without asp-for
            var inputHtml = $@"
                <input class='govuk-input {WidthClass} {(hasError ? "govuk-input--error" : "")}'
                       id='{propertyName}'
                       name='{propertyName}'
                       type='{InputType}'
                       value='{value}'
                       {attrHtml} />";

            // Set final content
            output.Content.SetHtmlContent(labelHtml + errorsHtml + inputHtml);
        }
    }
}