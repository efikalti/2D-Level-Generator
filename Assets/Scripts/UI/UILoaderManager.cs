using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class UILoaderManager : MonoBehaviour
{
    private Button Next;

    private Button Previous;

    private TilemapLoader TilemapLoader;

    public void Start()
    {
        SetupTilemapLoader();
        SetupButtons();
    }

    private void SetupButtons()
    {
        var buttons = FindObjectsOfType<Button>();
        if (buttons.Length > 0)
        {
            foreach(var button in buttons)
            {
                if (button.name == "NextButton")
                {
                    Next = button;
                }
                else if (button.name == "PreviousButton")
                {
                    Previous = button;
                }
            }
        }

        if (Next != null)
        {
            Next.onClick.AddListener(NextDungeon);
        }


        if (Previous != null)
        {
            Previous.onClick.AddListener(PreviousDungeon);
        }

    }

    private void SetupTilemapLoader()
    {
        var loaders = FindObjectsOfType<TilemapLoader>();
        if (loaders.Length > 0)
        {
            TilemapLoader = loaders[0];
        }
    }

    void NextDungeon()
    {
        TilemapLoader.LoadNextTilemapFromFile();
    }

    void PreviousDungeon()
    {
        TilemapLoader.LoadPreviousTilemapFromFile();
    }
}
