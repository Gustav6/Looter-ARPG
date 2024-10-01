using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    int currentHealth, maxHealth, currentXp,
        currentExperiance, maxExperiance,
        currentLevel;

    private void OnEnable()
    {
        //subscribe to event
        ExperianceManager.Instance.OnExperianceChange += HandleExperianceChange;
    }
    private void OnDisable()
    {
        //unsubscribe from event
        ExperianceManager.Instance.OnExperianceChange -= HandleExperianceChange;
    }

    private void HandleExperianceChange(int newExperiance)
    {
        currentExperiance += newExperiance;
        if (currentExperiance >= maxExperiance)
        {
            LevelUp();

        }
    }
    private void LevelUp()
    {
        maxHealth += 10;
        currentHealth = maxHealth;

        currentLevel++;

        currentExperiance = 0;
        maxExperiance += 100;
    }
}
