# Chatrooms

## TODO:
1. Add a listener on the client's side of things (another console to show all messages)
2. RoomServer will send all messages to all clients (save addresses to all active listeners - in addition to client names or as a different list)
3. Give the different processes' windows different names + add a message in the beginning indicating what process this is
3. Test clients on different machines
4. Give RoomServer a way to save all user names, thus knowing all of the connected users
5. Add way to tell RoomServer when a user leaves it
6. RoomServer will shut itself down after not getting a message for 5 minutes, given that no clients are connected to it
