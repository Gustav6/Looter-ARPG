using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IDamagable
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public bool TickDamageActive { get; set; }

    public void Damage(int damageAmount);
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
