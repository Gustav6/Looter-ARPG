using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    [field: SerializeField] public int Health { get; private set; }

    [SerializeField] private BoxCollider2D colliderComponent;
    [SerializeField] private Rigidbody2D rbComponent;

    [SerializeField] private Loot[] lootList;
    [SerializeField] private GameObject lootPrefab;

    private Vector2Int region;

    private void Start()
    {
        region = new ((int)(transform.position.x / MapManager.Instance.RegionWidth), (int)(transform.position.y / MapManager.Instance.RegionHeight));
    }

    private void OnBecameVisible()
    {
        colliderComponent.enabled = true;
        rbComponent.simulated = true;
    }

    private void OnBecameInvisible()
    {
        colliderComponent.enabled = false;
        rbComponent.simulated = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            MapManager.Instance.RemoveGameObject(gameObject, region);

            Loot loot = GetLootDrop();

            if (lootList.Length > 0 && loot != null)
            {
                GameObject g = Instantiate(lootPrefab, transform.position, Quaternion.identity, MapManager.Instance.activeGameObjectsParent.transform);
                g.GetComponent<SpriteRenderer>().sprite = loot.lootSprite;
                MapManager.Instance.SetGameObjectsRegion(g);
            }
        }
    }

    private Loot GetLootDrop()
    {
        int randomNumber = new System.Random().Next(0, 101);
        List<Loot> possibleLoot = new();

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
                if (!loot.Equals(willDrop))
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
}
