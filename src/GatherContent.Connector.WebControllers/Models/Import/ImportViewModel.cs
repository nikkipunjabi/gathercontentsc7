using System.Collections.Generic;

namespace GatherContent.Connector.WebControllers.Models.Import
{
    public class ImportViewModel
    {
        public ImportViewModel()
        {
            Items = new List<ImportListItemViewModel>();
            Filters = new FiltersViewModel();
            Languages = new List<LanguageViewModel>();
        }
        public FiltersViewModel Filters { get; set; }

        public List<ImportListItemViewModel> Items { get; set; }

        public List<LanguageViewModel> Languages { get; set; }

    }
}
