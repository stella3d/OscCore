# OscCore
A performance-oriented OSC library for Unity

#### Why Another OSC Library ?

There are already at least 4 other OSC implementations for Unity:
- [OscJack](https://github.com/keijiro/OscJack), [ExtOSC](https://github.com/Iam1337/extOSC), [UnityOSC](https://github.com/jorgegarcia/UnityOSC), & [OscSimpl](https://assetstore.unity.com/packages/tools/input-management/osc-simpl-53710)

OscCore was created largely because all of these other libraries _allocate memory for each received message_, which will cause lots of garbage collections when used with a large amount of messages.

###### Because of Strings

Every OSC message starts with an "address", specified as an ascii string.  
It's perfectly reasonable to represent this address in C# as a standard `string`, which is how other libraries work.

However, because strings in C# are immutable & UTF16, every time we receive a message from the network, this now requires us to allocate a new `string` & expand the received ascii string's bytes to UTF16.   

OscCore _eliminates parsing to string_ by leveraging the [BlobHandles](https://github.com/stella3d/BlobHandles) package.  
This means no temporary memory is allocated when a message is received & routed to the appropriate method.

## Protocol Support


