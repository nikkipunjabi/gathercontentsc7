
using System.Collections.Generic;

namespace GatherContent.Connector.Managers.Models.Mapping
{
    public class FieldMappingModel
    {
        public string CmsTemplateId { get; set; }
        public IList<string> CmsTemplateNamesAdditional { get; set; }
        public string GcFieldId { get; set; }
        public string GcFieldName { get; set; }
        public string CmsFieldMappingItemId { get; set; }
        public string CmsFieldMappingItemUrl { get; set; }
    }
}
