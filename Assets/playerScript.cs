﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class playerScript : MonoBehaviour
{

    private int myValue = 0;

    private bool myIsVulnerable = true;
    public bool invincible = false;
    public int ExtraLives = 0;
    private bool myHasExtraWeight = false;
    private bool gotLosingRoll = false;
    private float lastRollTime;

    private bool myCanWrap = false;

    DiceFace[] faces;
    private DiceFace currFace;
    public Sprite[] faceSprites;
    public Sprite vulnerableSprite;

    private SpriteRenderer spriteRend;
    private Rigidbody2D rb;
    public GameObject dustMaker;

    public AudioClip[] DiceLandNoises;
    public AudioClip[] DeathNoises;
    public ParticleSystem DeathParticles;

    public event System.Action<int> NewValueAction;
    public event System.Action<bool> GotUpgradeAction;

    public int Value
    {
        get { return myValue; }
        set
        {
            myValue = value;
            NewValueAction.Invoke(value);
        }
    }

    public bool IsVulnerable
    {
        get{ return myIsVulnerable;}
        set{
            if (value){
                spriteRend.sprite = vulnerableSprite;
            }
            myIsVulnerable = value;
        }
    }

    public bool HasExtraWeight
    {
        get { return myHasExtraWeight; }
        set
        {
            myHasExtraWeight = value;
            GotUpgradeAction.Invoke(true);
        }
    }

    public bool CanWrap
    {
        get { return myCanWrap; }
        set
        {
            myCanWrap = value;
            GotUpgradeAction.Invoke(true);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody2D>();

        this.faces = new DiceFace[7];
        for (int i = 0; i < 7; i++)
        {
            this.faces[i] = new DiceFace(i, faceSprites[i]);
        }

        this.currFace = this.faces[UnityEngine.Random.Range(1, 7)];
        this.myValue = currFace.Value;
        spriteRend = GetComponent<SpriteRenderer>();
        spriteRend.sprite = this.currFace.sprite;
    }

    // Update is called once per frame
    void HandleTouch(Vector3 vector3)
    {
        if (gameManager.Instance.IsPaused == true)
        {
            return;
        }

        //Only bottom 3/4 of screen is for movement, since the pause is up there
        if (vector3.y > Screen.height * 0.75)
        {
            return;
        }

        if (vector3.x > (Screen.width * 2 / 3))
        { //Right Third of Screen
            Roll();
        }
        else if (vector3.x < (Screen.width * 1 / 3))
        { // Left thrid of Screen
            moveLeft();
        }
        else
        {
            moveRight();
        }

    }

    void moveRight()
    {
        if (gameManager.Instance.IsPaused == true)
        {
            return;
        }
        if (CanWrap && transform.position.x > 3.4f)
        {
            transform.position = new Vector3(-3.5f, transform.position.y, 0);
        }
        else
        {
            transform.position = new Vector3(System.Math.Min(transform.position.x + 1, 3.5f), transform.position.y, 0);
        }
    }

    void moveLeft()
    {
        if (gameManager.Instance.IsPaused == true)
        {
            return;
        }
        if (CanWrap && transform.position.x < -3.4f)
        {
            transform.position = new Vector3(3.5f, transform.position.y, 0);
        }
        else
        {
            transform.position = new Vector3(System.Math.Max(transform.position.x - 1, -3.5f), transform.position.y, 0);
        }
    }

    void Roll()
    {
        if (gameManager.Instance.IsPaused == true)
        {
            return;
        }
        rb.freezeRotation = false;
        rb.AddForce(new Vector2(0f, 250));
        if (rb.mass < 1.1)
        {
            rb.AddTorque(75);
            //Only spin when at original Mass (1)
        }
        IsVulnerable = true;
    }

    void Update()
    {

        // Handle native touch events
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                this.HandleTouch(touch.position);
            }
        }

        // Simulate touch events from mouse events
        if (Input.touchCount == 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleTouch(Input.mousePosition);
            }
        }

        //Arrow controls for computers
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            moveRight();
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Roll();
            //Debug.Log("Roll Sound.");
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {

            this.invincible = !this.invincible;
            Debug.Log("Player is Invincible: " + this.invincible);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            Time.timeScale += 0.25f;
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            Time.timeScale = 1f;
        }
    }

    internal void GetKilled()
    {

        Animator ani = Camera.main.GetComponent<Animator>();
        if (!ani.GetCurrentAnimatorStateInfo(0).IsName("CameraZoom"))
        {
            ani.Play("CameraShake");
        }

        ParticleSystem ps = Instantiate(DeathParticles, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z), Quaternion.Euler(0f, 0f, 220));
        //ps.textureSheetAnimation.SetSprite(0, this.currFace.sprite);

        ParticleSystem.MainModule settings = ps.main;
        //settings.startColor = new ParticleSystem.MinMaxGradient(this.mySpriteRenderer.color);
        ps.Play();

        AudioSource.PlayClipAtPoint(this.DeathNoises[UnityEngine.Random.Range(0, this.DeathNoises.Length)], this.transform.position);
        Destroy(gameObject);
    }

    public void DecrementValue()
    {
        this.Value -= 1;
        this.spriteRend.sprite = faceSprites[this.myValue];
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Floor" && (Time.fixedTime - lastRollTime > 0.25f || lastRollTime == 0))
        {

            AudioSource.PlayClipAtPoint(this.DiceLandNoises[UnityEngine.Random.Range(0, this.DiceLandNoises.Length)], this.transform.position);

            this.IsVulnerable = false;
            lastRollTime = Time.fixedTime;
            //Debug.Log("Floor Sound.");
            //Effects
            rb.freezeRotation = true;
            rb.rotation = 0;

            Animator ani = Camera.main.GetComponent<Animator>();
            if (!ani.GetCurrentAnimatorStateInfo(0).IsName("CameraZoom"))
            {
                ani.Play("CameraShake");
            }

            GameObject dust = Instantiate<GameObject>(dustMaker, new Vector3(this.transform.position.x, this.transform.position.y - 1, this.transform.position.z), Quaternion.identity);
            Destroy(dust, 2); //get rid of the dust in 2 seconds

            //Don't allow player to roll losing roll twice in a row
            int lowerRange = 1;

            if (gotLosingRoll)
            {
                lowerRange = gameManager.Instance.weakestEnemyHP + 1;
                gotLosingRoll = false;
            }
            else if (myValue <= 1)
            {
                lowerRange = 2;
            }

            this.Value = UnityEngine.Random.Range(lowerRange, 7);

            if (this.myValue <= gameManager.Instance.weakestEnemyHP)
            {
                //take note that we gave them a losing roll
                gotLosingRoll = true;
            }

            this.currFace = faces[this.myValue];
            this.spriteRend.sprite = this.currFace.sprite;
        }
    }
}
