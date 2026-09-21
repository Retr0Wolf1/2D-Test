using UnityEngine;

public class FoodObject : CellObject
{
    public int AmountGranted = 10;
    public AudioClip EatSound;

    public override void PlayerEntered()
    {
        if (AudioManager.Instance != null && EatSound != null)
            AudioManager.Instance.SFXSource.PlayOneShot(EatSound, 2f);

        GameManager.Instance.ChangeFood(AmountGranted);
        Destroy(gameObject);
    }
}