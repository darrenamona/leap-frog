using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The only thing this script does is call a game over when the player enters the trigger collision zone
public class GoalScript : MonoBehaviour
{
    public LogicManager logic;

    void Start()
    {
        //there should only ever be one LogicManager at a time
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            logic.gameOver();
        }
    }
}
