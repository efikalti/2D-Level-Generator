using Assets.Scripts.Generators;
using UnityEngine;
using UnityEngine.UI;

public class UIGeneratorManager : MonoBehaviour
{
    private Button StartButton;

    private TilemapGeneratorBase TilemapGenerator;

    public void Start()
    {
        var tilemapControllers = FindObjectsOfType<TilemapGeneratorBase>();
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
