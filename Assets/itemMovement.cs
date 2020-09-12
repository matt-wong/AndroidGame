﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class itemMovement : MonoBehaviour
{
    public float CenterY = -3.5f;
    public float Amplitude = 1f;
    
    protected bool isMoving = true; 
    public float horizSpeed = 1f;
    //private Rigidbody2D rb;

    private float lastBump = 0f;
    protected float myStartingTime = 0f;
    public float timeToLive = -1f;
    public float fadeTime = 5f;
    public string fadeAnimationName;
    private bool isFading = false;

    protected Animator myAnimator;

    // Start is called before the first frame update
    void Start()
    {
        this.myAnimator = GetComponent<Animator>();
        myStartingTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

        if (this.isMoving)
        {
            Vector3 newPos = transform.position;
            newPos.z = 0;
            newPos.y = Amplitude * Mathf.Cos(Time.time - myStartingTime) + CenterY;
            newPos.x += horizSpeed * Time.deltaTime;
            transform.position = newPos;
        }

        if (this.timeToLive > 0f && Time.time - myStartingTime > this.timeToLive)
        {
            Destroy(gameObject);
        }
        else if (this.timeToLive > 0f && Time.time - myStartingTime > (this.timeToLive - this.fadeTime)){
            if (!this.isFading)
            {
                {
                    this.myAnimator.Play(this.fadeAnimationName);
                    this.isFading = true;
                }
            }
        }
    }

    protected abstract void TouchedPlayer(Collider2D col);

    protected void OnTriggerEnter2D(Collider2D col)
    {

        if (col.tag == "Bumper" && Time.time - lastBump > 0.05f)
        {
            lastBump = Time.time;
            horizSpeed = horizSpeed * -1;
        }
        else if(col.tag == "Player"){
            this.TouchedPlayer(col);
        }
    }

}
