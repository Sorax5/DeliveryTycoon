using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void OnStartGame()
    {
        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Game, SceneDatabase.Scenes.Game)
            .Unload(SceneDatabase.Slots.UI)
            .WithOverlay()
            .Perform();
    }
}
