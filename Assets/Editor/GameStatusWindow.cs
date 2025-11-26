using UnityEngine;
using UnityEditor;

public class GameStatusWindow : EditorWindow
{
    private TurnManager turnManager;
    private RabbitController rabbitController;

    [MenuItem("Window/Game Status")]
    static void Init()
    {
        GameStatusWindow window = (GameStatusWindow)EditorWindow.GetWindow(typeof(GameStatusWindow));
        window.Show();
    }

    void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            FindManagers();
        }
    }

    void FindManagers()
    {
        turnManager = FindAnyObjectByType<TurnManager>();
        rabbitController = FindAnyObjectByType<RabbitController>();
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Game Status", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            GUILayout.Label("Enter Play Mode to see status");
            return;
        }

        if (turnManager == null || rabbitController == null)
        {
            FindManagers();
            if (turnManager == null || rabbitController == null)
            {
                GUILayout.Label("TurnManager or RabbitController not found in scene");
                return;
            }
        }

        GUILayout.Label("Current Turn: " + turnManager.CurrentTurn);
        GUILayout.Label("Rabbit State: " + rabbitController.CurrentState);
    }
}