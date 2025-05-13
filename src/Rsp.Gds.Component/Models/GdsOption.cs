namespace Rsp.Gds.Component.Models;

/// <summary>
/// Represents a single option for use in GOV.UK-styled form components such as
/// radio groups, checkboxes, and select dropdowns.
/// </summary>
public class GdsOption
{
    /// <summary>
    /// The value attribute sent with the form submission when this option is selected.
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// The text displayed to the user for this option.
    /// </summary>
    public string Label { get; set; } = null!;
}
