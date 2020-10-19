using UnityEngine;

namespace ClientSidePrediction.RB
{
    public class RigidbodyPrediction : ClientPrediction
    {
        protected override INetworkedClientInput GetInput(float deltaTime, uint currentTick)
        {
            var __movement = new Vector2
            {
                x = Input.GetAxis("Horizontal"), 
                y = Input.GetAxis("Vertical")
            };

            return new ClientInput(__movement, deltaTime, currentTick);
        }
    }
}