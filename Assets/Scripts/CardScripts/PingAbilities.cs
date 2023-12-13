using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PingAbilities : CardAbilities
{
    public int identifier = 0;

    public override void OnCompile()
    {
        PlayerManager.CmdGMChangeVariables(500);
    }

    public override void OnCompile(int pingSum)
    {
        PlayerManager.CmdGMChangeVariables(500 + pingSum);
    }

    public override void OnExecute()
    {
        PlayerManager.CmdGMChangeScore();
        if (GameManager.PlayerScore > GameManager.OpponentScore)
        {
            PlayerManager.CmdGMChangeBP(0, 1);
        }
        else
        {
            PlayerManager.CmdGMChangeBP(1, 0);
        }
    }
}
