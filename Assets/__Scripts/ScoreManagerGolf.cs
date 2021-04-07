using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eScoreEventGolf { 
    draw,
    putt,
    gameWin,
    gameLoss
}
public class ScoreManagerGolf : MonoBehaviour {
    static private ScoreManagerGolf S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;
    static public int ROUND = 0;

    [Header("Set Dynamically")]
    public int chain = 0;
    public int scoreRun = 19;
    public int score = 19;

    void Awake() { 
        if (S == null) {
            S = this;
        } else {
            Debug.LogError("ERROR: ScoreManager.Awake(): S is already set!");
        }
        if (PlayerPrefs.HasKey ("GolfHighScore")) {
            HIGH_SCORE = PlayerPrefs.GetInt("GolfHighScore");
        }
        score += SCORE_FROM_PREV_ROUND;
        SCORE_FROM_PREV_ROUND = 0;
    }

    static public void EVENT(eScoreEventGolf evt) { 
        try {
            S.Event(evt);
        }
        catch (System.NullReferenceException nre) {
            Debug.LogError("ScoreManager:EVENT() called while S=null. \n" + nre);
        }
    }

    void Event (eScoreEventGolf evt) { 
        switch (evt) {
            case eScoreEventGolf.draw:
                scoreRun += 1;
                break; 

            case eScoreEventGolf.putt:
                scoreRun -= 1;
                break;
            
            case eScoreEventGolf.gameWin:
            case eScoreEventGolf.gameLoss:
                score += scoreRun;
                scoreRun = 0;
                ROUND += 1;
                break;
        }
        switch (evt) {
            case eScoreEventGolf.gameWin:
                if (ROUND < 9) {
                    SCORE_FROM_PREV_ROUND = score;
                }
                print("You won this round! Round score: " + score);
                break;

            case eScoreEventGolf.gameLoss:
                if (ROUND == 9) {
                    if (HIGH_SCORE >= score) {
                        print(" You got the high score! High Score: " + score);
                        HIGH_SCORE = score;
                        PlayerPrefs.SetInt("GolfHighScore", score);
                    }
                } else {
                    print("Your score for Round " + ROUND + " was: " + score);
                }
                break;

            default:
                break;
        }
    }

    static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    static public int SCORE_RUN { get { return S.scoreRun; } }
}