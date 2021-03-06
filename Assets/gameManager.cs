﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum eDifficulty{
    regular, //Slower but no unlocks
    spicy
}

[Serializable] public class SaveFile{
    public int HighScore;
    public bool MuteMusic; 
}

public class gameManager
{

    public int Points;
    public int HighScore;
    private bool myMuteAudio = false;
    public bool IsPaused = false;

    public eDifficulty difficulty = eDifficulty.spicy;

    private bool myGameOver;
    public bool GameWasWon;
    public int weakestEnemyHP;

    //0 (0 - 20 points) - No items, playter rigged to never roll losing value twice in a row.
    //1 - (20 -X points) - Add dots items to make every row winnable.

    private static gameManager instance = null;

    public static gameManager Instance
    {
        get
        {
            if (gameManager.instance == null)
            {
                gameManager.instance = new gameManager();
                gameManager.instance.Load();
            }
            return gameManager.instance;
        }
    }

    public void IncreasePoints(int value){
        this.Points += value;

        if (Points > HighScore){
            HighScore = Points;
        }

    }

    internal void Restart()
    {
        GameOver = false;
        GameWasWon = false;
        Points = 0;
        SceneManager.LoadScene("scene");
    }


    public bool GameOver
    {
        get{
            return myGameOver;
        }
        set
        {
            myGameOver = value;
            if (myGameOver){
                Save();
            }
        }
    }

    public bool MuteAudio{
        get{
            return myMuteAudio;
        }

        set{
            if (myMuteAudio != value){
                myMuteAudio = value;
                AudioListener.volume = value ? 0 : 1;
                Debug.Log("SAVING");
                Save();
            }
        }
    }

    public void Save()
    {
        try{
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

            SaveFile sf = new SaveFile();
            sf.HighScore = this.HighScore;
            sf.MuteMusic= this.MuteAudio;

            bf.Serialize(file, sf);
            file.Close();
        }catch(Exception e){
            Debug.Log(e);
        }

    }
    public void Load()
    {
        try{
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
            SaveFile loadedFile = (SaveFile)bf.Deserialize(file);
            file.Close();

            HighScore = loadedFile.HighScore;
            MuteAudio = loadedFile.MuteMusic;

            Debug.Log("Loaded Save file");

        }
        }catch(Exception e){
            HighScore = 0;
            MuteAudio = false;
            Debug.Log("Failed to load");
        }

    }
}
