namespace Rsp.Gds.Component.UnitTests.TagHelpers.Base;

public class RspGdsCheckboxGroupTagHelperTests
{
    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "rsp-gds-checkbox-group",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "rsp-gds-checkbox-group",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    private static ViewContext CreateViewContext(string fieldName, string? errorMessage = null)
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

        return new ViewContext
        {
            ViewData = viewData
        };
    }

    private static ModelExpression CreateModelExpression(string name, object? value)
    {
        var provider = new EmptyModelMetadataProvider();
        var metadata = provider.GetMetadataForType(value?.GetType() ?? typeof(IEnumerable<string>));
        var modelExplorer = new ModelExplorer(provider, metadata, value);
        return new ModelExpression(name, modelExplorer);
    }

    private static List<string> GetOptions()
    {
        return new List<string> { "Email", "Phone", "Post" };
    }

    // ---------------------------- Existing tests (unchanged) ----------------------------

    [Fact]
    public void Process_RendersAllCheckboxes_WithLabels()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("ContactMethods", new List<string>()),
            LabelText = "Preferred contact methods",
            Options = GetOptions(),
            ViewContext = CreateViewContext("ContactMethods")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var inputs = doc.DocumentNode.SelectNodes("//input[@type='checkbox']");
        inputs.Count.ShouldBe(3);

        foreach (var option in GetOptions())
        {
            html.ShouldContain(option);
        }
    }

    [Fact]
    public void Process_ChecksSelectedCheckboxes()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("ContactMethods", new List<string> { "Email", "Post" }),
            LabelText = "Contact",
            Options = GetOptions(),
            ViewContext = CreateViewContext("ContactMethods")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var checkedInputs = doc.DocumentNode.SelectNodes("//input[@checked]");
        var values = checkedInputs.Select(i => i.GetAttributeValue("value", "")).ToList();

        values.ShouldContain("Email");
        values.ShouldContain("Post");
        values.ShouldNotContain("Phone");
    }

    [Fact]
    public void Process_RendersErrorMessage_WhenModelStateIsInvalid()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("Interests", null),
            LabelText = "Choose at least one",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Interests", "You must choose at least one")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        html.ShouldContain("You must choose at least one");
        output.Attributes["class"].Value.ToString().ShouldContain("govuk-form-group--error");
    }

    [Fact]
    public void Process_RendersUnchecked_WhenModelIsNull()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("Alerts", null),
            LabelText = "Alert options",
            Options = GetOptions(),
            ViewContext = CreateViewContext("Alerts")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var checkedInputs = doc.DocumentNode.SelectNodes("//input[@type='checkbox'][@checked]");
        (checkedInputs == null || checkedInputs.Count == 0).ShouldBeTrue();
    }

    // ----------------------------
    // NEW: Readonly / partial-readonly coverage ----------------------------

    [Fact]
    public void Process_ReadOnlyTrue_DisablesAllOptions_AndPreservesSelectedValuesWithHiddenInputs()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("ContactMethods", new List<string> { "Email", "Post" }),
            LabelText = "Contact",
            Options = GetOptions(),
            ReadOnly = true,
            ViewContext = CreateViewContext("ContactMethods")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // All checkboxes disabled
        var disabledCheckboxes = doc.DocumentNode.SelectNodes("//input[@type='checkbox' and @disabled]");
        disabledCheckboxes.ShouldNotBeNull();
        disabledCheckboxes.Count.ShouldBe(3);

        // Checked state still rendered
        var checkedCheckboxes = doc.DocumentNode.SelectNodes("//input[@type='checkbox' and @checked]");
        checkedCheckboxes.ShouldNotBeNull();
        checkedCheckboxes.Select(x => x.GetAttributeValue("value", "")).ShouldBe(new[] { "Email", "Post" }, ignoreOrder: true);

        // Disabled checkboxes don't post, so selected should be preserved as hidden inputs:
        // name=ContactMethods, value=Email/Post
        var hiddenSelected = doc.DocumentNode.SelectNodes("//input[@type='hidden' and @name='ContactMethods']");
        hiddenSelected.ShouldNotBeNull();

        var hiddenValues = hiddenSelected.Select(x => x.GetAttributeValue("value", "")).ToList();
        hiddenValues.ShouldContain("Email");
        hiddenValues.ShouldContain("Post");
        hiddenValues.ShouldNotContain("Phone");
    }

    [Fact]
    public void Process_ReadOnlyItems_DisablesOnlyMatchingOptions_AndAddsHiddenOnlyForThoseSelected()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        // Selected: Email + Post. Lock: Post only.
        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("ContactMethods", new List<string> { "Email", "Post" }),
            LabelText = "Contact",
            Options = GetOptions(),
            ReadOnlyItems = "Post",
            ViewContext = CreateViewContext("ContactMethods")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Only Post checkbox disabled
        var disabled = doc.DocumentNode.SelectNodes("//input[@type='checkbox' and @disabled]");
        disabled.ShouldNotBeNull();
        disabled.Count.ShouldBe(1);
        disabled.Single().GetAttributeValue("value", "").ShouldBe("Post");

        // Email should not be disabled
        var email = doc.DocumentNode.SelectSingleNode("//input[@type='checkbox' and @value='Email']");
        email.ShouldNotBeNull();
        email.Attributes["disabled"].ShouldBeNull();

        // Hidden "ContactMethods" values should include Post (because it's selected & disabled),
        // but not Email (still enabled)
        var hiddenSelected = doc.DocumentNode.SelectNodes("//input[@type='hidden' and @name='ContactMethods']");
        hiddenSelected.ShouldNotBeNull();
        hiddenSelected.Count.ShouldBe(1);
        hiddenSelected.Single().GetAttributeValue("value", "").ShouldBe("Post");
    }

    private sealed class RoleItem
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public bool IsSelected { get; set; }
    }

    [Fact]
    public void Process_ComplexModel_ReadOnlyItemsByName_DisablesOnlyMatchingItems_AndPreservesIsSelectedWithHiddenBoolean()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var roles = new List<object>
        {
            new RoleItem { Id = "1", Name = "organisation_admin", DisplayName = "Organisation admin", IsSelected = true },
            new RoleItem { Id = "2", Name = "reviewer",           DisplayName = "Reviewer",           IsSelected = true },
            new RoleItem { Id = "3", Name = "sponsor",            DisplayName = "Sponsor",            IsSelected = false },
        };

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("UserRoles", roles),
            LabelText = "Role",
            ItemLabelProperty = "DisplayName",
            ItemValueProperty = "IsSelected",
            ItemHiddenProperties = "Id,Name",
            ReadOnlyItems = "organisation_admin,sponsor",
            ReadOnlyItemProperty = "Name",
            ViewContext = CreateViewContext("UserRoles")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // There are 3 complex checkboxes
        var checkboxes = doc.DocumentNode.SelectNodes("//input[@type='checkbox']");
        checkboxes.ShouldNotBeNull();
        checkboxes.Count.ShouldBe(3);

        // Disabled should be for index 0 (org admin) and index 2 (sponsor)
        var disabledCheckboxes = doc.DocumentNode.SelectNodes("//input[@type='checkbox' and @disabled]");
        disabledCheckboxes.ShouldNotBeNull();
        disabledCheckboxes.Count.ShouldBe(2);

        // Identify by their generated names: UserRoles[0].IsSelected and UserRoles[2].IsSelected
        disabledCheckboxes.Select(x => x.GetAttributeValue("name", ""))
            .ShouldBe(new[] { "UserRoles[0].IsSelected", "UserRoles[2].IsSelected" }, ignoreOrder: true);

        // Hidden bool should be present for readonly items only, with their current values: index 0
        // => true, index 2 => false
        var hiddenBool0 = doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[0].IsSelected']");
        hiddenBool0.ShouldNotBeNull();
        hiddenBool0.GetAttributeValue("value", "").ShouldBe("true");

        var hiddenBool2 = doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[2].IsSelected']");
        hiddenBool2.ShouldNotBeNull();
        hiddenBool2.GetAttributeValue("value", "").ShouldBe("false");

        // Non-readonly index 1 should NOT have the hidden bool helper (it posts normally)
        var hiddenBool1 = doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[1].IsSelected']");
        hiddenBool1.ShouldBeNull();

        // Hidden properties (Id/Name) should exist for each item
        doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[0].Id' and @value='1']").ShouldNotBeNull();
        doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[0].Name' and @value='organisation_admin']").ShouldNotBeNull();

        doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[1].Id' and @value='2']").ShouldNotBeNull();
        doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[1].Name' and @value='reviewer']").ShouldNotBeNull();

        doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[2].Id' and @value='3']").ShouldNotBeNull();
        doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[2].Name' and @value='sponsor']").ShouldNotBeNull();
    }

    [Fact]
    public void Process_ComplexModel_GlobalReadOnly_DisablesAllItems_AndAddsHiddenBooleanForAll()
    {
        var context = CreateTagHelperContext();
        var output = CreateTagHelperOutput();

        var roles = new List<object>
        {
            new RoleItem { Id = "1", Name = "organisation_admin", DisplayName = "Organisation admin", IsSelected = true },
            new RoleItem { Id = "2", Name = "reviewer",           DisplayName = "Reviewer",           IsSelected = false },
        };

        var tagHelper = new RspGdsCheckboxGroupTagHelper
        {
            For = CreateModelExpression("UserRoles", roles),
            LabelText = "Role",
            ItemLabelProperty = "DisplayName",
            ItemValueProperty = "IsSelected",
            ItemHiddenProperties = "Id,Name",
            ReadOnly = true,
            ViewContext = CreateViewContext("UserRoles")
        };

        tagHelper.Process(context, output);

        var html = output.Content.GetContent();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // All checkboxes disabled
        var disabledCheckboxes = doc.DocumentNode.SelectNodes("//input[@type='checkbox' and @disabled]");
        disabledCheckboxes.ShouldNotBeNull();
        disabledCheckboxes.Count.ShouldBe(2);

        // Hidden bool emitted for both
        var hiddenBool0 = doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[0].IsSelected']");
        hiddenBool0.ShouldNotBeNull();
        hiddenBool0.GetAttributeValue("value", "").ShouldBe("true");

        var hiddenBool1 = doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='UserRoles[1].IsSelected']");
        hiddenBool1.ShouldNotBeNull();
        hiddenBool1.GetAttributeValue("value", "").ShouldBe("false");
    }
}