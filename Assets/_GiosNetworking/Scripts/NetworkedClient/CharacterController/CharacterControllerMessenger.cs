using System;
using Mirror;

namespace ClientSidePrediction.CC
{
    public class CharacterControllerMessenger : NetworkBehaviour, INetworkedClientMessenger<CharacterControllerInput, CharacterControllerState>
    {
        public event Action<CharacterControllerInput> OnInputReceived;

        public CharacterControllerState LatestServerState => _latestServerState;

        CharacterControllerState _latestServerState;
        
        public void SendState(CharacterControllerState state)
        {
            RpcSendState(state);
        }

        public void SendInput(CharacterControllerInput input)
        {
            CmdSendInput(input);
        }
        
        [ClientRpc(channel = Channels.DefaultUnreliable)]
        void RpcSendState(CharacterControllerState state)
        {
            _latestServerState = state;
        }
        
        [Command(channel = Channels.DefaultUnreliable)]
        void CmdSendInput(CharacterControllerInput input)
        {
            OnInputReceived?.Invoke(input);
        }
    }
}