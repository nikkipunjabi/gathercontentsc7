using GatherContent.Connector.Entities.Entities;
using GatherContent.Connector.GatherContentService.Services;
using GatherContent.Connector.WebControllers.IoC;
using Sitecore.Data.Items;
using Sitecore.Publishing.Pipelines.PublishItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GatherContent.Connector.Website.Events
{
    public class GatherContentStatusChangeProcessor
    {

        public void ItemProcessed(object sender, EventArgs args)
        {
            ItemProcessedEventArgs itemProcessedEventArgs = args as ItemProcessedEventArgs;
            PublishItemContext context = itemProcessedEventArgs != null ? itemProcessedEventArgs.Context : null;

            if (context != null)
            {
                var publishingItem = context.VersionToPublish;

                if (publishingItem != null)
                {
                    var gcId = publishingItem["GC Content Id"];

                    if (!String.IsNullOrEmpty(gcId))
                    {
                        var settings = ServiceFactory.Settings;

                        var gcItemService = new ItemsService(settings);
                        ItemEntity gcItem = gcItemService.GetSingleItem(gcId);
                        if (gcItem != null)
                        {
                            var gcProjectService = new ProjectsService(settings);
                            var allProjectStatuses = gcProjectService.GetAllStatuses(gcItem.Data.ProjectId.ToString());
                            if (allProjectStatuses != null && allProjectStatuses.Data.Any())
                            {
                                var targetStatus = allProjectStatuses.Data.FirstOrDefault(s => s.Name.ToLower() == settings.StatusAfterPublish.ToLower());
                                if (targetStatus != null)
                                {
                                    gcItemService.ChangeItemStatus(gcId, targetStatus.Id);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}