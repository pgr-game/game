using UnityEngine;
using UnityEngine.UI;

public class FortButtonManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameManager gameManager;
    public GameObject createButton;
    public GameObject createFrame;
    public GameObject createIcon;

    public GameObject deleteButton;
    public GameObject deleteFrame;
    public GameObject deleteIcon;
    public GameObject deleteXPart1;
    public GameObject deleteXPart2;
    public void  Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
        DisableCreationButton();
        DisableeDeleteButton();
    }
    public void CreateFortButtonPress()
    {
        this.gameManager.activePlayer.ShowAvailableFortPositions();
    }

    public void DeleteFortButtonPress()
    {
        this.gameManager.activePlayer.ShowAvailableFortsForDeletion();
    }

    private void EnableCreationButton()
    {
        createButton.GetComponent<Button>().interactable = true;
        createButton.GetComponent<Image>().color = new Color32(90, 44, 21, 255);
        createFrame.GetComponent<Image>().color = new Color32(240, 166, 63, 255);
        createIcon.GetComponent<Image>().color = new Color32(243, 253, 66, 255);
    }

    private void EnableDeleteButton()
    {
        deleteButton.GetComponent<Button>().interactable = true;
        deleteButton.GetComponent<Image>().color = new Color32(90, 44, 21, 255);
        deleteFrame.GetComponent<Image>().color = new Color32(240, 166, 63, 255);
        deleteIcon.GetComponent<Image>().color = new Color32(243, 253, 66, 255);

        deleteXPart1.GetComponent<Image>().color = new Color32(253, 66, 76, 255);
        deleteXPart2.GetComponent<Image>().color = new Color32(253, 66, 76, 255);
    }

    private void DisableCreationButton()
    {
        createButton.GetComponent<Button>().interactable = false;
        createButton.GetComponent<Image>().color = new Color32(90, 44, 21, 200);
        createFrame.GetComponent<Image>().color = new Color32(240, 166, 63, 200);
        createIcon.GetComponent<Image>().color = new Color32(243, 253, 66, 200);
    }

    private void DisableeDeleteButton()
    {
        deleteButton.GetComponent<Button>().interactable = false;
        deleteButton.GetComponent<Image>().color = new Color32(90, 44, 21, 200);
        deleteFrame.GetComponent<Image>().color = new Color32(240, 166, 63, 200);
        deleteIcon.GetComponent<Image>().color = new Color32(243, 253, 66, 200);

        deleteXPart1.GetComponent<Image>().color = new Color32(253, 66, 76, 200);
        deleteXPart2.GetComponent<Image>().color = new Color32(253, 66, 76, 200);
    }
    public void CheckIfFortResearched(PlayerManager playerManager)
    {
        bool isFortReaserched = this.gameManager.playerTreeManager.isNodeResearched(1, "Strategy");
        if (isFortReaserched)
        {
            EnableCreationButton();
            EnableDeleteButton();
        }
        else
        {
            DisableCreationButton();
            DisableeDeleteButton();
        }
    }
}
