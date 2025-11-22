using System;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    #region singleton
    public static ApplicationManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    private void Start()
    {

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.UI, SceneDatabase.Scenes.MainMenu)
            .WithOverlay()
            .Perform();
    }
}
