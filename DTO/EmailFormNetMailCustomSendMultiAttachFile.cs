namespace PRJ_WAREHOUSE_BIVN.DTO
{
    public class EmailFormNetMailCustomSendMultiAttachFile
    {
        public string mail_from { get; set; }
        public string mail_to { get; set; }
        public string mail_cc { get; set; }
        public string mail_bcc { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public List<string> attachmentPaths { get; set; }
    }
}
