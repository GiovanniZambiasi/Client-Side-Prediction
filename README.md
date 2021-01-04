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
<br><br>
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
<br><br>
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

Create your input block. This block should contain:
 * Information about your input in a given tick
 * The amount of time the input is meant for _(delta time)_
 
Similarly to the _state_ block, it's recommended to use structs for your _input_ to avoid heap allocations.
<br><br>
The _input_ must implement `INetworkedClientInput`
```cs
public interface INetworkedClientInput
{
        /// <summary>
        /// The amount of time the input was recorded for 
        /// </summary>
        float DeltaTime { get; }
        
        /// <summary>
        /// The tick in which this input was sent on the client
        /// </summary>
        uint Tick { get; }
}
```

The two required fields are:
 * `DeltaTime`: the amount of time that has passed between ticks when this input state was recorded. This is necessary because the client's application can be running at a framerate lower than the target framerate.
 * `Tick`: what the current tick number is. This is used by the server while sending states to the clients. The state will be _stamped_ with the Tick number of the last processed input block. The client will then use the _stamp_ to determine which input ticks to predict.
 
 Both fields will be supplied by the `NetworkedClient` when recording state.
 <br><br>
 Example:
 ```cs
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
 ```

### 3) Implement the _Messenger_

The _Messenger_ is a component that's really a workaround. Because Mirror doesn't support generic arguments in a _NetworkBehaviour_, the messaging part of the `NetworkedClient` needs to be encapsulated in a separate component.

The _Messenger_ is an interface:
```cs
public interface INetworkedClientMessenger<TClientInput, TClientState>
        where TClientInput : INetworkedClientInput
        where TClientState : INetworkedClientState
{
        event System.Action<TClientInput> OnInputReceived;

        TClientState LatestServerState { get; }
        
        void SendState(TClientState state);

        void SendInput(TClientInput input);
}
```
<br><br>
To implement it, the developer needs to create a `NetworkBehaviour` that implements the interface, using their _state_ and _input_ blocks as the `TCLientInput` and `TClientState` generic parameters, respectivelly:
```cs
public class NetworkedRigidbodyMessenger : NetworkBehaviour, INetworkedClientMessenger<RigidbodyInput, RigidbodyState>
```
<br><br>
The `SendState` method should call an _Rpc_ and send the provided _state_ to the clients:
```cs
public void SendState(RigidbodyState state)
{
            RpcSendState(state);
}

[ClientRpc(channel = Channels.DefaultUnreliable)]       // It's recommended to use the unreliable channel, since this message will be sent frequently
void RpcSendState(RigidbodyState state)
{
            _latestServerState = state;
}
```
<br><br>
The `SendInput` method should call a _Cmd_ and send the provided _input_ to the server:
```cs
public void SendInput(RigidbodyInput input)
{
            CmdSendInput(input);
}

[Command(channel = Channels.DefaultUnreliable)]       // It's recommended to use the unreliable channel, since this message will be sent frequently
void CmdSendInput(RigidbodyInput state)
{
            //...
}
```
<br><br>
Lastly, the _Messenger_ *must* implement an ``Action`` of the input type, and invoke it whenever an _input_ message is received:
```cs
public void SendInput(RigidbodyInput input)
{
            CmdSendInput(input);
}

[Command(channel = Channels.DefaultUnreliable)]
void CmdSendInput(RigidbodyInput state)
{
            OnInputReceived?.Invoke(state);
}
```

### 4) Implement the _Client_

The _Client_ is an ``abstract class`` that the developer needs to inherit from. It'll be the main part of your prediction. The _Client_ is responsible for controlling the ticks, recording the _state_ and processing the _inputs_. It'll also reference the _Messenger_ in order to send the networked messages.

```cs
public abstract class NetworkedClient<TClientInput, TClientState> : MonoBehaviour, INetworkedClient 
        where TClientInput : INetworkedClientInput
        where TClientState : INetworkedClientState 
```
<br><br>
To implement the _Client_, the developer needs to inherit from ``NetworkedClient``, using their _state_ and _input_ blocks as the `TCLientInput` and `TClientState` generic parameters, respectivelly:

```cs
public class NetworkedRigidbody : NetworkedClient<RigidbodyInput, RigidbodyState>
```
<br><br>
The _Client_ has 3 abstract methods that need to be implemented:
 __a)__ ``void SetState(TClientState)``: Responsible for setting the object to a particular tick:
 ```cs
 public override void SetState(RigidbodyState state)
{
            _rigidbody.position = state.position;
            _rigidbody.velocity = state.velocity;
            _rigidbody.rotation = state.rotation;
            _rigidbody.angularVelocity = state.angularVelocity;
}
 ```
 <br><br>
 __b)__ ``void ProcessInput(TClientInput)``: Responsible for applying the _input_ to the client for an amount of time _(``DeltaTime`` provided in the input block)_:
 ```cs
 public override void ProcessInput(RigidbodyInput input)
{
            var __force = new Vector3(input.movement.x, 0f, input.movement.y);
            __force *= _speed * input.deltaTime;
            _rigidbody.AddForce(__force, ForceMode.Impulse);
            
            _physicsScene.Simulate(input.deltaTime);
}
 ```
 <br><br>
 __c)__ ``TClientState RecordState``: Responsible for creating the _state_ block at the current time:
 ```cs
protected override RigidbodyState RecordState(uint lastProcessedInputTick)
{
            return new RigidbodyState(_rigidbody.position, _rigidbody.velocity, _rigidbody.angularVelocity, _rigidbody.rotation, lastProcessedInputTick);
}
 ```

### 5) Implement the _Prediction_

The _prediction_ is an abstract class that the developer needs to inherit from. It'll be responsible for the local client's behaviour. It does two things:
 * Record the _input_ block
 * _Predict_ the outcome of said _input_
 
 ```cs
 public abstract class ClientPrediction<TClientInput, TClientState> : MonoBehaviour 
        where TClientInput : INetworkedClientInput
        where TClientState : INetworkedClientState
 ```
 
 <br><br>
 To implement, the developer needs to inherit from ``ClientPrediction``, using their _state_ and _input_ blocks as the `TCLientInput` and `TClientState` generic parameters, respectivelly:
 ```cs
 public class RigidbodyPrediction : ClientPrediction<RigidbodyInput, RigidbodyState>
 ```
<br><br>
The only method that needs to be implemented is `TCLientInput GetInput(float deltaTime, uint currentTick);`. The developer will be responsible for storing the `deltaTime` and `currentTick` values in the _input_ block:
```cs
protected override RigidbodyInput GetInput(float deltaTime, uint currentTick)
{
            var __movement = new Vector2
            {
                x = Input.GetAxis("Horizontal"), 
                y = Input.GetAxis("Vertical")
            };

            return new RigidbodyInput(__movement, deltaTime, currentTick);
}
```
<br>

At every tick, the component will check if any new states have been received by the server. If so, the client will be set to that new state, and all recorded input packets from that point onwards will be re-simulated. This fulfills the _client-side prediction_.

### Finally

...TBD
