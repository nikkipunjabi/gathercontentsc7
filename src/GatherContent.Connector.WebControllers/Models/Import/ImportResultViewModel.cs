namespace GatherContent.Connector.WebControllers.Models.Import
{
    public class ImportResultViewModel
    {
        public bool IsImportSuccessful { get; set; }
        public string Message { get; set; }
        public string GcTemplateName { get; set; }
        public string Title { get; set; }       
        public StatusViewModel Status { get; set; }       
        public string CmsLink { get; set; }
        public string GcLink { get; set; }
    }
}
