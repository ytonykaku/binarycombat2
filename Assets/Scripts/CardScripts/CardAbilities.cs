using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public abstract class CardAbilities : NetworkBehaviour
{
    public PlayerManager PlayerManager;
    public GameManager GameManager;

    void Start()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public abstract void OnCompile();

    public abstract void OnCompile(int value);

    public abstract void OnExecute();
}
