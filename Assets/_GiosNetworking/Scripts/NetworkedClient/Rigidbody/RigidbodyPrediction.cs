using UnityEngine;

namespace ClientSidePrediction.RB
{
    public class RigidbodyPrediction : ClientPrediction<RigidbodyInput, RigidbodyState>
    {
        protected override RigidbodyInput GetInput(float deltaTime, uint currentTick)
        {
            var __movement = new Vector2
            {
                x = Input.GetAxis("Horizontal"), 
                y = Input.GetAxis("Vertical")
            };

            return new RigidbodyInput(__movement, deltaTime, currentTick);
        }
    }
}