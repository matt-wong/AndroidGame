using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class enemyScript : fallingObject
{
    
    public DiceFace currFace;
    DiceFace[] faces;

    public AudioClip[] DeathSounds;

    public Sprite[] faceSprites;
    public Sprite UnbeatableSprite;
    public ParticleSystem DeathParticles;

    public event System.Action DiedAction;

    public bool IsDisabled = false;

    private SpriteRenderer mySpriteRenderer;
    private SpriteRenderer myMatchGlowRenderer;
    private pointKeeper myPointKeeper;

    private Color MATCH_GLOW_COLOR = new Color(0.3019608f, 0.8588235f, 0.4588235f, 1); 

    public int Value() { return this.currFace.Value;}

    void Awake(){
        this.faces = new DiceFace[6];
        for (int i = 0; i < 6; i++)
        {
            this.faces[i] = new DiceFace(i + 1, faceSprites[i]);
        }

        this.currFace = this.faces[Random.Range(0,6)];
        this.mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.sprite = this.currFace.sprite;

        Transform matchIndicator = transform.Find("MatchGlow");
        if (matchIndicator){
            myMatchGlowRenderer = matchIndicator.GetComponent<SpriteRenderer>();
        }

        Animator animator = GetComponent<Animator>();
        animator.Play("DiceEnemy" + this.currFace.Value.ToString(), -1, Random.Range(0f, 1f));

        myPointKeeper = GameObject.FindObjectOfType<pointKeeper>();
     }

    public void SetAsUnbeatable(){
        this.currFace = new DiceFace(7, UnbeatableSprite);

        this.CheckColor(0); // Doesn't matter what the player has...

        //TEMP until we can get a new animation
        Animator animator = GetComponent<Animator>();
        animator.Play("DiceEnemyBlocked", -1, Random.Range(0f, 1f));
    }

    void OnTriggerEnter2D(Collider2D col)
    {

        if (col.tag == "Player" && !IsDisabled)
        {
            if (!col.gameObject.GetComponent(typeof(playerScript)).Equals(null))
            {
                playerScript playerHitScript = (playerScript)col.gameObject.GetComponent(typeof(playerScript));

                if (playerHitScript.Invincible)
                {
                    myPointKeeper.IncreasePoints(1);
                    this.GetKilled();
                }
                else if (playerHitScript.Value >= this.currFace.Value && !playerHitScript.IsVulnerable)
                {
                    if (playerHitScript.Value == this.currFace.Value)
                    {
                        myPointKeeper.IncreasePoints(1);
                        myPointKeeper.IncreaseMatchCounter();
                    }
                    else
                    {
                        myPointKeeper.DecreaseMatchMultiplier();
                        myPointKeeper.IncreasePoints(1);
                    }

                    //Decrease the players HP so they cannot stay still all day
                    this.GetKilled();
                    playerHitScript.DecrementValue();
                }

                else if (playerHitScript.ExtraLives > 0)
                {

                    //Player collected a heart make this enemy die now
                    var effects = GameObject.FindObjectOfType<EffectsMaker>();
                    if (effects)
                    {
                        effects.HeartEffect(this.transform.position);
                    }

                    playerHitScript.TakeDamage();
                    myPointKeeper.IncreasePoints(1);
                    myPointKeeper.DecreaseMatchMultiplier();
                    GetKilled();
                }

                else
                {
                    playerHitScript.GetKilled();
                }
            }
        }
        else if(col.tag == "EnemyManager"){
            //End of the page, die now
            Destroy(gameObject);
        }
    }

    internal void CheckColor(int playerValue, bool doMatchHint = false)
    {
        if (mySpriteRenderer == null){ return;}

        if (this.currFace.Value > playerValue){
            this.mySpriteRenderer.color = new Color(1f,0.1f,0.2f); //Light Red
        }else{
            if (doMatchHint && this.currFace.Value == playerValue && this.speed > 0){
                // Flash match indicator
                myMatchGlowRenderer.color = MATCH_GLOW_COLOR;
            }
            this.mySpriteRenderer.color = new Color(1f,1f,1f);
        }
    }

    public void GetKilled(float angle = 0f)
    {

        Animator ani = Camera.main.GetComponent<Animator>();
        if (!ani.GetCurrentAnimatorStateInfo(0).IsName("CameraZoom"))
        {
            ani.Play("CameraShake");
        }

        ParticleSystem ps = Instantiate(DeathParticles, new Vector3(this.transform.position.x, this.transform.position.y - 0.5f, this.transform.position.z), Quaternion.Euler(0f, 0f, angle + 40));
        //ps.textureSheetAnimation.SetSprite(0, this.currFace.sprite);

        ParticleSystem.MainModule settings = ps.main;
        //settings.startColor = new ParticleSystem.MinMaxGradient(this.mySpriteRenderer.color);
        ps.Play();

        AudioSource.PlayClipAtPoint(this.DeathSounds[Random.Range(0, this.DeathSounds.Length)], this.transform.position);

        if (DiedAction != null)
        {
            DiedAction.Invoke();
        }

        Destroy(gameObject);
    }


    internal void Disable()
    {
        IsDisabled = true;
    }

    new void Update(){
        base.Update();
        if (IsDisabled){
            // Fade away...
            mySpriteRenderer.color = new Color(mySpriteRenderer.color.r, mySpriteRenderer.color.g, mySpriteRenderer.color.b, mySpriteRenderer.color.a - (Time.deltaTime * 5));
        }
        myMatchGlowRenderer.color = new Color(myMatchGlowRenderer.color.r, myMatchGlowRenderer.color.g, myMatchGlowRenderer.color.b, myMatchGlowRenderer.color.a - (Time.deltaTime * 3));
    }
}
