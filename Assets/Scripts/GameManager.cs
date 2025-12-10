using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState { Start, Playing, End }
    public GameState currentState = GameState.Start;

    public Canvas startCanvas;
    public Canvas endCanvas;
    public TextMeshProUGUI startText;
    public TextMeshProUGUI endText;
    public Button startButton;
    public Button restartButton;

    public RabbitController rabbit;
    public LynxController lynx;
    public TurnManager turnManager;

    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        restartButton.onClick.AddListener(RestartGame);

        SetState(GameState.Start);
    }

    void Update()
    {
        if (currentState == GameState.Playing)
        {
            CheckGameOver();
        }
    }

    void CheckGameOver()
    {
        Vector3Int rabbitCell = rabbit.tilemap.WorldToCell(rabbit.transform.position);
        Vector3Int lynxCell = lynx.tilemap.WorldToCell(lynx.transform.position);

        if (rabbitCell == lynxCell)
        {
            SetState(GameState.End);
        }
    }

    void SetState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.Start:
                startCanvas.gameObject.SetActive(true);
                endCanvas.gameObject.SetActive(false);
                turnManager.enabled = false;
                rabbit.canHighlight = false;
                lynx.enabled = false;
                if (rabbit.highlightTilemap != null) rabbit.highlightTilemap.ClearAllTiles();
                if (lynx.highlightTilemap != null) lynx.highlightTilemap.ClearAllTiles();
                break;
            case GameState.Playing:
                startCanvas.gameObject.SetActive(false);
                endCanvas.gameObject.SetActive(false);
                turnManager.enabled = true;
                rabbit.canHighlight = true;
                lynx.enabled = true;
                break;
            case GameState.End:
                startCanvas.gameObject.SetActive(false);
                endCanvas.gameObject.SetActive(true);
                endText.text = "Game Over! The Lynx caught the Rabbit.";
                turnManager.enabled = false;
                rabbit.canHighlight = false;
                lynx.enabled = false;
                if (rabbit.highlightTilemap != null) rabbit.highlightTilemap.ClearAllTiles();
                if (lynx.highlightTilemap != null) lynx.highlightTilemap.ClearAllTiles();
                break;
        }
    }

    void StartGame()
    {
        SetState(GameState.Playing);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}