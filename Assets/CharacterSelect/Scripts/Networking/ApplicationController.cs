using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostSingleton;

    private ApplicationData appData;
    public static bool IsServer;

    private async void Start()
    {
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isServer)
    {
        appData = new ApplicationData();
        IsServer = isServer;

        if (isServer)
        {
            Debug.Log("You should not be a server");
        }
        else
        {
            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            Instantiate(hostSingleton);

            await clientSingleton.CreateClient();

            clientSingleton.Manager.ToMainMenu();
        }
    }
}
