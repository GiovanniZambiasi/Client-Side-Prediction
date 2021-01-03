using ClientSidePrediction.RB;
using UnityEngine;

namespace ClientSidePrediction.CC
{
    public class CharacterControllerPrediction : ClientPrediction<CharacterControllerInput, CharacterControllerState> 
    {
        protected override CharacterControllerInput GetInput(float deltaTime, uint currentTick)
        {
            var __inputs = new Vector2
            {
                x = Input.GetAxis("Horizontal"),
                y = Input.GetAxis("Vertical")
            };
            
            return new CharacterControllerInput(__inputs, currentTick, deltaTime);
        }
    }
}