/**
 * Initializes the accessible autocomplete functionality for a given input field.
 *
 * @param {string} autoCompleteInputId - The ID for the visible autocomplete input field.
 * @param {string} inputIdForSubmission - The ID of the hidden input field to store the final value.
 * @param {string} defaultValue - The default text to pre-fill in the autocomplete.
 * @param {string} apiUrl - The endpoint to call for suggestions.
 * @param {string} containerId - The container DOM element to attach the autocomplete UI to.
 */
function initAutocomplete(autoCompleteInputId, inputIdForSubmission, defaultValue, apiUrl, containerId) {
    const beforeSuggestionsText = 'Suggestions';
    const afterSuggestionsText = 'Continue entering to improve suggestions';
    const noResultsText = 'No suggestions found.';
    let resultsFound = false;
    let requestToken = 0;

    accessibleAutocomplete({
        element: document.querySelector(`#${containerId}`),
        id: autoCompleteInputId,
        defaultValue: defaultValue,
        minLength: 1,
        autoselect: false,
        displayMenu: 'overlay',
        inputClasses: 'govuk-input govuk-!-width-three-quarters',
        menuClasses: 'govuk-!-width-three-quarters',
        confirmOnBlur: false,

        source: function (query, populateResults) {
            requestToken++;
            const currentToken = requestToken;

            if (query.length < 3) {
                populateResults([]);
                $(`.autocomplete__menu`).attr('data-before-suggestions', '');
                $(`.autocomplete__menu`).attr('data-after-suggestions', afterSuggestionsText);
                return;
            }

            $.ajax({
                url: apiUrl,
                method: 'GET',
                data: { name: query },
                dataType: 'json',
                success: function (data) {
                    if (currentToken !== requestToken) return;

                    const currentValue = $(`#${autoCompleteInputId}`).val();
                    if (!currentValue || currentValue.length < 3) return;

                    if (!data || data.length === 0) {
                        populateResults([]);
                        return;
                    }

                    resultsFound = true;
                    $(`.autocomplete__menu`).attr('data-before-suggestions', beforeSuggestionsText);
                    $(`.autocomplete__menu`).attr('data-after-suggestions', afterSuggestionsText);
                    populateResults(data);
                    $(`#${inputIdForSubmission}`).val('');
                },
                error: function () {
                    if (currentToken !== requestToken) return;

                    console.error('Error fetching suggestions from', apiUrl);
                    populateResults([]);
                }
            });
        },

        onConfirm: function (suggestion) {
            $(`#${inputIdForSubmission}`).val(suggestion || '');
        },

        templates: {
            suggestion: function (suggestion) {
                const query = $(`#${autoCompleteInputId}`).val();
                const escapedQuery = query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
                const regex = new RegExp('(' + escapedQuery + ')', 'gi');

                if (resultsFound) {
                    return suggestion.replace(regex, '<strong>$1</strong>');
                }

                $(`.autocomplete__menu`).attr('data-before-suggestions', '');
                $(`.autocomplete__menu`).attr('data-after-suggestions', '');
                return '';
            }
        },

        tNoResults: function () {
            const query = $(`#${autoCompleteInputId}`).val();
            if (query.length < 3) {
                $(`.autocomplete__menu`).attr('data-before-suggestions', afterSuggestionsText);
                $(`.autocomplete__menu`).attr('data-after-suggestions', '');
                return '';
            }

            $(`#${inputIdForSubmission}`).val('');
            $(`.autocomplete__menu`).attr('data-before-suggestions', '');
            $(`.autocomplete__menu`).attr('data-after-suggestions', '');
            return noResultsText;
        }
    });

    // Hide original fallback input and label, show the JS-enabled input/label
    $(`#${inputIdForSubmission}`).hide();
    $(`label[for="${inputIdForSubmission}"]`).hide();
    $(`label[for="${autoCompleteInputId}"]`).show();

    // Clear hidden input and menu if user clears visible input
    $(`#${autoCompleteInputId}`).on('input', function () {
        if (!this.value) {
            $(`#${inputIdForSubmission}`).val('');
            $(`.autocomplete__menu`).html('');
            resultsFound = false;
        }
    });
}
