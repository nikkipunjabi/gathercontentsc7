using GatherContent.Connector.WebControllers.Models.Import;

namespace GatherContent.Connector.WebControllers.Models.Update
{
   public class UpdateListItemViewModel
    {
 
       //public bool IsNeedToHighlightingDate { get; set; }

        public string Id { get; set; }
        public string ScTitle { get; set; }
        public string ScTemplateName { get; set; }

        public StatusViewModel Status { get; set; }
        public ItemViewModel GcItem  { get; set; }
        public ProjectViewModel GcProject { get; set; }

        public string LastUpdatedInGc { get; set; }
        public string LastUpdatedInSitecore { get; set; }

        public TemplateViewModel GcTemplate { get; set; }

        public string CmsLink { get; set; }
        public string GcLink { get; set; }
        
    }
}
