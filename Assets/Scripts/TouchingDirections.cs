using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this is basically just for checking if the frog has something below them (ie if they're touching the ground or not)
public class TouchingDirections : MonoBehaviour
{
    private static TouchingDirections instance;
    public ContactFilter2D castFilter;
    public float groundDistance = 0.05f;
    BoxCollider2D touchingCol;
    Animator animator;

    RaycastHit2D[] groundHits = new RaycastHit2D[5];
    public bool isGrounded;

    void Awake() {
        if (instance != null) {
            Debug.LogWarning("Found more than one TouchingDirections in the scene");
        }
        instance = this;
        touchingCol = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    public static TouchingDirections GetInstance() {
        return instance;
    }

    void FixedUpdate() {
        isGrounded = touchingCol.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;
        animator.SetBool("isGrounded", isGrounded);
    }
}
