using System.Collections.Generic;

namespace GatherContent.Connector.WebControllers.Models.Mapping
{
    public class PostMappingViewModel
    {
        public PostMappingViewModel()
        {
            TemplateTabs = new List<TemplateTab>();
        }

        public List<TemplateTab> TemplateTabs { get; set; }
        public bool IsEdit { get; set; }
        public string TemplateId { get; set; }
        public string SelectedTemplateId { get; set; }
        public string GcMappingTitle { get; set; }
        public string ScMappingId { get; set; }
        public string DefaultLocation { get; set; }
    }
}
