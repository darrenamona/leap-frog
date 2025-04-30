using UnityEngine;
using System.Collections;

public class FlyPickup : MonoBehaviour
{
    private SpriteRenderer sr;
    private Collider2D col;
    private Animator animator;

    private bool collected = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.hasFly)
            {
                collected = true;
                player.CollectFly(this);
                StartCoroutine(PlayExplodeThenHide());
            }
        }
    }

    IEnumerator PlayExplodeThenHide()
    {
        animator.SetTrigger("explode");

        // Wait for explosion animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        sr.enabled = false;
        col.enabled = false;
    }

    public void RespawnFly()
    {
        sr.enabled = true;
        col.enabled = true;
        collected = false;
        animator.Play("hover", 0); // reset to hover animation
    }
}
