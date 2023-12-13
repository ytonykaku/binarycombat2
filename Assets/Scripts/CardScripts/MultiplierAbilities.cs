using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MultiplierAbilities : CardAbilities
{
    public int identifier = 1;

    public override void OnCompile()
    {
        PlayerManager.CmdGMChangeMultiplier(2);
    }

    public override void OnCompile(int sum)
    {
            PlayerManager.CmdGMChangeMultiplier(2+sum);
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
