using System;

namespace LunchBag.Common.EventMessages
{
    public class NoteCreatedMessage : INoteCreatedMessage
    {
        public string EventId { get; set; }
        public string Note { get; set; }
    }
}
