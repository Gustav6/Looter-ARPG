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

    [SerializeField] private Loot[] lootList;
    [SerializeField] private GameObject lootPrefab;

    private readonly List<Loot> possibleLoot = new();
    private Vector2Int region;

    private void Start()
    {
        region = new((int)(transform.position.x / MapManager.Instance.RegionWidth), (int)(transform.position.y / MapManager.Instance.RegionHeight));
    }

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

        if (loot != null)
        {
            Transform parent = MapManager.Instance.currentMap.transform.GetChild(1);

            GameObject g = MapManager.Instance.SpawnPrefab(lootPrefab, Vector3Int.FloorToInt(transform.position), MapManager.Instance.currentMap, parent);
            g.GetComponent<SpriteRenderer>().sprite = loot.lootSprite;
        }

        MapManager.Instance.RemoveGameObject(gameObject, MapManager.Instance.currentMap.MapRegions, region);
    }
}
