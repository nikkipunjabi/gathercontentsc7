using System.Collections.Generic;
using System.Linq;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace GatherContent.Connector.SitecoreRepositories.Repositories
{
    public abstract class BaseSitecoreRepository
    {
        protected readonly Database ContextDatabase;
        protected readonly Language ContextLanguage;

        protected BaseSitecoreRepository()
        {
            ContextDatabase = Factory.GetDatabase("master");
            ContextLanguage = Context.Language;
        }

        public Item GetItem(string sitecoreId)
        {
            var resultItem = GetItem(sitecoreId, ContextLanguage);
            return resultItem;
        }

        public Item GetItem(string sitecoreId, Language language)
        {
            Item resultItem = null;
            if (!string.IsNullOrEmpty(sitecoreId))
            {
                resultItem = ContextDatabase.GetItem(sitecoreId, language);
            }

            return resultItem;
        }

        public Item GetItemByPath(string path)
        {
            Item resultItem = null;
            if (!string.IsNullOrEmpty(path))
            {
                resultItem = ContextDatabase.GetItem(path);
            }

            return resultItem;
        }

        public TemplateItem GetItemTemplate(ID sitecoreId)
        {
            return ContextDatabase.GetTemplate(sitecoreId);
        }

        protected IEnumerable<Item> GetAllProjects()
        {
            var accountSettingItem = ContextDatabase.GetItem(Constants.AccountItemId, ContextLanguage);
            var projects = accountSettingItem.Axes.SelectItems(string.Format("./descendant::*[@@templatename='{0}']", Constants.ProjectTemplateName));

            return projects == null ? Enumerable.Empty<Item>() : projects.ToList();
        }

        protected IEnumerable<Item> GetAllMappings()
        {
            var accountSettingItem = ContextDatabase.GetItem(Constants.AccountItemId, ContextLanguage);
            var mappings = accountSettingItem.Axes.SelectItems(string.Format("./descendant::*[@@templatename='{0}']", Constants.TemplateMappingName));

            return mappings == null ? Enumerable.Empty<Item>() : mappings.ToList();
        }

        /// <summary>
        ///     Get project folder by gatherContent Id
        /// </summary>
        /// <param name="projectId">GC project Id</param>
        /// <returns>Project folder Item</returns>
        protected Item GetProject(string projectId)
        {
            var accountSettingItem = ContextDatabase.GetItem(Constants.AccountItemId, ContextLanguage);
            var projects = accountSettingItem.Axes.SelectItems(string.Format("./descendant::*[@@templatename='{0}']", Constants.ProjectTemplateName));
            if (projects == null || !projects.Any())
            {
                return null;
            }
            var project = projects.FirstOrDefault(i => i["Id"] == projectId);
            return project;
        }
    }
}