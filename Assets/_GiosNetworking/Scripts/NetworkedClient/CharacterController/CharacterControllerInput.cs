using UnityEngine;

namespace ClientSidePrediction.CC
{
    [System.Serializable]
    public struct CharacterControllerInput : INetworkedClientInput
    {
        public uint Tick => tick;
        
        public uint tick;
        public Vector2 input;
        public float deltaTime;

        public CharacterControllerInput(Vector2 input, uint tick, float deltaTime)
        {
            this.input = input;
            this.tick = tick;
            this.deltaTime = deltaTime;
        }
    }
}