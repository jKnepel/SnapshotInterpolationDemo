using System;
using System.Linq;
using System.Threading.Tasks;
using jKnepel.ProteusNet.Components;
using jKnepel.ProteusNet.Networking.Transporting;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private MonoNetworkManager manager;
    
    public string PlayerID { get; private set; }
    public string[] AllocationRegions { get; private set; }
    public string JoinCode { get; private set; }

    public async Task<bool> InitializeNetwork()
    {
        try
        {
            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogError("An error occurred trying to sign in the player. Try again later!");
                return false;
            }

            PlayerID = AuthenticationService.Instance.PlayerId;
            var regions = await RelayService.Instance.ListRegionsAsync();
            AllocationRegions = regions.Select(x => x.Id).ToArray();

            Debug.Log("The player was signed in!");
            return true;
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"Authentication failed: {e}");
            return false;
        }
        catch (RequestFailedException e)
        {
            Debug.LogError($"Unity Services request failed: {e}");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Unexpected error during initialization: {e}");
            return false;
        }
    }

    public async Task<bool> StartHost(int maxPlayers, string allocationRegion)
    {
        if (manager.Transport == null) 
            return false;

        try
        {
            var hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers, allocationRegion);
            ((UnityTransport)manager.Transport).SetRelayServerData(hostAllocation.ToRelayServerData("udp"));

            JoinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            manager.StartServer();
            manager.StartClient();
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay allocation failed: {e}");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start host: {e}");
            return false;
        }
    }

    public void StopHost()
    {
        manager.StopClient();
        manager.StopServer();
    }

    public async Task<bool> StartClient(string joinCode)
    {
        if (manager.Transport == null) 
            return false;

        if (manager.IsClient) 
            return false;

        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            ((UnityTransport)manager.Transport).SetRelayServerData(joinAllocation.ToRelayServerData("udp"));
            JoinCode = joinCode;

            manager.StartClient();
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay join failed: {e}");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"Unexpected error while starting client: {e}");
            return false;
        }
    }


    public void StopClient()
    {
        manager.StopClient();
    }
}
