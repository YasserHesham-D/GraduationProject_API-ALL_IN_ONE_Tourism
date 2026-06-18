SignalR Chat Hub Usage

Hub endpoint: /hubs/chat
Hub methods (server):
- JoinGroup(string groupName)
- LeaveGroup(string groupName)
- SendMessageToUser(string targetUserId, SendMessageRequest message)
- SendMessageToGroup(string groupName, SendMessageRequest message)

Client receives:
- ReceiveMessage(ChatMessageDto message)
- ReceiveGroupMessage(ChatMessageDto message)

Authentication
- The hub is authorized. Clients must send the JWT access token when connecting.
- For JS clients, use withUrl('/hubs/chat', { accessTokenFactory: () => token })
- For Flutter, set accessTokenFactory in the SignalR client config; include the JWT.

Flutter example (pseudo-code using signalr_core package):

import 'package:signalr_core/signalr_core.dart';

final hubUrl = 'https://api.example.com/hubs/chat';
final token = await getJwtToken();

final connection = HubConnectionBuilder()
  .withUrl(hubUrl, HttpConnectionOptions(accessTokenFactory: () async => token))
  .build();

connection.on('ReceiveMessage', (args) {
  final message = args?[0];
  // handle message
});

await connection.start();
await connection.invoke('JoinGroup', args: ['guide-123']);
await connection.invoke('SendMessageToUser', args: ['customer-456', {'text':'Hello'}]);

JS example (browser):

const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/chat', { accessTokenFactory: () => localStorage.getItem('jwt') })
  .build();

connection.on('ReceiveMessage', (message) => console.log('DM', message));
connection.on('ReceiveGroupMessage', (message) => console.log('Group', message));

await connection.start();
await connection.invoke('JoinGroup', 'guide-123');
await connection.invoke('SendMessageToUser', 'customer-456', { text: 'Hello' });

Notes:
- Clients must use the user ID expected by the hub (ClaimTypes.NameIdentifier). If your JWT uses 'sub' or another claim, ensure the server maps it correctly.
- Consider adding presence tracking (OnConnectedAsync stores mapping of userId -> connectionId), especially for multi-connection users.
- To persist chat messages, queries should use the ChatMessages DbSet; the Hub currently does not save messages automatically.
