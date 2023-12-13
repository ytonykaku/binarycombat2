﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    public GameManager GameManager;
    public GameObject Ping;
    public GameObject Multiplier;
    public GameObject PlayerArea;
    public GameObject EnemyArea;
    public GameObject PlayerSlot1;
    public GameObject PlayerSlot2;
    public GameObject PlayerSlot3;
    public GameObject PlayerSlot4;
    public GameObject PlayerSlot5;
    public GameObject EnemySlot1;
    public GameObject EnemySlot2;
    public GameObject EnemySlot3;
    public GameObject EnemySlot4;
    public GameObject EnemySlot5;
    public GameObject PlayerYard;
    public GameObject EnemyYard;
    public List<GameObject> PlayerSockets = new List<GameObject>();
    public List<GameObject> EnemySockets = new List<GameObject>();

    private GameObject lastPlayed = null;
    public int CardsPlayed = 0;
    public int multiplierSum = 0;
    public int pingSum = 0;
    public bool IsMyTurn = false;

    private List<GameObject> cards = new List<GameObject>();

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        PlayerArea = GameObject.Find("PlayerArea");
        EnemyArea = GameObject.Find("EnemyArea");
        PlayerYard = GameObject.Find("PlayerYard");
        EnemyYard = GameObject.Find("EnemyYard");

        PlayerSlot1 = GameObject.Find("PlayerSlot1");
        PlayerSlot2 = GameObject.Find("PlayerSlot2");
        PlayerSlot3 = GameObject.Find("PlayerSlot3");
        PlayerSlot4 = GameObject.Find("PlayerSlot4");
        PlayerSlot5 = GameObject.Find("PlayerSlot5");
        EnemySlot1 = GameObject.Find("EnemySlot1");
        EnemySlot2 = GameObject.Find("EnemySlot2");
        EnemySlot3 = GameObject.Find("EnemySlot3");
        EnemySlot4 = GameObject.Find("EnemySlot4");
        EnemySlot5 = GameObject.Find("EnemySlot5");

        PlayerSockets.Add(PlayerSlot1);
        PlayerSockets.Add(PlayerSlot2);
        PlayerSockets.Add(PlayerSlot3);
        PlayerSockets.Add(PlayerSlot4);
        PlayerSockets.Add(PlayerSlot5);
        EnemySockets.Add(EnemySlot1);
        EnemySockets.Add(EnemySlot2);
        EnemySockets.Add(EnemySlot3);
        EnemySockets.Add(EnemySlot4);
        EnemySockets.Add(EnemySlot5);

        if (isClientOnly)
        {
            IsMyTurn = true;
        }
    }

    [Server]
    public override void OnStartServer()
    {
        cards.Add(Ping);
        cards.Add(Multiplier);
    }

    [Command]
    public void CmdDealCards()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject card = Instantiate(cards[Random.Range(0, cards.Count)], new Vector2(0, 0), Quaternion.identity);
            NetworkServer.Spawn(card, connectionToClient);
            RpcShowCard(card, "Dealt");
        }
        RpcGMChangeState("Compile {}");
    }

    public void PlayCard(GameObject card)
    {
        try
        {
            CardAbilities cardAbilities = card.GetComponent<CardAbilities>();

            // Verifica se a carta jogada anteriormente tem a mesma identificação
            if (lastPlayed != null && lastPlayed.GetComponent<CardAbilities>().GetType() == cardAbilities.GetType())
            {
                int currentIdentifier = GetCardIdentifier(cardAbilities);

                if (lastPlayed.GetComponent<CardAbilities>().GetType() == typeof(MultiplierAbilities) && cardAbilities.GetType() == typeof(MultiplierAbilities))
                {
                    MultiplierAbilities multiplierCard = (MultiplierAbilities)cardAbilities;
                    multiplierSum++;
                    multiplierCard.OnCompile(multiplierSum);
                }
                else if (lastPlayed.GetComponent<CardAbilities>().GetType() == typeof(PingAbilities) && cardAbilities.GetType() == typeof(PingAbilities))
                {
                    PingAbilities pingCard = (PingAbilities)cardAbilities;
                    pingSum += 250;
                    pingCard.OnCompile(pingSum);
                }
                // Adicione mais condições para outros tipos de cartas, se necessário
            }
            else
            {
                // Trata a primeira vez que a carta é jogada ou quando a carta é diferente da jogada anterior
                cardAbilities.OnCompile();
                multiplierSum = 0;
                pingSum = 0;
            }
        }
        finally
        {
            CmdPlayCard(card);
            lastPlayed = card;
        }
    }

    private int GetCardIdentifier(CardAbilities cardAbilities)
    {
        if (cardAbilities is MultiplierAbilities)
        {
            return ((MultiplierAbilities)cardAbilities).identifier;
        }
        else if (cardAbilities is PingAbilities)
        {
            return ((PingAbilities)cardAbilities).identifier;
        }
        // Adicione mais verificações para outros tipos de cartas, se necessário
        return -1; // Retorno padrão para caso não haja identificador
    }


    [Command]
    void CmdPlayCard(GameObject card)
    {
        RpcShowCard(card, "Played");
    }

    [ClientRpc]
    void RpcShowCard(GameObject card, string type)
    {
        if (type == "Dealt")
        {
            if (hasAuthority)
            {
                card.transform.SetParent(PlayerArea.transform, false);
                card.GetComponent<CardFlipper>().SetSprite("cyan");
            }
            else
            {
                card.transform.SetParent(EnemyArea.transform, false);
                card.GetComponent<CardFlipper>().SetSprite("magenta");
                card.GetComponent<CardFlipper>().Flip();
            }
        }
        else if (type == "Played")
        {
            if (CardsPlayed == 5)
            {
                CardsPlayed = 0;
            }
            if (hasAuthority)
            {
                card.transform.SetParent(PlayerSockets[CardsPlayed].transform, false);
                CmdGMCardPlayed();
            }
            if (!hasAuthority)
            {
                card.transform.SetParent(EnemySockets[CardsPlayed].transform, false);
                card.GetComponent<CardFlipper>().Flip();
            }
            CardsPlayed++;
            PlayerManager pm = NetworkClient.connection.identity.GetComponent<PlayerManager>();
            pm.IsMyTurn = !pm.IsMyTurn;
        }
    }

    [Command]
    public void CmdGMChangeState(string stateRequest)
    {
        RpcGMChangeState(stateRequest);
    }

    [ClientRpc]
    void RpcGMChangeState(string stateRequest)
    {
        GameManager.ChangeGameState(stateRequest);
        if (stateRequest == "Compile {}")
        {
            GameManager.ChangeReadyClicks();
        }
    }

    [Command]
    void CmdGMCardPlayed()
    {
        RpcGMCardPlayed();
    }

    [ClientRpc]
    void RpcGMCardPlayed()
    {
        GameManager.CardPlayed();
    }

    [Command]
    public void CmdExecute()
    {
        RpcExecute();
    }

    [ClientRpc]
    void RpcExecute()
    {
        for (int i = 0; i < PlayerSockets.Count; i++)
        {
            PlayerSockets[i].transform.GetComponentInChildren<CardAbilities>().OnExecute();
            PlayerSockets[i].transform.GetChild(0).gameObject.transform.SetParent(PlayerYard.transform, false);
            EnemySockets[i].transform.GetChild(0).gameObject.transform.SetParent(EnemyYard.transform, false);
        }
    }

    [Command]
    public void CmdGMChangeVariables(int variables)
    {
        RpcGMChangeVariables(variables);
    }

    [ClientRpc]
    public void RpcGMChangeVariables(int variables)
    {
        GameManager.ChangeVariables(variables, hasAuthority);
    }

    [Command]
    public void CmdGMChangeMultiplier(int multiplier)
    {
        RpcGMChangeMultiplier(multiplier);
    }

    [ClientRpc]
    public void RpcGMChangeMultiplier(int multiplier)
    {
        GameManager.ChangeMultiplier(multiplier, hasAuthority);
    }

    [Command]
    public void CmdGMChangeBP(int playerBP, int opponentBP)
    {
        RpcGMChangeBP(playerBP, opponentBP);
    }

    [ClientRpc]
    public void RpcGMChangeBP(int playerBP, int opponentBP)
    {
        GameManager.ChangeBP(playerBP, opponentBP, hasAuthority);
    }

    [Command]
    public void CmdGMChangeScore()
    {
        RpcGMChangeScore();
    }

    [ClientRpc]
    public void RpcGMChangeScore()
    {
        GameManager.ChangeScore(hasAuthority);
    }
}
