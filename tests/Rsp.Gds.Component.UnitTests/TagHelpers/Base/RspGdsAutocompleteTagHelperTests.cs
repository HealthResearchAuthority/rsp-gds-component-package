namespace Rsp.Gds.Component.UnitTests.TagHelpers.Base
{
    public class RspGdsAutocompleteTagHelperTests
    {
        private static TagHelperContext CreateTagHelperContext() =>
            new("rsp-gds-autocomplete", new TagHelperAttributeList(), new Dictionary<object, object>(), "test");

        private static TagHelperOutput CreateTagHelperOutput() =>
            new("rsp-gds-autocomplete", new TagHelperAttributeList(), (useCachedResult, encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        private static ViewContext CreateViewContext(string fieldName, string value = null, string errorMessage = null)
        {
            var modelState = new ModelStateDictionary();
            if (errorMessage != null)
            {
                modelState.AddModelError(fieldName, errorMessage);
            }

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), modelState)
            {
                Model = null
            };

            return new ViewContext { ViewData = viewData };
        }

        private static ModelExpression CreateModelExpression(string name, object value)
        {
            var provider = new EmptyModelMetadataProvider();
            var metadata = provider.GetMetadataForType(value?.GetType() ?? typeof(string));
            var modelExplorer = new ModelExplorer(provider, metadata, value);
            return new ModelExpression(name, modelExplorer);
        }

        [Fact]
        public void Process_GeneratesExpectedHtml_ForValidInput()
        {
            var context = CreateTagHelperContext();
            var output = CreateTagHelperOutput();

            var tagHelper = new RspGdsAutocompleteTagHelper
            {
                For = CreateModelExpression("Organisation", "Health Org"),
                LabelText = "Organisation",
                ApiUrl = "/api/organisations",
                UseOrganisationId = true,
                DisplayName = "Displayed Org",
                ViewContext = CreateViewContext("Organisation")
            };

            tagHelper.Process(context, output);

            // Check content
            var html = output.Content.GetContent();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var label = doc.DocumentNode.SelectSingleNode("//label[not(contains(@class,'js-hidden'))]");
            label.InnerHtml.ShouldContain("Organisation");

            html.ShouldContain("initAutocomplete");
            html.ShouldContain("true");
            html.ShouldContain("Displayed Org");
        }

        [Fact]
        public void Process_SetsCustomAttributes_Correctly()
        {
            var context = CreateTagHelperContext();
            var output = CreateTagHelperOutput();

            var tagHelper = new RspGdsAutocompleteTagHelper
            {
                For = CreateModelExpression("Organisation", "Org"),
                LabelText = "Organisation",
                ApiUrl = "/api/organisations",
                ConditionalField = true,
                DataParentsAttr = "Q1,Q2",
                DataQuestionIdAttr = "Q3",
                HtmlId = "custom-id",
                HintHtml = "Pick your org",
                HintId = "hint-123",
                LabelAriaDescribedBy = "hint-123",
                ViewContext = CreateViewContext("Organisation")
            };

            tagHelper.Process(context, output);

            output.Attributes["data-parents"]?.Value.ShouldBe("Q1,Q2");
            output.Attributes["data-questionId"]?.Value.ShouldBe("Q3");
            output.Attributes["id"]?.Value.ShouldBe("custom-id");

            var html = output.Content.GetContent();
            html.ShouldContain("Pick your org");
            html.ShouldContain("aria-describedby='hint-123'");
        }
    }
}