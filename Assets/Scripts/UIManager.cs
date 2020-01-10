using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text FileNameText;

    public Button Next;

    public Button Previous;

    private TilemapManager tilemapManager;

    public void Start()
    {
        tilemapManager = GetComponent<TilemapManager>();
    }

    public void SetFilenameText(string name)
    {
        if (FileNameText != null)
        {
            FileNameText.text = " File: " + name;
        }
    }

    public void NextDungeon()
    {
        tilemapManager.LoadNextTilemapFromFile();
    }

    public void PreviousDungeon()
    {
        tilemapManager.LoadPreviousTilemapFromFile();
    }
}
