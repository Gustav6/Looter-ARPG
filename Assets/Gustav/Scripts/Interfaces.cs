using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IDamagable
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public bool TickDamageActive { get; set; }

    public bool IsBeingKnockedBack { get; set; }
    public Coroutine KnockbackCoroutine { get; set; }
    public AnimationCurve KnockbackForceCurve { get; set; }

    public void Damage(int damageAmount);

    public void Knockback(Controller2D controller, Vector2 hitDirection, float hitDirectionForce)
    {
        if (KnockbackCoroutine != null)
        {
            controller.StopCoroutine(KnockbackCoroutine);
        }

        KnockbackCoroutine = controller.StartCoroutine(KnockbackAction(controller, hitDirection, hitDirectionForce));
    }

    private IEnumerator KnockbackAction(Controller2D controller, Vector2 hitDirection, float hitDirectionForce)
    {
        IsBeingKnockedBack = true;

        Vector2 knockbackForce;

        float elapsedTime = 0f;
        float time = 0f;

        while (elapsedTime < KnockbackForceCurve[KnockbackForceCurve.length - 1].time)
        {
            elapsedTime += Time.fixedDeltaTime;
            time += Time.fixedDeltaTime;

            knockbackForce = hitDirectionForce * KnockbackForceCurve.Evaluate(time) * hitDirection;

            controller.Move(knockbackForce);

            yield return new WaitForFixedUpdate();
        }

        IsBeingKnockedBack = false;
        KnockbackCoroutine = null;
    }

    public void OnDeath();

    public IEnumerator TickDamage(int tickDamage, Transform hit, GameObject textPrefab, float timePerTick = .5f, int amountOfTicks = 10)
    {
        int tempTimer = 0;
        TickDamageActive = true;

        while (tempTimer <= amountOfTicks)
        {
            tempTimer++;

            Damage(tickDamage);
            DamagePopUp(textPrefab, hit.position, 10);

            yield return new WaitForSeconds(timePerTick);
        }

        TickDamageActive = false;
    }

    public void DamagePopUp(GameObject textPrefab, Vector3 spawnPosition, int damageAmount)
    {
        GameObject textPopUp = Object.Instantiate(textPrefab, spawnPosition, Quaternion.identity);
        textPopUp.GetComponent<TextMeshPro>().text = damageAmount.ToString();
    }
}
