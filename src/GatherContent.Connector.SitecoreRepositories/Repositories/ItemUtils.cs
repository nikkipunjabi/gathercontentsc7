using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Sitecore.Configuration;
using Sitecore.Data.Items;

namespace GatherContent.Connector.SitecoreRepositories.Repositories
{
	public static class ItemUtils
	{
		public static string ProposeValidItemName(this string itemName, bool replaceSpacesWithDash = true)
		{
			if (String.IsNullOrEmpty(itemName))
			{
				return String.Empty;
			}

			if (itemName.IndexOf('.') != -1)
			{
				itemName = itemName.Substring(0, itemName.LastIndexOf('.'));
			}

			itemName = WebUtility.HtmlDecode(itemName);
			itemName = new Regex("[\\\\,`~#%&*{}()/:;<>?|'\"—.-]").Replace(itemName, " ");

			if (itemName.IndexOfAny(Settings.InvalidItemNameChars) >= 0)
			{
				itemName = Settings.InvalidItemNameChars.Aggregate(itemName, (c1, c2) => c1.Replace(c2, ' '));
			}

			itemName = itemName.Trim();

			itemName = ItemUtil.ProposeValidItemName(itemName);

			itemName = Regex.Replace(itemName, @"\s+", " "); //replace 2 or more spaces to one
			itemName = itemName.Trim();

			var maxItemnameLength = Settings.MaxItemNameLength - 2;
			if (itemName.Length > maxItemnameLength)
			{
				itemName = itemName.Substring(0, maxItemnameLength);
				itemName = itemName.Substring(0, itemName.LastIndexOf(" ", StringComparison.Ordinal)).Trim();
			}

			if (replaceSpacesWithDash)
			{
				itemName = itemName.Replace(" ", "-");
			}

            return itemName.ToLower();
		}
	}
}
