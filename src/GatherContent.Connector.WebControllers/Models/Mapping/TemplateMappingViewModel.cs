
namespace GatherContent.Connector.WebControllers.Models.Mapping
{
    public class TemplateMappingViewModel
    {
        public string GcProjectName { get; set; }
        public string GcTemplateId { get; set; }
        public string GcTemplateName { get; set; }
        public string ScTemplateName { get; set; }
        public string MappingTitle { get; set; }
        public string LastMappedDateTime { get; set; }
        public string LastUpdatedDate { get; set; }
        public string ScMappingId { get; set; }
        public bool IsMapped { get; set; }
        public bool RemovedFromGc { get; set; }
        public bool IsHighlightingDate { get; set; }
    }
}
