using LunchBag.Common.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace LunchBag.Common.Test
{
    public static class DBMock
    {
        public static Dictionary<string, EventModel> _DBEvents;
        public static List<NoteTemplateModel> _DBNoteTemplates;
        public static List<NoteModel> _DBNotes;

        public static void Init()
        {
            _DBEvents = new Dictionary<string, EventModel>();
            _DBEvents.Add("Event1", new EventModel()
            {
                Id = "Event1",
                ETag = "ABC",
                Date = new DateTime(2018, 1, 1),
                EventName = "Event 1",
                EventLocations = new List<EventLocationModel>()
                {
                    new EventLocationModel()
                    {
                        Id = "Location1",
                        LocationName = "Location 1",
                        Goal = 100,
                        GoalStatus = 0
                    },
                    new EventLocationModel()
                    {
                        Id = "Location2",
                        LocationName = "Location 2",
                        Goal = 100,
                        GoalStatus = 0
                    }
                },
                EventSentiments = new List<EventSentimentModel>()
                {
                    new EventSentimentModel()
                    {
                        Name = "Hope",
                        Percentage = 0,
                        Value = 0
                    },
                    new EventSentimentModel()
                    {
                        Name = "Love",
                        Percentage = 0,
                        Value = 0
                    },
                    new EventSentimentModel()
                    {
                        Name = "Care",
                        Percentage = 0,
                        Value = 0
                    }
                },
                EventViews = new List<EventViewModel>()
                {

                }, 
                IsEventActive = true,
                LastNote = string.Empty,
                PayPalApi = new EventPayPalApiModel()
            });
            _DBEvents.Add("Event2", new EventModel()
            {
                Id = "Event2",
                ETag = "ABC",
                Date = new DateTime(2018, 1, 1),
                EventName = "Event 2",
                EventLocations = new List<EventLocationModel>()
                {
                    new EventLocationModel()
                    {
                        Id = "Location1",
                        LocationName = "Location 1",
                        Goal = 100,
                        GoalStatus = 0
                    },
                    new EventLocationModel()
                    {
                        Id = "Location2",
                        LocationName = "Location 2",
                        Goal = 100,
                        GoalStatus = 0
                    }
                },
                EventSentiments = new List<EventSentimentModel>()
                {
                    new EventSentimentModel()
                    {
                        Name = "Brave",
                        Percentage = 0,
                        Value = 0
                    },
                    new EventSentimentModel()
                    {
                        Name = "Courage",
                        Percentage = 0,
                        Value = 0
                    },
                    new EventSentimentModel()
                    {
                        Name = "Love",
                        Percentage = 0,
                        Value = 0
                    }
                },
                EventViews = new List<EventViewModel>()
                {

                },
                IsEventActive = true,
                LastNote = string.Empty,
                PayPalApi = new EventPayPalApiModel()
            });

            _DBNoteTemplates = new List<NoteTemplateModel>();
            _DBNoteTemplates.Add(new NoteTemplateModel()
            {
                Date = new DateTime(2018, 1, 1),
                ETag = "ABC",
                EventId = "Event1",
                Id = Guid.NewGuid().ToString(),
                Note = "This is a note about hope",
                Sentiment = "Hope"
            });
            _DBNoteTemplates.Add(new NoteTemplateModel()
            {
                Date = new DateTime(2018, 1, 1),
                ETag = "ABC",
                EventId = "Event1",
                Id = Guid.NewGuid().ToString(),
                Note = "We all want love",
                Sentiment = "Love"
            });
            _DBNoteTemplates.Add(new NoteTemplateModel()
            {
                Date = new DateTime(2018, 1, 1),
                ETag = "ABC",
                EventId = "Event1",
                Id = Guid.NewGuid().ToString(),
                Note = "We care about you!",
                Sentiment = "Care"
            });
            _DBNoteTemplates.Add(new NoteTemplateModel()
            {
                Date = new DateTime(2018, 1, 1),
                ETag = "ABC",
                EventId = "Event2",
                Id = Guid.NewGuid().ToString(),
                Note = "Bravery is the key",
                Sentiment = "Brave"
            });
            _DBNoteTemplates.Add(new NoteTemplateModel()
            {
                Date = new DateTime(2018, 1, 1),
                ETag = "ABC",
                EventId = "Event2",
                Id = Guid.NewGuid().ToString(),
                Note = "You need courage",
                Sentiment = "Courage"
            });
            _DBNoteTemplates.Add(new NoteTemplateModel()
            {
                Date = new DateTime(2018, 1, 1),
                ETag = "ABC",
                EventId = "Event2",
                Id = Guid.NewGuid().ToString(),
                Note = "We hope you feel better soon!",
                Sentiment = "Love"
            });

            _DBNotes = new List<NoteModel>();
        }

        public static EventModel GetEvent(string eventId)
        {
            if(_DBEvents.ContainsKey(eventId))
                return _DBEvents[eventId];
            return null;
        }

        public static HttpStatusCode UpdateEvent(EventModel eventObj)
        {
            if (eventObj.Id == "EventETAG")
                return HttpStatusCode.PreconditionFailed;

            if (_DBEvents.ContainsKey(eventObj.Id))
            {
                _DBEvents[eventObj.Id] = eventObj;
                return HttpStatusCode.OK;
            }
            else
                return HttpStatusCode.InternalServerError;
        }

        public static IEnumerable<NoteTemplateModel> GetNoteTemplates(string eventId)
        {
            return _DBNoteTemplates?.FindAll(p => p.EventId == eventId);
        }

        public static IEnumerable<NoteModel> GetNotes(string eventId)
        {
            return _DBNotes?.FindAll(p => p.EventId == eventId);
        }

        public static string CreateNote(NoteModel note)
        {
            note.Id = Guid.NewGuid().ToString();
            note.ETag = "ABC";
            note.Date = DateTime.Today;
            if (_DBNotes != null) {
                _DBNotes.Add(note);
            }
            return note.Id;
        }
    }
}
