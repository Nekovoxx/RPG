using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class TilemapInstantDeathHazard : MonoBehaviour
{
    [SerializeField] private bool killEveryTile;
    [SerializeField] private TileBase[] lethalTiles;
    [SerializeField] private string[] lethalTileNameKeywords =
    {
        "lava",
        "magma",
        "spike",
        "thorn",
        "\u5ca9\u6d46",
        "\u5730\u523a"
    };
    [SerializeField] private float contactProbeDistance = 0.08f;

    private readonly HashSet<TileBase> lethalTileSet = new HashSet<TileBase>();
    private Tilemap tilemap;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        RebuildTileSet();
    }

    private void OnValidate()
    {
        RebuildTileSet();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKillFromCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryKillFromCollision(collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKillFromCollider(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryKillFromCollider(other);
    }

    private void TryKillFromCollision(Collision2D collision)
    {
        if (!IsPlayer(collision.collider))
            return;

        if (killEveryTile || HitsLethalTile(collision))
            KillPlayer(collision.collider);
    }

    private void TryKillFromCollider(Collider2D other)
    {
        if (!IsPlayer(other))
            return;

        if (killEveryTile || OverlapsLethalTile(other.bounds))
            KillPlayer(other);
    }

    private bool HitsLethalTile(Collision2D collision)
    {
        int contactCount = collision.contactCount;

        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);

            if (IsLethalAtWorld(contact.point))
                return true;

            if (IsLethalAtWorld(contact.point + contact.normal * contactProbeDistance))
                return true;

            if (IsLethalAtWorld(contact.point - contact.normal * contactProbeDistance))
                return true;
        }

        return false;
    }

    private bool OverlapsLethalTile(Bounds bounds)
    {
        return IsLethalAtWorld(bounds.center) ||
               IsLethalAtWorld(new Vector2(bounds.min.x, bounds.min.y)) ||
               IsLethalAtWorld(new Vector2(bounds.min.x, bounds.max.y)) ||
               IsLethalAtWorld(new Vector2(bounds.max.x, bounds.min.y)) ||
               IsLethalAtWorld(new Vector2(bounds.max.x, bounds.max.y));
    }

    private bool IsLethalAtWorld(Vector2 worldPosition)
    {
        if (tilemap == null)
            return false;

        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        TileBase tile = tilemap.GetTile(cellPosition);

        return IsLethalTile(tile);
    }

    public bool IsLethalAtWorldPosition(Vector2 worldPosition)
    {
        return killEveryTile || IsLethalAtWorld(worldPosition);
    }

    public bool OverlapsLethalTileBounds(Bounds bounds)
    {
        return killEveryTile || OverlapsLethalTile(bounds);
    }

    private bool IsLethalTile(TileBase tile)
    {
        if (tile == null)
            return false;

        if (lethalTileSet.Contains(tile))
            return true;

        string tileName = tile.name;

        if (string.IsNullOrEmpty(tileName) || lethalTileNameKeywords == null)
            return false;

        for (int i = 0; i < lethalTileNameKeywords.Length; i++)
        {
            string keyword = lethalTileNameKeywords[i];

            if (!string.IsNullOrEmpty(keyword) &&
                tileName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    private bool IsPlayer(Collider2D other)
    {
        return other != null && other.GetComponentInParent<Player>() != null;
    }

    private void KillPlayer(Collider2D other)
    {
        Player player = other.GetComponentInParent<Player>();

        if (player == null)
            return;

        PlayerStats stats = player.GetComponent<PlayerStats>();

        if (stats != null && !stats.isDead)
            stats.TakeDamage(999999);
        else if (stats == null)
            player.Die();
    }

    private void RebuildTileSet()
    {
        lethalTileSet.Clear();

        if (lethalTiles == null)
            return;

        for (int i = 0; i < lethalTiles.Length; i++)
        {
            if (lethalTiles[i] != null)
                lethalTileSet.Add(lethalTiles[i]);
        }
    }
}
