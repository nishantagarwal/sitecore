using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Custom.Providers
{
    /// <summary>
    /// Class to generate URL for Sitecore items.
    /// </summary>
    public class LinkProvider : Sitecore.Links.LinkProvider
    {
        /// <summary>
        /// Returns user friendly URL for given Sitecore item.
        /// </summary>
        /// <param name="item">The Sitecore item for which URL is needed.</param>
        /// <param name="options">The options to be considered when generating URL</param>
        /// <returns>User friendly URL for Sitecore item.</returns>
        public override string GetItemUrl(Sitecore.Data.Items.Item item, Sitecore.Links.UrlOptions options)
        {
            // Get URL from Sitecore out of box link provider.
            string originalURL = base.GetItemUrl(item, options);

            // Get contentStartItem path for context site.
            string contentStartItem = Sitecore.Context.Site.RootPath.ToLower();

            // Replace contentStartItem path in originalURL with empty string and return the resultant string.
            return originalURL.ToLower().Replace(contentStartItem, String.Empty);
        }
    }
}