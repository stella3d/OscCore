# OscCore
A performance-oriented OSC library for Unity

## Why Another OSC Library ?

There are already several other OSC implementations for C# / Unity, like [OscJack](https://github.com/keijiro/OscJack) & [ExtOSC](https://github.com/Iam1337/extOSC).

OscCore was created because all of these other libraries _allocate memory for each message_, which will thrash the garbage collector when used with a large amount of messages.

##### Because of Strings

Every OSC message starts with an "address", specified as an ascii string.  
It's perfectly reasonable to represent this in C# as a standard `string`, which is how other libraries work.

However, because strings in C# are immutable, every time we receive a message from the network, this now requires us to allocate a new `string` & expand the ascii string's bytes to UTF16.   

OscCore _eliminates parsing to string_ by leveraging the [BlobHandles](https://github.com/stella3d/BlobHandles) package.  
This means no temporary memory is allocated when a message is received.


