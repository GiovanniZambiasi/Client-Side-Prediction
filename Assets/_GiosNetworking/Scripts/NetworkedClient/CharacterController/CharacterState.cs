using System;
using UnityEngine;

namespace ClientSidePrediction.CC
{
    [System.Serializable]
    public struct CharacterState: IEquatable<CharacterState>, INetworkedClientState
    {
        public uint LastProcessedInputTick => lastProcessedInput;
        
        public Vector3 position;
        public float verticalVelocity;
        public uint lastProcessedInput;

        public CharacterState(Vector3 position, float verticalVelocity, uint lastProcessedInput)
        {
            this.position = position;
            this.verticalVelocity = verticalVelocity;
            this.lastProcessedInput = lastProcessedInput;
        }

        public bool Equals(CharacterState other)
        {
            return position.Equals(other.position) && lastProcessedInput == other.lastProcessedInput;
        }

        public bool Equals(INetworkedClientState other)
        {
            return other is CharacterState __other && Equals(__other);
        }
    }
}