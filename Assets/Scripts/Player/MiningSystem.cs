using UnityEngine;
using System.Collections.Generic;

public class MiningSystem : MonoBehaviour
{
    [Header("Equipped Pickaxe")]
    public PickaxeData equippedPickaxe;

    [Header("References")]
    public CaveGenerator world;

    [Header("Tile Highlight")]
    public GameObject highlightObject;

    private SpriteRenderer pickaxeSR;
    private GameObject pickaxeObject;
    private float mineTimer;
    private Dictionary<Vector3Int, int> damageMap = new Dictionary<Vector3Int, int>();

    private Vector3Int targetCell;
    private bool hasTarget;

    private bool swinging;
    private Vector3 swingPosition;
    private float swingTimer;
    private float swingDuration = 0.2f;
    private float swingAngleFrom = 0f;
    private float swingAngleTo = 0f;

    void Start()
    {
        if (world == null)
            world = FindObjectOfType<CaveGenerator>();

        if (highlightObject == null)
        {
            Transform t = transform.Find("TileHighlight");
            if (t != null) highlightObject = t.gameObject;
        }

        GameObject go = new GameObject("PickaxeSwing");
        pickaxeSR = go.AddComponent<SpriteRenderer>();
        pickaxeSR.sortingOrder = 5;
        pickaxeObject = go;
        UpdatePickaxeSprite();
        pickaxeObject.SetActive(false);

        if (highlightObject != null)
            highlightObject.transform.localScale = Vector3.one;
    }

    void Update()
    {
        UpdateTarget();
        UpdateHighlight();
        HandleMining();
        UpdateSwing();
    }

    void UpdateTarget()
    {
        hasTarget = false;
        if (Camera.main == null || world == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        float reach = equippedPickaxe != null ? equippedPickaxe.reach * 2f + 1f : 3f;
        if (Vector2.Distance(transform.position, mouseWorld) > reach) return;

        int tx = Mathf.FloorToInt(mouseWorld.x);
        int ty = Mathf.FloorToInt(mouseWorld.y);
        TileType t = world.GetTile(tx, ty);

        if (t == TileType.Air || t == TileType.Bedrock) return;

        targetCell = new Vector3Int(tx, ty, 0);
        hasTarget = true;

        int dx = tx - Mathf.FloorToInt(transform.position.x);
        int dy = ty - Mathf.FloorToInt(transform.position.y);
        OrientPickaxe(dx, dy);
    }

    void OrientPickaxe(int dx, int dy)
    {
        if (pickaxeObject == null) return;

        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg - 90f;

        if (!swinging)
            pickaxeObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        swingAngleFrom = angle + 60f;
        swingAngleTo = angle - 60f;
    }

    void UpdateHighlight()
    {
        if (highlightObject == null) return;
        highlightObject.SetActive(hasTarget);
        if (hasTarget)
            highlightObject.transform.position = new Vector3(
                targetCell.x + 0.5f, targetCell.y + 0.5f, 0f);
    }

    void HandleMining()
    {
        if (equippedPickaxe == null || world == null) return;

        mineTimer -= Time.deltaTime;
        if (!Input.GetMouseButton(0)) return;
        if (!hasTarget) return;
        if (mineTimer > 0f) return;

        TileType t = world.GetTile(targetCell.x, targetCell.y);
        if (t == TileType.Air || t == TileType.Bedrock) return;
        if (!CanMine(t, equippedPickaxe.miningPower)) return;

        mineTimer = equippedPickaxe.mineInterval;

        int hits = 0;
        damageMap.TryGetValue(targetCell, out hits);
        hits++;
        damageMap[targetCell] = hits;

        swingPosition = new Vector3(targetCell.x + 0.5f, targetCell.y + 0.5f, 0f);
        TriggerSwing();

        if (hits >= CaveGenerator.GetHardness(t))
        {
            damageMap.Remove(targetCell);
            world.BreakTile(targetCell.x, targetCell.y);
            swinging = false;
            if (pickaxeObject != null) pickaxeObject.SetActive(false);
        }
    }

    bool CanMine(TileType t, int power)
    {
        switch (t)
        {
            case TileType.Dirt: return power >= 1;
            case TileType.Stone: return power >= 1;
            case TileType.CoalOre: return power >= 1;
            case TileType.IronOre: return power >= 1;
            case TileType.CopperOre: return power >= 1;
            case TileType.GoldOre: return power >= 2;
            case TileType.EmeraldOre: return power >= 3;
            default: return power >= 1;
        }
    }

    void TriggerSwing()
    {
        swinging = true;
        swingTimer = 0f;
        if (pickaxeObject != null)
        {
            pickaxeObject.SetActive(true);
            pickaxeObject.transform.position = swingPosition;
        }
    }

    void UpdateSwing()
    {
        if (pickaxeObject == null) return;
        if (!swinging) { pickaxeObject.SetActive(false); return; }

        swingTimer += Time.deltaTime;
        float t = swingTimer / swingDuration;

        if (t >= 1f)
        {
            swinging = false;
            pickaxeObject.SetActive(false);
            return;
        }

        float angle = Mathf.Lerp(swingAngleFrom, swingAngleTo, t);
        pickaxeObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void EquipPickaxe(PickaxeData pickaxe)
    {
        equippedPickaxe = pickaxe;
        UpdatePickaxeSprite();
    }

    void UpdatePickaxeSprite()
    {
        if (pickaxeSR == null) return;
        if (equippedPickaxe != null && equippedPickaxe.sprite != null)
            pickaxeSR.sprite = equippedPickaxe.sprite;
    }
}