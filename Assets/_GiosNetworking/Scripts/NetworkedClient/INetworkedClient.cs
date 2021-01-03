namespace ClientSidePrediction
{
    public interface INetworkedClient
    {
        INetworkedClientState LatestServerState { get; }
        uint CurrentTick { get; }
    }
}