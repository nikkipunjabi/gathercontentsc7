using System.Collections.Generic;

namespace GatherContent.Connector.WebControllers.Models.Import
{
    public class FiltersViewModel
    {
        public FiltersViewModel()
        {
            Statuses = new List<StatusViewModel>();
            Projects = new List<ProjectViewModel>();
            Templates = new List<TemplateViewModel>();
        }
        public ProjectViewModel Project { get; set; }

        public List<ProjectViewModel> Projects { get; set; }

        public List<TemplateViewModel> Templates { get; set; }

        public List<StatusViewModel> Statuses { get; set; }


    }
}
