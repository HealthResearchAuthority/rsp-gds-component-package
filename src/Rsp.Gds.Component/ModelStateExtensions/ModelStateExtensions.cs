using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rsp.Gds.Component.ModelStateExtensions;

public static class ModelStateExtensions
{
    public static string GetGovUkErrorHtml(this ModelStateEntry entry, string validationMessage = null)
    {
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            var encoded = HtmlEncoder.Default.Encode(validationMessage);
            return $"<span class='govuk-error-message'>{encoded}</span>";
        }

        if (entry is { Errors.Count: > 0 })
        {
            var encodedErrors = entry.Errors
                .Select(e => HtmlEncoder.Default.Encode(e.ErrorMessage))
                .Where(e => !string.IsNullOrWhiteSpace(e));

            var errorMessage = string.Join("<br/>", encodedErrors);

            return string.IsNullOrWhiteSpace(errorMessage)
                ? string.Empty
                : $"<span class='govuk-error-message'>{errorMessage}</span>";
        }

        return string.Empty;
    }
}