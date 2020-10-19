using UnityEngine;

namespace ClientSidePrediction.RB
{
    public struct ClientInput : INetworkedClientInput
    {
        public uint Tick => tick;
        
        public Vector2 movement;
        public float deltaTime;
        public uint tick;
        
        public ClientInput(Vector2 movement, float deltaTime, uint tick)
        {
            this.movement = movement;
            this.deltaTime = deltaTime;
            this.tick = tick;
        }
    }
}