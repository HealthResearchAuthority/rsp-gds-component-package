namespace Rsp.Gds.Component.TagHelpers.Base;

[HtmlTargetElement("rsp-gds-checkbox-group", Attributes = ForAttributeName)]
public class RspGdsCheckboxGroupTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)] public ModelExpression For { get; set; }

    [HtmlAttributeName("label-text")] public string LabelText { get; set; }

    [HtmlAttributeName("options")] public IEnumerable<string> Options { get; set; }

    [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var propertyName = For.Name;

        // Get selected values from model
        var selectedValues = For.Model as IEnumerable<string> ?? Enumerable.Empty<string>();

        // Check for validation errors
        ViewContext.ViewData.ModelState.TryGetValue(propertyName, out var modelStateEntry);
        var hasError = modelStateEntry != null && modelStateEntry.Errors.Count > 0;
        var errorHtml = hasError
            ? $"<span class='govuk-error-message'>{modelStateEntry.Errors[0].ErrorMessage}</span>"
            : "";

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", $"govuk-form-group {(hasError ? "govuk-form-group--error" : "")}");

        var fieldsetHtml = $@"
                <fieldset class='govuk-fieldset'>
                    <legend class='govuk-fieldset__legend govuk-fieldset__legend--l'>
                        <label class='govuk-label govuk-label--s' for='{propertyName}'>
                            {LabelText ?? propertyName}
                        </label>
                    </legend>
                    {errorHtml}
                    <div class='govuk-checkboxes' data-module='govuk-checkboxes' id='{propertyName}'>
                        {string.Join("\n", Options.Select(option =>
                        {
                            var isChecked = selectedValues.Contains(option) ? "checked" : "";
                            var safeId = $"{propertyName}_{option.Replace(" ", "_")}";
                            return $@"
                    <div class='govuk-checkboxes__item'>
                        <input class='govuk-checkboxes__input'
                               id='{safeId}'
                               name='{propertyName}'
                               type='checkbox'
                               value='{option}'
                               {isChecked} />
                        <label class='govuk-label govuk-checkboxes__label' for='{safeId}'>{option}</label>
                    </div>";
                        }))}
                    </div>
                </fieldset>";

        output.Content.SetHtmlContent(fieldsetHtml);
    }
}