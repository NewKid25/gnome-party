using System;
using System.Collections.Generic;
using System.Text;

namespace Models.GameMetaData
{
    public class ConnectionMessage
    {
        public object Message { get; set; }
        public string Subject { get; set; }
        public ConnectionMessage(string subject, object message) 
        {
            Subject = subject;
            Message = message;
        }
    }
}
