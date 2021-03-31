using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardGolf : MonoBehaviour {
    public static ScoreboardGolf S;

    [Header("Set Dynamically")]
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;

    private Transform canvasTrans;

    public int score { 
        get {
            return (_score);
        }
        set {
            _score = value;
            _scoreString = _score.ToString();
            GetComponent<Text>().text = _score.ToString();
        }
    }
    
    public string scoreString { 
        get {
            return (_scoreString);
        }
        set {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }

    void Awake() { 
        if (S == null) {
            S = this;
        } else {
            Debug.LogError("ERROR: Scoreboard.Awake(): S is already set!");
        }
        canvasTrans = transform.parent;
    }
}