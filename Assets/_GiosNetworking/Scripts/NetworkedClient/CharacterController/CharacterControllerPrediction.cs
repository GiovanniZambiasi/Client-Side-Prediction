using System;
using ClientSidePrediction.RB;
using UnityEngine;

namespace ClientSidePrediction.CC
{
    public class CharacterControllerPrediction : ClientPrediction
    {
        protected override INetworkedClientInput GetInput(float deltaTime, uint currentTick)
        {
            var __inputs = new Vector2
            {
                x = Input.GetAxis("Horizontal"),
                y = Input.GetAxis("Vertical")
            };
            
            return new CharacterInput(__inputs, currentTick, deltaTime);
        }
    }
}