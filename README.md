# I. Description
Freecon Galactic is a 2D, top down, space themed MMORPG, representing a collaborative ~5 year hobby effort. A primarily PVP based game, the goal of Freecon is to explore thousands of star systems, solo or in teams, colonizing, building, invading, and defending planets, while harvesting their resources to upgrade ship weapons and armor and vie for the control of slices of a massive, dynamically generated galaxy. Unfortunately, for reasons beyond the scope of this readme, development has stalled indefinitely, and this project is being hosted here temporarily as an exhibition of full stack development experience. Almost all of the code here was written by two collaborators over a ~5 year span: myself, Ilaan Shtaygrud, and Johnathan Free Wortley (@freeqaz).

# II. Recruiters
As a portfolio piece, this readme very briefly describes the major elements of the Freecon stack, as well as some of the paths to and reasoning for our design choices. Below you'll find descriptions of my experience with the client, server, networking, database, UI/graphics, and web code hosted in this repo. While this project is incomplete, there is a ton of value here, and I appreciate you taking the time to glance over it! 

# III. Code Overview
This codebase is a massive, real time, parallel, concurrent, persistent, distributed client/server architecture, spanning 50+ projects. A full description of the code and justifications of design choices, lessons learned, pitfalls, obstacles, and the like would fill the pages of a small novel. To spare you, below instead is a very brief summary of the major components of the stack and a selection of neat tricks we employed to get things running. That said, this readme is hardly a scratch in the surface of Freecon Galactic. While there have not been any releases to date, the game has been repeatedly tested by volunteers from all over the US, which is to say, it does run!

## III-1. General Architecture
This code may be generally divided into four submodules, from which are compiled the four executables that must be run to play Freecon: client, server, simulator, and web. The bulk of the codebase follows something similar to the mediator design pattern, where objects of particular classes are stored in and interacted with through managers. For example, on the server, all instances of classes derived from the base Ship class (Server.Models.PlayerShip.cs, Server.Models.NPC.cs) are stored by a single (per server process) ShipManager (Server.Managers.ShipManager.cs) instance. Among the advantages, this limited spaghettification by forcing a top down architecture, made the code easier to follow for other developers, and simplified loading from database during server initialization, nestling conveniently with our business logic. All other tasks are similarly delegated to dedicated, appropriately named managers (e.g. ConnectionManager, TeamManager, ChatManager).

To maintain readability and organization, and to avoid problems with circular references, the code has been logically divided into projects organized hierarchically, with naming conventions reflective of reference dependencies. For example, projects belonging to Core.* are referenced by both the server and the client (e.g. Core.Networking.Messages). Client.* and Server.* should be self-explanatory, and so on.

## III-2. Excerpts from the FULL STACK

### Client and Simulator
The client (Client.MonoGame.Windows) was originally built on top of Microsoft XNA, and was ported over to MonoGame after support ended. Like the server, the client organizes objects into instances of manager classes (e.g. Client.Core.Managers.ClientShipManager.cs). Business logic is represented heirarchically: near the top is an instance of GameStateManager, which swaps between different implementations of IGameState (for login, space, colony, etc.), ensuring clean swaps between different game modes. Physics based game states each have an independent instance of the PhysicsManager class, which is a lightweight wrapper for the open source physics engine Farseer, which efficiently handles collisions, object movement, and, well, physics! 

There is a centralized pub/sub message polling loop (Client.Core.MainNetworkingManager.cs), built on top of the open source Lidgren networking library, which deserializes and publishes structured server messages (Core.Networking.Messages) which are handled by subscribed IGameState callbacks as appropriate. All game states inherit from GameStateBase with overridden MessageReceived() callback methods.

Originally concieved as a means to stress test the server, the simulator takes full advantage of the client's MVVM architecture (discussed in the UI section below) and functions as a modified, specialized headless client (no view necessary!), running locally on the server. Using the INetworkingService interface, redis seamlessly drops in to replace lidgren, passing the same MessagePackSerializableObject blobs (Core.Networking.Messages.MessagePackSerializableObject.cs) directly to server nodes through dedicated pub/sub channels. With slight modifications, the simulator allows serverside physics (anti-exploit hardening) and NPC simulation essentially for free, by way of a well architected client codebase.

### Server
Built to handle thousands of simultaneous client connections, serverside operation consists of multiple instances of the server executable communicating over pub/sub channels in redis (Server.RedisWrapper). We cleverly employ business logic to balance client load: in-game, the galaxy consists of thousands of star systems (Server.Models.PSystem) between which players regularaly travel. During startup, each server is assigned a set of systems, and as clients travel between systems belonging to different servers, player and ship data is serialized to disk before clients are sent instructions to seamlessly reconnect to appropriate server instances, which then deserialize and begin handling client game data. To a player, crossing server boundaries is indistinguishable from traveling between star systems on the same server instance. By limiting the maximum number of players per system, and algorithmically adjusting the number of systems handled per server instance, we prevent server overload, dynamically adding additional instances during periods high load.

During server initialization, all server instances first check a central key-value database in redis for the existance of a master server timestamp. The first server to find that none exists automatically promotes itself to a so called master server (see project MasterServer), using redis as a central authority to prevent multiple master server instances from spawning. The master server maintains an index of existing slave server IDs, and acts as a load balancer by assigning star systems to slave servers. Slaves periodically write pings to unique ID based redis DB keys and in the case that the master server detects a late ping caused by a slave crashing, star systems (and associated player clients) are dynamically reassigned to running slave instances.

### Networking
There are two distinct forms of networking implemented in Freecon (3 if you count the web/UI RESTful API): client-server, built on top of a neat little UDP socket library called Lidgren, and server-server, powered by redis. Network message data types are fully decoupled from the transmission protocol, such that we may trivially swap between UDP and redis pub/sub depending on which element of the Freecon stack is transmitting data. This allowed us to reuse client code with minimal modification to implement a serverside "headless" client for simulating NPCs and stress testing/debugging. A ton of utility gained for free because of decoupled, modular coding practices! 

