using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pot : MonoBehaviour, IDamagable
{
    private int health;

    public int CurrentHealth
    {
        get => health;
        set
        {
            if (value > MaxHealth)
            {
                health = MaxHealth;
            }
            else if (value < 0)
            {
                OnDeath();
            }
            else
            {
                health = value;
            }
        } 
    }

    [field: SerializeField] public int MaxHealth { get; set; }
    public bool TickDamageActive { get; set; }

    [SerializeField] private Loot[] guaranteedToDrop;

    [SerializeField] private Loot[] lootList;
    [SerializeField] private GameObject lootPrefab;

    private readonly List<Loot> possibleLoot = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnDeath();
            Debug.Log("Pot Destroyed");
        }
    }

    private Loot GetLootDrop()
    {
        int randomNumber = new System.Random().Next(0, 101);

        foreach (Loot loot in lootList)
        {
            if (loot.dropChance >= randomNumber)
            {
                possibleLoot.Add(loot);
            }
        }

        Loot willDrop = null;

        if (possibleLoot.Count > 0)
        {
            willDrop = possibleLoot.First();

            foreach (Loot loot in possibleLoot)
            {
                if (loot != willDrop)
                {
                    if (willDrop.dropChance > loot.dropChance)
                    {
                        willDrop = loot;
                    }
                }
            }
        }

        return willDrop;
    }

    public void Damage(int damageAmount)
    {
        CurrentHealth -= damageAmount;
    }

    public void OnDeath()
    {
        Loot loot = GetLootDrop();
        GameObject g;

        if (loot != null)
        {
            g = MapManager.Instance.SpawnPrefab(lootPrefab, Vector3Int.FloorToInt(transform.position), MapManager.Instance.currentMap);
            g.GetComponent<SpriteRenderer>().sprite = loot.lootSprite;
        }

        foreach (Loot guaranteedDrop in guaranteedToDrop)
        {
            g = MapManager.Instance.SpawnPrefab(lootPrefab, Vector3Int.FloorToInt(transform.position), MapManager.Instance.currentMap);
            g.GetComponent<SpriteRenderer>().sprite = guaranteedDrop.lootSprite;

            g.GetComponent<BoxCollider2D>().size = guaranteedDrop.lootSprite.rect.size / 32;
        }

        MapManager.Instance.RemoveGameObject(gameObject, MapManager.Instance.currentMap.MapRegions);
    }
}
