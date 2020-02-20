using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class UILoaderManager : MonoBehaviour
{
    private Button NextButton;

    private Button PreviousButton;

    private InputField FolderInputField;

    private TilemapLoader TilemapLoader;

    public void Start()
    {
        SetupTilemapLoader();
        SetupButtons();
        SetupInputField();
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
                    NextButton = button;
                }
                else if (button.name == "PreviousButton")
                {
                    PreviousButton = button;
                }
            }
        }

        if (NextButton != null)
        {
            NextButton.onClick.RemoveAllListeners();
            NextButton.onClick.AddListener(NextDungeon);
        }


        if (PreviousButton != null)
        {
            PreviousButton.onClick.RemoveAllListeners();
            PreviousButton.onClick.AddListener(PreviousDungeon);
        }

    }

    private void SetupInputField()
    {
        var inputFields = FindObjectsOfType<InputField>();
        if (inputFields.Length > 0)
        {
            foreach (var field in inputFields)
            {
                if (field.name == "FolderInputField")
                {
                    FolderInputField = field;
                }
            }
        }

        if (FolderInputField != null)
        {
            FolderInputField.onEndEdit.AddListener(delegate { LoadInputFiles(); });
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

    void LoadInputFiles()
    {
        Debug.Log(FolderInputField.text);
        TilemapLoader.LoadInputFiles(FolderInputField.text);
    }
}
