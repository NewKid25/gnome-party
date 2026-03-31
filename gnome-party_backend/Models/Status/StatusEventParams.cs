using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Status
{
    public class StatusTickEventParams
    {
        public string StatusType { get; set; } = "";
        public string SourceId { get; set; } = "";
        public string TargetId { get; set; } = "";
        public string TargetName { get; set; } = "";
        public int StatusAmount { get; set; }
    }
    public class StatusAppliedEventParams
    {
        public string StatusType { get; set; } = "";
        public string SourceId { get; set; } = "";
        public string OwnerId { get; set; } = "";
    }
    public class StatusExpiredEventParams
    {
        public string StatusType { get; set; } = "";
        public string OwnerId { get; set; } = "";
    }
}
