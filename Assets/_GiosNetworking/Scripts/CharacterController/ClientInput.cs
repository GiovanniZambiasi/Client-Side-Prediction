using UnityEngine;

namespace ClientSidePrediction.CC
{
    [System.Serializable]
    public struct ClientInput
    {
        public uint tick;
        public Vector2 input;
        public float deltaTime;

        public ClientInput(Vector2 input, uint tick, float deltaTime)
        {
            this.input = input;
            this.tick = tick;
            this.deltaTime = deltaTime;
        }
    }
}