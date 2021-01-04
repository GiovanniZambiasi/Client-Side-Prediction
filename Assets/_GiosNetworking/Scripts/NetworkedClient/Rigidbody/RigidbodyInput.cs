using UnityEngine;

namespace ClientSidePrediction.RB
{
    public struct RigidbodyInput : INetworkedClientInput
    {
        public float DeltaTime => deltaTime;
        public uint Tick => tick;

        public Vector2 movement;
        public float deltaTime;
        public uint tick;
        
        public RigidbodyInput(Vector2 movement, float deltaTime, uint tick)
        {
            this.movement = movement;
            this.deltaTime = deltaTime;
            this.tick = tick;
        }
    }
}