# Client-Side-Prediction framework
_Made with Mirror for Unity_

This repository contains a garbage free client-side prediction framework, implemented in two different example scenarios:
* CharacterController
* Rigidbodies _*To be completed_

My goal is to develop an easy-to-use, garbage free framework to streamline the implementation of client-side prediction across any scenario that needs it. Though the current solution is still a work-in-progress, it works, and will hopefully avoid the headache of adding all the boilerplate code to all predictive clients your project needs.

## How to use

The developer needs to fulfill 5 steps to implement prediction:

### 1) Implement _state_

Create your state block. This block should represent a snapshot of your client at any given time _(position, rotation, speed, for example)_. 

The state *must* implement the `INetworkedClientState` interface:
```cs
public interface INetworkedClientState : IEquatable<INetworkedClientState>
{
        /// <summary>
        /// The tick number of the last input packet processed by the server
        /// </summary>
        uint LastProcessedInputTick { get; }
}
```

The only requirement is to create a `uint` field to store the _last processed input tick_. This is important because the prediction will set the client at that state, and then reprocess all inputs from that tick till the most recent tick. It's recommended to create your state as a struct to avoid heap allocations.

Example:
```cs
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
}
```

### 2) Implement _input_

...TBD

### 3) Implement the _Messenger_

...TBD

### 4) Implement the _Client_

...TBD

### 5) Implement the _Prediction_

...TBD
