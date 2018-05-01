
using System.Collections.Generic;

namespace GatherContent.Connector.WebControllers.Models.Mapping
{
    public class FieldMappingViewModel
    {
        public string SitecoreTemplateId { get; set; }
        public string GcFieldId { get; set; }
	    public IList<string> CmsTemplateNamesAdditional { get; set; }
	    public string CmsFieldMappingItemId { get; set; }
	    public string CmsFieldMappingItemUrl { get; set; }
	}
}
