﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class stageManager : MonoBehaviour
{
    public event System.Action<Stage> NewStageAction; 

    Stage CurrentStage;
    Stage NextStage;
    public int CurrentRow = 0;
    public List<Stage> Stages;
    private List<ParticleSystem> bgParticleSystems;
    private SpriteRenderer bgFader;
    TextMeshProUGUI stageText;

    public Sprite[] BlobSprites;

    private const float BG_ALPHA = 0.8f;
    private Color BG_BLUE = new Color(0.568f, 0.686f, 0.792f);
    private Color BG_PURPLE = new Color(0.874f, 0.807f, 0.890f);
    private Color BG_RED = new Color(0.901f, 0.776f, 0.756f);
    private Color BG_ORANGE = new Color(0.921f, 0.831f, 0.756f);
    private Color BG_YELLOW = new Color(0.909f, 0.921f, 0.756f);
    private Color BG_GREEN = new Color(0.756f, 0.921f, 0.8f);

    void Start(){

        this.stageText = transform.Find("CenterText").GetComponent<TextMeshProUGUI>();
        this.bgFader = GameObject.Find("BackgroundFader").GetComponent<SpriteRenderer>();


        this.bgFader.color = new Color(BG_BLUE.r, BG_BLUE.g, BG_BLUE.b, BG_ALPHA);
        
        bgParticleSystems = new List<ParticleSystem>();
        ParticleSystem[] partSyses = GameObject.FindObjectsOfType<ParticleSystem>();
        foreach (ParticleSystem ps in partSyses)
        {
            if (ps.tag != "EnemyDeathParticles") //CHANGE TO USE NEW TAG!
            {
                bgParticleSystems.Add(ps);
            }
        }

        this.Stages = new List<Stage>();
        this.Stages.Add(new Stage { StartingRow = 0, BgColor = BG_BLUE, numberOfObstacles = 0 }); //Normal
        this.Stages.Add(new Stage { StartingRow = 5, BgColor = BG_PURPLE, numberOfObstacles = 0 }); //Faster
        this.Stages.Add(new Stage { StartingRow = 10, BgColor = BG_RED, numberOfObstacles = 0 }); //Item / Faster - Red
        this.Stages.Add(new Stage { StartingRow = 15, BgColor = BG_ORANGE, numberOfObstacles = 0 }); //Faster - Orange
        this.Stages.Add(new Stage { StartingRow = 20, BgColor = BG_YELLOW, numberOfObstacles = 1 }); //1 Obstacle / row - Yellow 
        this.Stages.Add(new Stage { StartingRow = 30, BgColor = BG_GREEN, numberOfObstacles = 2 }); //2 Obstacle / row -Green
        this.Stages.Add(new Stage { StartingRow = 40, BgColor = BG_BLUE, numberOfObstacles = 3 }); //3 Obstacle / row -New Blob Sprite
        this.Stages.Add(new Stage { StartingRow = 50, BgColor = BG_PURPLE, numberOfObstacles = 4 }); //4 Obstacles
        this.Stages.Add(new Stage { StartingRow = 60, BgColor = BG_RED, numberOfObstacles = 5 }); // 5 Obstacles
        this.Stages.Add(new Stage { StartingRow = 70, BgColor = BG_ORANGE, numberOfObstacles = 6 }); // 6 Obstacles
        this.Stages.Add(new Stage { StartingRow = 80, BgColor = BG_YELLOW, numberOfObstacles = 7 }); // 7 Obstacles
        this.Stages.Add(new Stage { StartingRow = 90, BgColor = BG_GREEN, numberOfObstacles = 8 }); // 8 Obs

        foreach(Stage stg in this.Stages){
            stg.EnemySpeed = SpeedFromRowIndex(stg.StartingRow, gameManager.Instance.difficulty);
        }

        this.NextStage = this.Stages[0];
        CheckForStageIncrease(0);
    }

    static float SpeedFromRowIndex(int rowsSpawned, eDifficulty diff)
    {
        if (diff == eDifficulty.spicy)
        {
            //Linear relation until keep speed at row 20
            return 0.25f * Math.Min(rowsSpawned, 20) + 3.5f;
        }
        else //(gameManager.Instance.difficulty == eDifficulty.easy)
        {
            //Linear relation until keep speed at row 20
            return 0.15f * Math.Min(rowsSpawned, 20) + 3.5f;
        }
    }

    public float GetFirstSpeed()
    {
        return SpeedFromRowIndex(0, gameManager.Instance.difficulty);
    }

    public void CheckForStageIncrease(int rowNumber){
        if (NextStage == null) return; //No more stages past this point

        if (NextStage.StartingRow <= rowNumber){
            this.HandleStageChange(Stages.IndexOf(NextStage));
        }
        else if (NextStage.StartingRow - rowNumber < 4 && NextStage.StartingRow - rowNumber > 0)
        {
            stageText.fontSize = 120;
            stageText.text = "Next Stage in " + (NextStage.StartingRow - rowNumber) + "...";
            stageText.color = new Color(stageText.color.r, stageText.color.g, stageText.color.b, BG_ALPHA);
        }
    }

    private void Update(){
        //Fade Stage # text away over time.
        if (stageText.color.a > 0){
            stageText.color = new Color(stageText.color.r, stageText.color.g, stageText.color.b, stageText.color.a - (Time.deltaTime * 0.5f)); 
        }

        // if (Camera.main.backgroundColor != this.CurrentStage.BgColor){
        //     Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, this.CurrentStage.BgColor, Time.deltaTime);
        //     foreach(ParticleSystem ps in this.bgParticleSystems){
        //         ps.startColor = Color.Lerp(Camera.main.backgroundColor, this.CurrentStage.BgColor, Time.deltaTime);
        //     }
        // }

        if (this.bgFader.color != this.CurrentStage.BgColor){
            Color gbTransparent = new Color(this.CurrentStage.BgColor.r, this.CurrentStage.BgColor.g, this.CurrentStage.BgColor.b, 0.8f);
            this.bgFader.color = Color.Lerp( this.bgFader.color, gbTransparent, Time.deltaTime);
        }
    }

    private void HandleStageChange(int stageNumber)
    {

        stageText.fontSize = 150;
        stageText.text = "Stage " + (stageNumber + 1);
        //Opacity full
        stageText.color = new Color(stageText.color.r, stageText.color.g, stageText.color.b, 1);
        stageText.rectTransform.localScale = new Vector3(1,1,1);

        int bgSpriteIdx = 0;

        if (stageNumber > 5)
        { //Change the background sprites when past a certain stage.
            bgSpriteIdx = 1;
        }

        foreach (ParticleSystem ps in this.bgParticleSystems)
        {
            ps.textureSheetAnimation.SetSprite(0, this.BlobSprites[bgSpriteIdx]);
        }


        if (this.Stages.Count > stageNumber + 1){
            this.NextStage = this.Stages[stageNumber + 1];
        }else{
            this.NextStage = null;
        }
        this.CurrentStage = this.Stages[stageNumber];
        this.NewStageAction.Invoke(this.CurrentStage);
    }

}
