using Newtonsoft.Json;

namespace LunchBag.WebPortal.Api.Models.SignalRMessages
{
    public class GoalUpdatedSignal
    {
        public string LocationId { get; set; }
        public int GoalStatus { get; set; }
    }
}
