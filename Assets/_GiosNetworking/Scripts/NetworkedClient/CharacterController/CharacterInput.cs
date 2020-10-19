using UnityEngine;

namespace ClientSidePrediction.CC
{
    [System.Serializable]
    public struct CharacterInput : INetworkedClientInput
    {
        public uint Tick => tick;
        
        public uint tick;
        public Vector2 input;
        public float deltaTime;

        public CharacterInput(Vector2 input, uint tick, float deltaTime)
        {
            this.input = input;
            this.tick = tick;
            this.deltaTime = deltaTime;
        }
    }
}