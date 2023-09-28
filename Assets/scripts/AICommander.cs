using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICommander : Commander
{
    private GameObject player;

   private void Start()
   {
        var players = GameObject.FindGameObjectsWithTag("Player");
        if (players != null) {
            player = players[0];
        }
   }

    protected override void LateUpdate()
    {
        if (player != null) {
            Aim(player.transform.position);
        }
    }
}
