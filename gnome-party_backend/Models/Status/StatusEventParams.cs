using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Status
{
    // Event parameter classes for status-related combat events
    public class StatusTickEventParams
    {
        public string SourceId { get; set; } = "";
        public int StatusAmount { get; set; }
        public string StatusType { get; set; } = "";
        public string CharacterId { get; set; } = "";
    }
    public class StatusAppliedEventParams
    {
        public string OwnerId { get; set; } = "";
    }
    public class StatusExpiredEventParams
    {
        public string CharacterId { get; set; } = "";
        public string StatusType { get; set; } = "";
    }
}
