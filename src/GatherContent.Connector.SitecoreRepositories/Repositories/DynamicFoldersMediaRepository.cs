using System.Linq;
using GatherContent.Connector.IRepositories.Models.Import;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace GatherContent.Connector.SitecoreRepositories.Repositories
{
	public class DynamicFoldersMediaRepository : SimpleMediaRepository
	{
		public override string ResolveMediaPath(CmsItem item, Item createdItem, CmsField cmsField)
		{
			Field scField = createdItem.Fields[new ID(cmsField.TemplateField.FieldId)];
			string dataSourcePath = GetItem(scField.ID.ToString())["Source"];
			if (string.IsNullOrEmpty(dataSourcePath) || GetItem(dataSourcePath) == null)
			{
				dataSourcePath = MediaFolderRoot;
			}

			Item parent = createdItem.Parent;
			string subfolder = string.Empty;
			while (parent != null && ItemIsGcLinked(parent))
			{
				subfolder = "/" + parent.Name + subfolder;

				parent = parent.Parent;
			}

			dataSourcePath = dataSourcePath + subfolder;

			var path = string.IsNullOrEmpty(cmsField.TemplateField.FieldName)
				? string.Format("{0}/{1}/", dataSourcePath, item.Title)
				: string.Format("{0}/{1}/{2}/", dataSourcePath, item.Title, cmsField.TemplateField.FieldName);

			return path;
		}

		private bool ItemIsGcLinked(Item item)
		{
			if (item.TemplateName == Constants.GCNotMappedItemTemplateName || item.TemplateName == Constants.GCLinkItemTemplateName)
			{
				return true;
			}

			if (item.Template.BaseTemplates.Any(i => (i.Name == Constants.GCNotMappedItemTemplateName || i.Name == Constants.GCLinkItemTemplateName)))
			{
				return true;
			}

			return false;
		}
	}
}