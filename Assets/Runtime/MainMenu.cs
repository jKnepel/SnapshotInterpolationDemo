using System.Linq;
using System.Threading.Tasks;
using jKnepel.ProteusNet.Components;
using jKnepel.ProteusNet.Networking;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private MonoNetworkManager networkManager;
    [SerializeField] private RelayManager relayManager;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private TMP_Text errorMessage;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField userColor;
    [SerializeField] private TMP_Dropdown regionDropdown;
    [SerializeField] private TMP_InputField joinCode;
    [SerializeField] private TMP_Text joinCodeText;

    private async void Start()
    {
        username.text = networkManager.Client.Username;
        userColor.text = "#" + ColorUtility.ToHtmlStringRGB(networkManager.Client.UserColour);
        
        if (!await relayManager.InitializeNetwork())
        {
            errorMessage.text = "Could not initialize the network. Please try again later!";
            return;
        }

        networkManager.Server.OnLocalServerStarted += OnGameStarted;
        networkManager.Client.OnLocalClientStarted += OnGameStarted;
        networkManager.Server.OnLocalStateUpdated += OnGameStopped;
        networkManager.Client.OnLocalStateUpdated += OnGameStopped;
        
        errorMessage.text = string.Empty;
        regionDropdown.options = relayManager.AllocationRegions.Select(x => new TMP_Dropdown.OptionData(x)).ToList();
    }

    public void StartHost() => _ = StartHostInternal();
    private async Task StartHostInternal()
    {
        networkManager.Client.Username = username.text;
        if (ColorUtility.TryParseHtmlString(userColor.text, out Color parsedColor))
            networkManager.Client.UserColour = parsedColor;
        
        if (!await relayManager.StartHost(5, regionDropdown.options[regionDropdown.value].text))
        {
            errorMessage.text = "Could not start the host. Please try again later!";
            return;
        }
    }

    public void JoinRoom() => _ = JoinRoomInternal();
    private async Task JoinRoomInternal()
    {
        networkManager.Client.Username = username.text;
        if (ColorUtility.TryParseHtmlString(userColor.text, out Color parsedColor))
            networkManager.Client.UserColour = parsedColor;
        
        if (!await relayManager.StartClient(joinCode.text))
        {
            errorMessage.text = "Could not start the client. Please try again later!";
            return;
        }
    }

    public void ExitRoom()
    {
        if (networkManager.IsHost)
            relayManager.StopHost();
        else if (networkManager.IsClient)
            relayManager.StopClient();
    }

    private void OnGameStarted()
    {
        joinCodeText.text = $"Join Code: {relayManager.JoinCode}";
        errorMessage.text = string.Empty;
        mainMenu.SetActive(false);
    }

    private void OnGameStopped(ELocalServerConnectionState state)
    {
        if (state == ELocalServerConnectionState.Stopped)
        {
            errorMessage.text = string.Empty;
            mainMenu.SetActive(true);
        }
    }
    
    private void OnGameStopped(ELocalClientConnectionState state)
    {
        if (state == ELocalClientConnectionState.Stopped)
        {
            errorMessage.text = string.Empty;
            mainMenu.SetActive(true);
        }
    }
}
