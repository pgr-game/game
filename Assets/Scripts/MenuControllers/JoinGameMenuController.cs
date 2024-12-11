using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinGameMenuController : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField privateGameJoinCode;
    private ClientGameManager gameManager;

    private void Start()
    {
        if (ClientSingleton.Instance == null) { return; }
        gameManager = ClientSingleton.Instance.Manager;
    }

    public async void JoinPrivateGame()
    {
        await ClientSingleton.Instance.Manager.BeginConnection(privateGameJoinCode.text);
    }

    public void SearchForLobbies()
    {
        SceneManager.LoadScene("LobbiesSearch", LoadSceneMode.Single);
    }
}
