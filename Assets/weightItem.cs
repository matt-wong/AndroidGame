﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weightItem : itemMovement
//Item for increase mass of player dice. This should make the rolling take less time
{

    private bool isDead; 

    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }

    protected override void TouchedPlayer(Collider2D col)
    {
        if (!isMoving) return; //Don't give the player more weight

        Rigidbody2D playerRB = col.gameObject.GetComponent<Rigidbody2D>();
        playerScript playerHitScript = (playerScript)col.gameObject.GetComponent(typeof(playerScript));
        playerHitScript.HasExtraWeight = true;
        playerRB.mass += 0.3f;

        Animator animator = GetComponent<Animator>();
        this.isMoving = false; //Stop moving, spin and shrink
        animator.Play("upgradeCollect");
        Invoke("DestroyMe", 0.3f);

    }

    private void DestroyMe(){
        Destroy(gameObject);
    }
        

}
