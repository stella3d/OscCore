# OscCore
A **performance-oriented** OSC library for Unity

#### Why Another OSC Library ?

There are already at least 4 other OSC implementations for Unity:
- [OscJack](https://github.com/keijiro/OscJack), [ExtOSC](https://github.com/Iam1337/extOSC), [UnityOSC](https://github.com/jorgegarcia/UnityOSC), & [OscSimpl](https://assetstore.unity.com/packages/tools/input-management/osc-simpl-53710)

OscCore was created largely because all of these other libraries _allocate memory for each received message_, which will cause lots of garbage collections when used with a large amount of messages.  For more on this see [performance details](#performance-details)

## Installation

Download & import the .unitypackage for your platform from the [Releases](https://github.com/stella3d/OscCore/releases) page.

Proper support for the [Unity package manager](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@1.8/manual/index.html) will come once I also have packages setup for the dependencies.


### Performance Details

###### Strings and Addresses

Every OSC message starts with an "address", specified as an ascii string.  
It's perfectly reasonable to represent this address in C# as a standard `string`, which is how other libraries work.

However, because strings in C# are immutable & UTF16, every time we receive a message from the network, this now requires us to allocate a new `string`, and in the process expand the received ascii string's bytes (where each character is a single byte) to UTF16 (each character is two bytes).   

OscCore eliminates both 
- string allocation
- the need to convert ascii bytes to UTF16

This works through leveraging the [BlobHandles](https://github.com/stella3d/BlobHandles) package.  
Incoming message's addresses are matched directly against their unmanaged ascii bytes.  

This has two benefits 
- no memory is allocated when a message is received
- takes less CPU time to parse a message

### Protocol Support

All [OSC 1.0 types](http://opensoundcontrol.org/spec-1_0), required and non-standard are supported.  
The spec is somewhat unclear about how array / list tags are supposed to work, however.  


