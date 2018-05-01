
using System.Collections.Generic;
using GatherContent.Connector.IRepositories.Models.Import;

namespace GatherContent.Connector.IRepositories.Models.Mapping
{
    public class FieldMapping
    {
	    public string CmsFieldMappingItemId { get; set; }
	    public string CmsFieldMappingItemUrl { get; set; }
		public CmsField CmsField { get; set; }
        public IList<CmsField> CmsFieldAdditionalMapping { get; set; }
        public GcField GcField { get; set; }
    }
}
