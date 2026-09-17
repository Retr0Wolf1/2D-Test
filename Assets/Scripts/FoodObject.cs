using UnityEngine;

public class FoodObject : CellObject
{
    public int AmountGranted = 10;
    public AudioClip EatSound;

    public override void PlayerEntered()
    {
        if (EatSound != null)
            AudioSource.PlayClipAtPoint(EatSound, transform.position);

        GameManager.Instance.ChangeFood(AmountGranted);
        Destroy(gameObject);
    }
}