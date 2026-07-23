using UnityEngine;
using System.Collections;

public class PlayerCombatOrParry : MonoBehaviour
{
    public bool IsParryActive { get; private set; }
    public float parryWindow = 0.2f;
    public float shadowRewindAmount = 2f;

    public void ActivateParry()
    {
        StopAllCoroutines();
        StartCoroutine(ParryRoutine());
    }

    IEnumerator ParryRoutine()
    {
        IsParryActive = true;
        yield return new WaitForSeconds(parryWindow);
        IsParryActive = false;
    }

    public void ParryShadow(ShadowPlayback shadow)
    {
        shadow.delaySeconds += shadowRewindAmount;
    }
}