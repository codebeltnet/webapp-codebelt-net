using System;
using System.Threading.Tasks;
using Cuemon.AspNetCore.Configuration;
using Cuemon.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Codebelt.Website.TagHelpers
{
    [HtmlTargetElement("app-link")]
    public class LinkAppTagHelper : CdnTagHelper
    {
        public LinkAppTagHelper(IOptions<AppTagHelperOptions> setup, ICacheBusting cacheBusting = null) : base(setup, cacheBusting)
        {
            Type = "text/css";
            Rel = "stylesheet";
        }

        public string Href { get; set; }

        public string Type { get; set; }

        public string Rel { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagOnly;
            output.TagName = "link";
            output.Attributes.Add("rel", Rel);
            output.Attributes.Add("href", string.Concat(GetBaseUrl(), UseCacheBusting ? FormattableString.Invariant($"{Href}?v={CacheBusting.Version}") : Href));
            if (!string.IsNullOrWhiteSpace(Type)) { output.Attributes.Add("type", new HtmlString(Type)); }
            return Task.CompletedTask;
        }
    }
}