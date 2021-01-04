namespace ClientSidePrediction
{
    public interface INetworkedClientInput
    {
        /// <summary>
        /// The amount of time the input was recorded for 
        /// </summary>
        float DeltaTime { get; }
        
        /// <summary>
        /// The tick in which this input was sent on the client
        /// </summary>
        uint Tick { get; }
    }
}