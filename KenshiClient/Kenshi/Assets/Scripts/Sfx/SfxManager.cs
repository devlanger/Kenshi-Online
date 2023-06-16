using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class SfxManager : ItemsManager<SfxItem>
{
    [SerializeField] private List<SfxId> hitIds = new List<SfxId>();
    [SerializeField] private List<SfxItem> hitWoosh = new List<SfxItem>();
    [SerializeField] private List<SfxItem> dashSfx = new List<SfxItem>();

    public SfxItem GetRandomMeleeHitSfx()
    {
        return GetItemBySfxId(hitIds[UnityEngine.Random.Range(0, hitIds.Count)]);
    }
    
    public SfxItem GetRandomMeleeWooshSfx()
    {
        return hitWoosh[UnityEngine.Random.Range(0, hitWoosh.Count)];
    }

    public SfxItem GetRandomDashSfx()
    {
        return dashSfx[UnityEngine.Random.Range(0, dashSfx.Count)];
    }
    
    public SfxItem GetItemBySfxId(SfxId id)
    {
        return items.FirstOrDefault(s => s.sfxId == id);
    }

}