namespace GatherContent.Connector.Entities
{
    public class GCAccountSettings
    {
        public string ApiUrl { get; set; }

        public string Username { get; set; }

        public string ApiKey { get; set; }

        public string TemplateFolderId { get; set; }

        /// <summary>
        /// Date format used for datetime output on application's dialogs and forms
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Format used to parse datetime fields
        /// </summary>
        public string DateTimeParseFormat { get; set; }

        public string AccountItemId { get; set; }

        public string GatherContentUrl { get; set; }

        public string DropTreeHomeNode { get; set; }

        public string StatusAfterPublish { get; set; }
    }
}
