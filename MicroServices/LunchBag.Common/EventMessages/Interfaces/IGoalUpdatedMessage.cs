namespace LunchBag.Common.EventMessages
{
    public interface IGoalUpdatedMessage : IMessage
    {
        string LocationId { get; set; }
        int GoalStatus { get; set; }
    }
}
