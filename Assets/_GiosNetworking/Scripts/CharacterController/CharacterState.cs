using System;
using UnityEngine;

namespace ClientSidePrediction.CC
{
    [System.Serializable]
    public struct CharacterState: IEquatable<CharacterState>
    {
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

        public override bool Equals(object obj)
        {
            return obj is CharacterState other && Equals(other);
        }
    }
}