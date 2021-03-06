using System;
using CleanCityCore.EmailSender;

namespace CleanCityCore.Model
{
    public class Report
    {
        public long UserId { get; set; }
        public DateTime CreationDate { get; set; }
        public GeoLocation Location { get; set; }
        public Attachment[] Attachments { get; set; }
        public string Subject { get; set; }
        public string ReportText { get; set; }
        public Guid ResponsibleId { get; set; }
    }
}
