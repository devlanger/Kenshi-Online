using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AbilitiesController : MonoBehaviour
{
    [SerializeField] private GameObject kunai;
    
    public void CastAbility(AbilityInfo abilityInfo)
    {
        GameObject inst = Instantiate(kunai, abilityInfo.user.transform.position + (UnityEngine.Quaternion.LookRotation(abilityInfo.aimPoint) * new Vector3(0, 1, 1)),
            Quaternion.LookRotation(abilityInfo.aimPoint));
        Vector3 dir = (abilityInfo.aimPoint - inst.transform.position).normalized * 10;
        
        inst.transform.rotation = Quaternion.LookRotation(dir);
        inst.GetComponent<Rigidbody>().velocity = dir;
    }
}   

public class AbilityInfo
{
    public Player user;
    public int abilityId;
    public Vector3 aimPoint;
}