using System;
using System.Threading.Tasks;
using Cuemon.AspNetCore.Configuration;
using Cuemon.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Codebelt.Website.TagHelpers
{
    [HtmlTargetElement("app-img")]
    public class ImageAppTagHelper : CdnTagHelper
    {
        public ImageAppTagHelper(IOptions<AppTagHelperOptions> setup, ICacheBusting cacheBusting = null) : base(setup, cacheBusting)
        {
        }

        public string Id { get; set; }

        public string Class { get; set; }

        public string Src { get; set; }

        public string Alt { get; set; }

        public string Title { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagOnly;
            output.TagName = "img";
            if (!string.IsNullOrWhiteSpace(Id)) { output.Attributes.Add("id", Id); }
            if (!string.IsNullOrWhiteSpace(Class)) { output.Attributes.Add("class", Class); }
            output.Attributes.Add("src", string.Concat(GetBaseUrl(), UseCacheBusting ? FormattableString.Invariant($"{Src}?v={CacheBusting.Version}") : Src));
            if (!string.IsNullOrWhiteSpace(Alt)) { output.Attributes.Add("alt", Alt); }
            if (!string.IsNullOrWhiteSpace(Title)) { output.Attributes.Add("title", Title); }
            return Task.CompletedTask;
        }
    }
}