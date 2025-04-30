using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//this is just for holding the game over/restart functionality
public class LogicManager : MonoBehaviour
{

    public bool gameIsOver = false;
    public GameObject gameOverScreen;

    //start again
    public void restartGame() {
        gameIsOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void gameOver() {
        gameOverScreen.SetActive(true);
        gameIsOver = true;
    }
}
