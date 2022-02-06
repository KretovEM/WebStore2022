﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebStore.TagHelpers;

// You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
[HtmlTargetElement(Attributes = AttributeName)]
public class ActiveRoute : TagHelper
{
    private const string AttributeName = "is-active-route";

    [HtmlAttributeName("asp-controller")]
    public string Controller { get; set; }

    [HtmlAttributeName("asp-action")]
    public string Action { get; set; }

    [HtmlAttributeName("asp-all-route-data", DictionaryAttributePrefix = "asp-route-")]
    public Dictionary<string, string> RouteValues { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.RemoveAll(AttributeName);


    }
}