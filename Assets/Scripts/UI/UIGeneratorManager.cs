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
            TilemapGenerator = tilemapControllers[0];
        }
        StartButton = GetComponent<Button>();
        StartButton.onClick.AddListener(StartGeneratingOnClick);
    }

    void StartGeneratingOnClick()
    {
        if (TilemapGenerator != null)
        {
            TilemapGenerator.StartGenerating();
        }
    }
}
