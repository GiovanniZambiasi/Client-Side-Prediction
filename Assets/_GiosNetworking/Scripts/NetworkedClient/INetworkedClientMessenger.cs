namespace ClientSidePrediction
{
    // The networking of inputs and states needs to be encapsulated in this class because Mirror doesn't support generic
    // arguments in their classes. If it did, this behaviour could be inside the implementation of NetworkedClient
    public interface INetworkedClientMessenger<TClientInput, TClientState>
        where TClientInput : INetworkedClientInput
        where TClientState : INetworkedClientState
    {
        event System.Action<TClientInput> OnInputReceived;

        TClientState LatestServerState { get; }
        
        void SendState(TClientState state);

        void SendInput(TClientInput input);
    }
}