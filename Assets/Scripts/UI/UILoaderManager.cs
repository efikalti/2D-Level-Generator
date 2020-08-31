using Assets.Scripts;
using Assets.Scripts.Reporting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

public class UILoaderManager : MonoBehaviour
{
    private Button NextButton;

    private Button PreviousButton;

    private InputField FolderInputField;

    private TilemapLoader TilemapLoader;

    private Dropdown Dropdown;

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

        var dropdownObjects = FindObjectsOfType<Dropdown>();
        if (dropdownObjects.Length > 0)
        {
            foreach (var dropdown in dropdownObjects)
            {
                if (dropdown.name == "DungeonDropdown")
                {
                    Dropdown = dropdown;
                }
            }
        }

        if (Dropdown != null)
        {

            // Create GraphParser object
            var directories = new DataParser().GetAllFoldersInBasePath();
            Dropdown.AddOptions(directories.Select(x => new OptionData(x)).ToList());
            Dropdown.onValueChanged.AddListener(delegate { LoadInputFilesFromDropdown(); });
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
    void LoadInputFilesFromDropdown()
    {
        Debug.Log(Dropdown.options[Dropdown.value].text);
        if (Dropdown.value != 0)
        {
            TilemapLoader.LoadInputFiles(Dropdown.options[Dropdown.value].text);
            FolderInputField.text = Dropdown.options[Dropdown.value].text;
        }
    }
}
