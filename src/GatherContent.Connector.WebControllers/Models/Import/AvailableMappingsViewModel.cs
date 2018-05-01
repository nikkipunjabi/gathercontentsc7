using System.Collections.Generic;

namespace GatherContent.Connector.WebControllers.Models.Import
{
    public class AvailableMappingsViewModel
    {
        public AvailableMappingsViewModel()
        {
            Mappings = new List<AvailableMappingViewModel>();
        }
        public string SelectedMappingId { get; set; }
        public List<AvailableMappingViewModel> Mappings { get; set; }
    }
}
