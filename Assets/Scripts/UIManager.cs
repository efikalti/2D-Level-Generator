using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text FileNameText;

    public Button Next;

    public Button Previous;

    private TilemapLoader tilemapLoader;

    public void Start()
    {
        tilemapLoader = GetComponent<TilemapLoader>();
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
        tilemapLoader.LoadNextTilemapFromFile();
    }

    public void PreviousDungeon()
    {
        tilemapLoader.LoadPreviousTilemapFromFile();
    }
}
