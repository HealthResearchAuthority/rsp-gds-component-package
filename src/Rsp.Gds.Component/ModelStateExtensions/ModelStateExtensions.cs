namespace Rsp.Gds.Component.ModelStateExtensions;

/// <summary>
///     Provides extension methods for formatting model state validation errors in GOV.UK-compatible markup.
/// </summary>
public static class ModelStateExtensions
{
    /// <summary>
    ///     Returns the validation error(s) from a <see cref="ModelStateEntry" /> formatted as a GOV.UK-style error message
    ///     span.
    /// </summary>
    /// <param name="entry">The model state entry to inspect.</param>
    /// <param name="validationMessage">
    ///     Optional override error message. If provided, this is used instead of any model state error messages.
    /// </param>
    /// <returns>
    ///     A string containing a <c>&lt;span class="govuk-error-message"&gt;</c> element, or an empty string if there are no
    ///     errors.
    /// </returns>
    public static string GetGovUkErrorHtml(this ModelStateEntry entry, string validationMessage = null)
    {
        // If a manual validation message is supplied, use it instead of model state errors
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            var encoded = HtmlEncoder.Default.Encode(validationMessage);
            return $"<span class='govuk-error-message'>{encoded}</span>";
        }

        // If there are one or more errors in the model state
        if (entry is { Errors.Count: > 0 })
        {
            // Encode all error messages and filter out any empty/null strings
            var encodedErrors = entry.Errors
                .Select(e => HtmlEncoder.Default.Encode(e.ErrorMessage))
                .Where(e => !string.IsNullOrWhiteSpace(e));

            // Join errors with <br/> to allow multiple error lines in one span
            var errorMessage = string.Join("<br/>", encodedErrors);

            return string.IsNullOrWhiteSpace(errorMessage)
                ? string.Empty
                : $"<span class='govuk-error-message'>{errorMessage}</span>";
        }

        // No errors to display
        return string.Empty;
    }
}