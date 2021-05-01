using System;
using System.Threading.Tasks;
using Cuemon.AspNetCore.Configuration;
using Cuemon.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Codebelt.Website.TagHelpers
{
    [HtmlTargetElement("app-script")]
    public class ScriptAppTagHelper : CdnTagHelper
    {
        public ScriptAppTagHelper(IOptions<CdnTagHelperOptions> setup, ICacheBusting cacheBusting = null) : base(setup, cacheBusting)
        {
        }

        public string Src { get; set; }

        public bool Defer { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "script";
            output.Attributes.Add("type", "text/javascript");
            output.Attributes.Add("src", string.Concat(GetBaseUrl(), UseCacheBusting ? FormattableString.Invariant($"{Src}?v={CacheBusting.Version}") : Src));
            if (Defer) { output.Attributes.Add("defer", "defer"); }
            return Task.CompletedTask;
        }
    }
}