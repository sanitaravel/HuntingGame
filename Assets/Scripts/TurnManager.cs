using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public enum Turn { Lynx, Rabbit }
    public Turn CurrentTurn { get; private set; }

    private LynxController lynxController;
    private RabbitController rabbitController;

    public void RegisterLynx(LynxController lynx)
    {
        lynxController = lynx;
    }

    public void RegisterRabbit(RabbitController rabbit)
    {
        rabbitController = rabbit;
    }

    void Start()
    {
        CurrentTurn = Turn.Lynx;
        // Start with Lynx turn
        if (lynxController != null)
            lynxController.StartTurn();
    }

    public void EndLynxTurn()
    {
        if (CurrentTurn == Turn.Lynx)
        {
            CurrentTurn = Turn.Rabbit;
            if (rabbitController != null)
                rabbitController.StartTurn();
        }
    }

    public void EndRabbitTurn()
    {
        if (CurrentTurn == Turn.Rabbit)
        {
            CurrentTurn = Turn.Lynx;
            if (lynxController != null)
                lynxController.StartTurn();
        }
    }
}