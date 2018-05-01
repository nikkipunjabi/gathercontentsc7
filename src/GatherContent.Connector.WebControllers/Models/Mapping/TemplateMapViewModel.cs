using System.Collections.Generic;
using GatherContent.Connector.WebControllers.Models.Import;

namespace GatherContent.Connector.WebControllers.Models.Mapping
{
    public class TemplateMapViewModel
    {
        public TemplateMapViewModel()
        {
            SitecoreTemplates = new List<SitecoreTemplateViewModel>();
            Rules = new Dictionary<string, string>();
            GcProjects = new List<ProjectViewModel>();
            SelectedFields = new List<FieldMappingViewModel>();
        }

        public string GcProjectName { get; set; }
        public string GcProjectId { get; set; }
        public string GcTemplateName { get; set; }
        public List<SitecoreTemplateViewModel> SitecoreTemplates { get; set; }
        public List<ProjectViewModel> GcProjects { get; set; }
        public AddMappingViewModel AddMappingModel { get; set; }
        public Dictionary<string, string> Rules { get; set; }
        public string ScMappingId { get; set; }
        public List<FieldMappingViewModel> SelectedFields { get; set; }
        public bool IsEdit { get; set; }
    }
}