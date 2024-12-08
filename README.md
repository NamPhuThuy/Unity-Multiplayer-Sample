# Set up
## Môi trường
- **Unity Editor**: 2022.3.24f1
- Packages: 
  - **Authentication** 3.3.3, **Lobby** 1.2.2, **Netcode for GameObjects** 1.8.1, **Relay** 1.1.1, **Multiplayer Tools** 1.1.1
  - **Cinemachine** 2.9.7, **Input System** 1.7.0

## Kết nối với **Unity Service**
- Đăng nhập Unity account 
- Tạo 1 project mới trên Unity Cloud
- Kết nối local Unity project với Unity Cloud project vừa tạo: Project Settings -> Services -> Link to the <project_name> project

# Hướng dẫn
- Tải bản release để tạo nhiều instance (1 instance tạo host và các instance còn lại sẽ join)
- Host player có quyền chọn map và quyền khởi động game
- Tất cả người chơi trong lobby phải sẵn sàng thì Host-player mới bắt đầu game được

## Explain the flow
Mỗi player cần request 1 **Access Token** từ Authentication Service, **Access Token** này được dùng để sử dụng các **Unity Services**: Relay, Lobby,..  

<div style="text-align: center;">
<img src="images/multiplayer_1.png" alt="Image description">
</div>

- Một player (Host) sẽ gửi **create lobby request** tới Lobby service. **Lobby Service** sẽ kiểm tra tính xác thực **(authentication)** của player đó. Nếu player này đã được xác thực => Lobby Service sẽ tạo 1 lobby mới và gửi lại Host-player 1 **Lobby object** (có thể chứa thông tin: lobby id, lobby code, players data, lobby data,..)
<div style="text-align: center;">
<img src="images/multiplayer_2.png" alt="Image description">
</div>

- Trong quá trình chơi, người chơi sẽ không trực tiếp tương tác với nhau, mà phải truyền data tới **Lobby Service**. Ta sẽ cần yêu cầu Lobby Service làm mới Lobby sau mỗi một khoảng thời gian nhất định (thông qua **RefreshLobbyRequest**)

- Bên cạnh đó, Lobby Service sẽ có life-cycle khác so với game, Lobby service sẽ đóng sau 1 khoảng thời gian không nhận được thông tin gì từ player. Ta sẽ cần gửi đi các **HeartbeatLobbyRequest** để đảm bảo Lobby không bị dừng lại

## Một số Keywords 
### Lobby Service
**HeartbeatLobbyRequest**: 
- Mô tả: Một loại message chứa ít thông tin mà client gửi định kì tới server
- Mục đích: Cho server biết client này vẫn đang hoạt động. 
- Lợi ích:
  - **Nhận biết sự cố mất kết nối**: nếu server không nhận được 1 heartbeat nào trong khoảng thời gian nhất định, client này sẽ được coi là đã mất kết nối (có thể do vấn đề về mạng, crash,..). Sau đó, server có thể xóa client này, và cập nhật lại lobby 
  - **Duy trì session state**: Heartbeats có thể mang 1 số thông tin cơ bản về client's state (Hp, trạng thái sẵn sàng), cho phép server nắm được 1 số thông tin mà không cần phải cập nhật liên tục

<div style="text-align: center;">
<img src="images/multiplayer_3.png" alt="Image description" width="500px">
<p>HeartbeatLobbyRequest</p>
</div>

**RefreshLobbyRequest**:
- Mô tả: Một loại message clinet dùng để yêu cầu server gửi thông tin hiện tại của lobby
- Mục đích: Client cập nhật những thông tin mới nhất của lobby tại local(danh sách player, trạng thái sẵn sàng của các player, các thuộc tính của lobby,..)
- Lợi ích:
  - **Cập nhật lại lobby**: trong 1 lobby, nhiều sự kiện có thể diễn ra: các player tham gia hoặc rời lobby, player thay đổi trạng thái sẵn sàng. Tất cả player trong lobby đều phải nhận được thông tin về những sự thay đổi này


<div style="text-align: center;">
<img src="images/multiplayer_4.png" alt="Image description" width="500px">
<p>RefreshLobbyRequest</p>
</div>


### Netcode for GameObjects package
**Remote Procedure Call (RPC)**
- Mô tả: Kỹ thuật được sử dụng trong các hệ thống phân tán (vd: multiplayer game), nó cho phép một phần của hệ thống **(client/server)** thực thi một **function** hoặc **procedure** trên một phần khác **(server/client)**

<div style="text-align: center;">
<img src="images/ServerRPCs.png" alt="Image description"">
<p>Server RPCs</p>
</div>

<div style="text-align: center;">
<img src="images/ClientRPCs.png" alt="Image description"">
<p>Client RPCs</p>
</div>

### Techniques
**Client-side prediction**
- Mô tả: Client lập tức cập nhật local (bắn ra viên đạn, di chuyển nhân vật,..) dựa trên thông tin đầu vào (player nhấn nút bắn, player di chuyển,..) ngay khi gửi cho server mà không cần quan tâm server có nhận được hay không
- Mục đích:
  - **Phòng tránh hiện tượng lag**: khi client phải đợi quá lâu để server gửi thông tin xác minh rồi mới cập nhật ở local

**Server Reconciliation**
- Mô tả: Sau khi client tự cập nhật ở local, server sẽ xác minh lại dựa trên **thông tin đầu vào** nhận được, và gửi lại data về game-state cho client. 
- Mục đích:
  - **Tránh sai sót**: client có thể tính toán sai, dựa trên nhiều yếu tố (khi client A cập nhật thông tin, A chưa nhận được thông tin mới nhất từ client B) 

=> **Client-side prediction** & **Server Reconciliation**: client dự đoán (predict) để tạo cảm giác game có phản hồi tốt hơn, server điều chỉnh (reconcile) để tránh việc tính toán sai hoặc gian lận

# References
- <a href = "https://www.youtube.com/playlist?list=PLxmtWA2eKdQSf2EXE-tv0lmqmmdDzs0fV">Unity Multiplayer tutorial</a> - Carl Boisvert Dev
- <a href = "https://docs-multiplayer.unity3d.com/">Unity Multiplayer documents</a> - Unity
- <a href = "https://www.linkedin.com/pulse/multiplayer-client-side-prediction-server-demystified-zack-sinisi-p2efe/">Multiplayer Client-side Prediction and Server Reconciliation Demystified</a> - Zack Sinisi
- <a href = "https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/rpc/">RPC</a> - Unity
- [Unity Multiplayer Solutions](https://docs.google.com/spreadsheets/d/1Bj5uLdnxZYlJykBg3Qd9BNOtvE8sp1ZQ4EgX1sI0RFA/edit?gid=127892449#gid=127892449)