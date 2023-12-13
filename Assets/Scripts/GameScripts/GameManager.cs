using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    public UIManager UIManager;
    public int TurnOrder = 0;
    public string GameState = "Initialize {}";
    public int PlayerBP = 0;
    public int OpponentBP = 0;
    public int PlayerScore = 0;
    public int OpponentScore = 0;
    public int PlayerVariables = 0;
    public int OpponentVariables = 0;
    public int PlayerMultiplier = 0;
    public int OpponentMultiplier = 0;
    public bool first = false;

    public PlayerManager PlayerManager;

    private int ReadyClicks = 0;

    void Start()
    {
        UIManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();
        UIManager.UpdatePlayerText();
        UIManager.UpdateButtonText(GameState);
    }

    public void ChangeGameState(string stateRequest)
    {
        if (stateRequest == "Initialize {}")
        {
            ReadyClicks = 0;
            GameState = "Initialize {}";
            if (first)
            {
                UpdateBP();
            }
            first = true;
        }
        else if (stateRequest == "Compile {}")
        {
            PlayerScore = 0;
            OpponentScore = 0;
            PlayerVariables = 0;
            OpponentVariables = 0;
            PlayerMultiplier = 0;
            OpponentMultiplier = 0;
            if (ReadyClicks == 1)
            {
                GameState = "Compile {}";
                UIManager.HighlightTurn(TurnOrder);
            }
        }
        else if (stateRequest == "Execute {}")
        {
            GameState = "Execute {}";
            TurnOrder = 0;
        }
        UIManager.UpdateButtonText(GameState);
    }

    public void ChangeReadyClicks()
    {
        ReadyClicks++;
    }

    public void CardPlayed()
    {
        TurnOrder++;
        UIManager.HighlightTurn(TurnOrder);
        if (TurnOrder == 10)
        {
            ChangeGameState("Execute {}");
        }
    }

    public void ChangeBP (int playerBP, int opponentBP, bool hasAuthority)
    {
        if (hasAuthority)
        {
            PlayerBP += playerBP;
        }
        else
        {
            OpponentBP += opponentBP;
        }
        UIManager.UpdatePlayerText();
    }

    public void ChangeVariables (int variables, bool hasAuthority)
    {
        if (hasAuthority)
        {
            PlayerVariables += variables;
        }
        else
        {
            OpponentVariables += variables;
        }
        UIManager.UpdatePlayerText();
    }

    public void ChangeMultiplier(int multiplier, bool hasAuthority)
    {
        if (hasAuthority)
        {
            PlayerMultiplier += multiplier;
        }
        else
        {
            OpponentMultiplier += multiplier;
        }

        UIManager.UpdatePlayerText();
    }

    public void ChangeScore(bool hasAuthority)
    {
        OpponentScore = OpponentMultiplier * OpponentVariables;
        PlayerScore = PlayerMultiplier * PlayerVariables;
        UIManager.UpdatePlayerText();
    }

    private void UpdateBP()
    {
        if (PlayerScore > OpponentScore)
        {
            PlayerManager.CmdGMChangeBP(1, 0);
        }
        else if (PlayerScore < OpponentScore)
        {
            PlayerManager.CmdGMChangeBP(0, 1);
        }
    }
}
