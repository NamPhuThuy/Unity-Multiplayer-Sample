# Unity-Multiplayer-with-Netcode-for-GameObjects
This is a sample project of using Netcode for GameObject to create a multiplayer game


## 1: Create and Join Lobby
- Step 1: Download packages
  - Authentication, Lobby, Netcode for GameObjects

- Step 2: Connect to **Unity Service**
  - Log in Unity account 
  - Create new project on Unity Cloud with name: **Unity-Multiplayer-with-Netcode-for-GameObjects**
  - Project Settings -> Services -> Link to the **Unity-Multiplayer-with-Netcode-for-GameObjects** project

### Explain the flow
Each player may request an **Access Token** from the Authentication Service, this **Access Token** is used for many Unity Services: Relay, Lobby,..  
![](images/multiplayer_1.png)

- One player (the Host) will call the Lobby Service to make a **create lobby request**. Lobby service will check if the current player is authenticated or not, if player is authenticated => Lobby service will create a new lobby and send back a **Lobby object** to the player (the Host) (it may contains: lobby id, lobby code, players data, lobby data)

- Trong quá trình chơi, người chơi sẽ không trực tiếp tương tác với nhau, mà phải truyền data thông qua Lobby Service. Ta sẽ cần yêu cầu Lobby Service làm mới Lobby sau mỗi một khoảng thời gian nhất định

- Lobby Service sẽ có life-cycle khác so với game, Lobby service sẽ đóng sau 1 khoảng thời gian không nhận được thông tin gì từ người chơi. Ta sẽ cần gửi đi các HeartBeatRequest để đảm bảo Lobby không bị dừng lại 
![](images/multiplayer_2.png)

Ta sẽ cần triển khai một Coroutine để gửi đi các **HeartBeatRequest** theo thời gian   
![](images/multiplayer_3.png)

Ta cũng sẽ cần triển khai Coroutine để gửi đi yêu cầu cập nhật sau mỗi khoảng thời gian nhất định   
![](images/multiplayer_4.png)

## 2: Sync Lobby Data


# References

<a href = "https://www.youtube.com/playlist?list=PLxmtWA2eKdQSf2EXE-tv0lmqmmdDzs0fV">Unity Multiplayer tutorial</a> - Carl Boisvert Dev