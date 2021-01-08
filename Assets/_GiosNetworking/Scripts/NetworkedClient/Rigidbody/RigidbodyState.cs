using System;
using UnityEngine;

namespace ClientSidePrediction.RB
{
    public struct RigidbodyState : IEquatable<RigidbodyState>, INetworkedClientState
    {
        public uint LastProcessedInputTick => lastProcessedInputTick;
        
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public Quaternion rotation;
        public uint lastProcessedInputTick;

        public RigidbodyState(Vector3 position, Vector3 velocity, Vector3 angularVelocity, Quaternion rotation, uint lastProcessedInputTick)
        {
            this.position = position;
            this.velocity = velocity;
            this.angularVelocity = angularVelocity;
            this.rotation = rotation;
            this.lastProcessedInputTick = lastProcessedInputTick;
        }

        public bool Equals(RigidbodyState other)
        {
            return position.Equals(other.position) && velocity.Equals(other.velocity) && 
                   angularVelocity.Equals(other.angularVelocity) && rotation.Equals(other.rotation);
        }

        public bool Equals(INetworkedClientState other)
        {
            return other is RigidbodyState __other && Equals(__other);
        }

        public override string ToString()
        {
            return
                $"Pos: {position.ToString()} | Vel: {velocity.ToString()} | Angular Vel: {angularVelocity.ToString()} | Rot: {rotation.ToString()} | LasProcessInput: {lastProcessedInputTick.ToString()}";
        }
    }
}