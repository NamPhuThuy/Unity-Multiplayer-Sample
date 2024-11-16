using System.Linq;
using System.Threading.Tasks;
using GameFramework.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace GameFramwork.Manager
{
    public class RelayManager : Singleton<RelayManager>
    {
        private string _joinCode;
        private string _ip;
        private int _port;
        private byte[] _connectionData;
        private System.Guid _allocationId;
        
        //GETTERS
        public string GetConnectionData()
        {
            return _connectionData.ToString();
        }

        public string GetAllocationId()
        {
            return _allocationId.ToString();
        }
        
        public async Task<string> CreateRelay(int maxConnection)
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            //Using DTLS protocol
            RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(conn => conn.ConnectionType == "dtls");

            _ip = dtlsEndpoint.Host;
            _port = dtlsEndpoint.Port;

            _allocationId = allocation.AllocationId;
            _connectionData = allocation.ConnectionData;

            return _joinCode;
        }

        public async Task<bool> JoinRelay(string joinCode)
        {
            _joinCode = joinCode;
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            
            //Using DTLS protocol
            RelayServerEndpoint dtlsEndpoint = allocation.ServerEndpoints.First(conn => conn.ConnectionType == "dtls");

            _ip = dtlsEndpoint.Host;
            _port = dtlsEndpoint.Port;

            _allocationId = allocation.AllocationId;
            _connectionData = allocation.ConnectionData;

            return true;
        }


       
    }
}