using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class ConnectionMessage
    {
        public string Subject { get; set; }
        public object Message { get; set; }
        public ConnectionMessage(string subject, object message) 
        {
            Subject = subject;
            Message = message;
        }
    }
}
