using System.Text.Encodings.Web;
using Rsp.Gds.Component.ModelStateExtensions;

namespace Rsp.Gds.Component.UnitTests.ModelStateExtensions;

public class ModelStateExtensionsTests
{
    private static ModelStateEntry GetEntryWithErrors(params string[] errors)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in errors)
        {
            modelState.AddModelError("TestField", error);
        }

        return modelState["TestField"];
    }

    [Fact]
    public void Returns_ValidationMessage_When_Provided()
    {
        var entry = GetEntryWithErrors("Should not show");
        var validationMessage = "<strong>Error!</strong>";

        var result = entry.GetGovUkErrorHtml(validationMessage);

        result.ShouldBe($"<span class='govuk-error-message'>{HtmlEncoder.Default.Encode(validationMessage)}</span>");
    }

    [Fact]
    public void Returns_EncodedErrors_When_EntryHasMultipleErrors()
    {
        var entry = GetEntryWithErrors("<script>alert('one')</script>", "Second error");

        var result = entry.GetGovUkErrorHtml();

        result.ShouldBe("<span class='govuk-error-message'>" +
                        HtmlEncoder.Default.Encode("<script>alert('one')</script>") +
                        "<br/>" +
                        HtmlEncoder.Default.Encode("Second error") +
                        "</span>");
    }

    [Fact]
    public void Returns_Empty_When_NoErrorsAndNoMessage()
    {
        var modelState = new ModelStateDictionary();
        modelState.SetModelValue("TestField", "", "");
        var entry = modelState["TestField"];

        var result = entry.GetGovUkErrorHtml();

        result.ShouldBe(string.Empty);
    }
}