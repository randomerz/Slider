
using System.Collections;
using UnityEngine;

public class JungleItemDestroyTimer : MonoBehaviour
{
    private const float DESTROY_AFTER_SECONDS = 60;
    private const float FLASH_FOR_SECONDS = 5;

    private Coroutine destroyCoroutine;

    public FlashWhiteSprite flashWhite;

    public void StopTimer()
    {
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
            flashWhite.StopFlashing();
        }
    }

    public void RestartTimer()
    {
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
            flashWhite.StopFlashing();
        }
        destroyCoroutine = StartCoroutine(DestroyItemAfterDelay());
    }

    private IEnumerator DestroyItemAfterDelay()
    {
        yield return new WaitForSeconds(DESTROY_AFTER_SECONDS);

        flashWhite.Flash((int)(FLASH_FOR_SECONDS / flashWhite.flashTime));

        yield return new WaitForSeconds(FLASH_FOR_SECONDS);

        ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position, transform.parent);
        Destroy(gameObject);
    }
}