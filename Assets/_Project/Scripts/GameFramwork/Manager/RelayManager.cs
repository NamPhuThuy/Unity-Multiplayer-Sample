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
        
        /// <summary>
        /// Create a Relay connection
        /// </summary>
        /// <param name="maxConnection"> max number of players</param>
        /// <returns>a join code</returns>
        public async Task<string> CreateRelay(int maxConnection)
        {
            /*
             - CreateAllocationAsync(maxConnection): makes an asynchronous request to the relay service provider to allocate resources for a new relay connection. The maxConnection parameter is passed to the provider.

            - Allocation allocation: The result of the allocation request. This object contains information about the allocated relay connection, including connection details and an allocation ID.
            */
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
            
            /*
             - GetJoinCodeAsync(allocation.AllocationId): After the allocation is successful, this method retrieves the unique join code associated with the newly created relay connection.
            */
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            /*
             - allocation.ServerEndpoints: The Allocation object contains a collection of endpoints representing how to connect to the relay server.

            - .First(conn => conn.ConnectionType == "dtls"): This uses LINQ to select the first endpoint where the ConnectionType is "dtls" (Datagram Transport Layer Security). DTLS is a secure protocol often used for real-time communication over unreliable networks.
            */
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