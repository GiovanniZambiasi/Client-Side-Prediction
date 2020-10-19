namespace ClientSidePrediction
{
    public interface INetworkedClientInput
    {
        /// <summary>
        /// The tick in which this input was sent on the client
        /// </summary>
        uint Tick { get; }
    }
}