#### Client-Server
As with much of our early code, the initial approach to client-server networking was rather naive. Message creation consisted of initializing a Lidgren.NetOutgoingMessage() object, writing a Core.Networking.Objects.MessageTypes (enum, byte) header to the outgoing message, and then manually serializing each bit of relevant data, float by float, string by string, and so on. Processing consisted of the reverse – both the client and server first read the header byte from a Lidgren.NetIncomingMessage instance, and then read each field in the order that it was serialized, depending on the type of message. As we should have expected, this unstructured approach saw many hours wasted on debugging, as keeping serialization and deserialization in sync between the server and the client for dozens of message types was very difficult, especially with multiple contributors!

Eventually we swallowed a good amount of technical debt and centralized all client-server messages into one Core.Networking.Messages project, creating a unique class for each message type, all derived from an abstract MessagePackSerializableObject base class. With this nifty solution, messages are instantiated, data fields are populated, and with the MessagePackSerializableObject.Serialize() method, a library known as MessagePack automatically and efficiently serializes (and compresses) all fields, and also handles unpacking the messages when received on the other side with a simple type cast, based on the message header. This way we solved the problem of synchronizing serialization/deserialization by defining fields for each message in one place, and allowing a third party library to manage the packaging for us.

#### Server-Server
Serverside networking is facillitated entirely by pub/sub in redis. While the raw redis driver allows subscription channels to be specified by strings, the Server.RedisWrapper.RedisServer class standardizes subscription with the following two methods:  

```c#
public void Subscribe(MessageTypes channel, EventHandler<NetworkMessageContainer> callback);

public void Subscribe(ChannelTypes channelType, int channelID, EventHandler<NetworkMessageContainer> callback);
```

The first is used to subscribe to general, one to many channels, as is typical of pub/sub architecture, while the second is used to message specific servers, by specifying the server's unique Id. Under the hood, the channelType and channelID arguments are combined into strings used as unique channel specifiers. By restricting subscription with this enum based API, consumers of the redis server are not vulnerable to bugs from mispelling channel name strings, and require no knowlege as to how ChannelTypes and server Ids are combined for subscription.

### Database
Given our inexperience, at the start of the project serialization was somewhat of an afterthought. With more exciting features to work on, we made and set aside vague plans of manually reading/writing to binary blobs. Once we reached the point where lack of persistence was holding back development of other features, we quickly realized that was a terrible idea, and initially settled on a SQL relational database accessed through Microsoft's Entity Framework (EF) ORM. Here we learned another major lesson: because our codebase was not initially designed with a database in mind, the impedance mismatch made this implementation largely impractical. So we ditched EF in favor of schemaless Mongodb, expecting to just throw our unmodified classes at the Mongodb C# driver. This wasn't quite enough; while we could now trivially store our data as arbitrarily structured documents, there was still impractical amount of class specific data management that had to occur during serialization and deserialization. Plus, things got messy in frequent cases where objects were referenced by multiple other objects (e.g. one to many, many to many relationships in SQL parlance).

At this point we arrived at the current architecture, after a major refactoring effort. All of the serialized classes on the server were rearchitected into something akin to models and view models, decoupling the serializable data from the logic used to access and manipulate it. For example, each instance of the Ship base class (Server.Models.Ships.Ship.cs) has a protected reference to a ShipModel object which has no methods and is directly written to the database. Writing objects to the DB is as simple as calling Server.Mongodb.MongoDatabaseManager.SaveAsync(ISerializable obj), a method in our custom Mongodb wrapper which automatically resolves the type of the model and inserts it into the appropriate collection. This wrapper is quite elegant and I encourage you to take a look! Our current design is somewhat of a semi-schemaless hybrid, as we store different types in their own collections, indexed with UUIDs in table style, while nesting documents for one to one relationships.

### Graphics and UI
For ease of design, we drew inspiration from the MVVM model, decoupling the UI from the rest of the client with CefSharp (Client.CefSharpWrapper), a headless embedded Chromium browser which allowed us to assemble the interface in javascript, rendered as a semi-transparent overlaid bitmap. This makes it substantially easier to generate rich, dynamic user interfaces, taking advantage of modern JS libraries like React. Further, by decoupling the client view from the client model, it was trivial to create our own headless client, for serverside physics/NPC simulation, as well as client simulation for testing!

Graphics are handled by the Monogame engine, which is an almost 1:1 replacement API for the XNA library. All graphics are currently 2D sprites rendered with calls to Monogame's SpriteBatch class, with bloom and a number of custom shaders. While sprite based games have their charm, 2D graphics have significant limitations, particularly with respect to lighting. We have thus completed and tested plumbing for full 3D graphics, with plans to take advantage modern 3D rendering techniques like bump mapping, as soon as we can get our hands on some nice 3D asset models!

### Web
The Services.Web project is built with the lightweight Nancy web framework for C#, and interfaces with the UI Javascript for a subset of RESTful, asynchronous data fetching operations on the client. For example, when docking with a colony on a planet, the client switches to a static (no physics) GameStateTypes.Colony gamestate, which requests colony data like population count and resources to display on the client UI. Nancy, through redis, interfaces with the appropriate server instance, verifying client authentication keys and player state to prevent spoofing/hacking, and serves up the data directly to the waiting client UI, independent of the main C# client process. The web module is only partially implemented, but I thought it was an awesome proof of concept to include!


# IV. Licensing
All rights reserved. This code is hosted for display purposes only. No permission is granted for distribution, in original or modified form. You wouldn't download a car, would you?
