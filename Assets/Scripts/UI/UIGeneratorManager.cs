using Assets.Scripts.Controllers.TilemapController;
using UnityEngine;
using UnityEngine.UI;

public class UIGeneratorManager : MonoBehaviour
{
    private Button StartButton;

    private TilemapControllerBase TilemapGenerator;

    public void Start()
    {
        var tilemapControllers = FindObjectsOfType<TilemapControllerBase>();
        if (tilemapControllers.Length > 0)
        {
            foreach(var controller in tilemapControllers)
            {
                if (controller.enabled)
                {
                    TilemapGenerator = controller;
                    StartButton = GetComponent<Button>();
                    StartButton.onClick.AddListener(StartGeneratingOnClick);
                    break;
                }
            }
        }
    }

    void StartGeneratingOnClick()
    {
        if (TilemapGenerator != null)
        {
            TilemapGenerator.StartGenerating();
        }
    }
}
