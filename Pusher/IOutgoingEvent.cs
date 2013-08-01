namespace Pusher
{
    public interface IOutgoingEvent<T>
    {
        string EventName { get; set; }
        string Channel { get; set; }
        T Data { get; set; }
    }
}