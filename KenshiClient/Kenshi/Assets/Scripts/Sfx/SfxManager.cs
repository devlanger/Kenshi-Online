using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class SfxManager : ItemsManager<SfxItem>
{
    [SerializeField] private List<SfxId> hitIds = new List<SfxId>();

    public SfxItem GetRandomMeleeHitSfx()
    {
        return GetItemBySfxId(hitIds[UnityEngine.Random.Range(0, hitIds.Count)]);
    }

    public SfxItem GetItemBySfxId(SfxId id)
    {
        return items.FirstOrDefault(s => s.sfxId == id);
    }

}