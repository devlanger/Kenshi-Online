using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarterAssets.CombatStates;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

public class AbilitiesController : MonoBehaviour
{
    public static AbilitiesController Instance;
    
    [SerializeField] private GameObject kunai;

    public List<AbilityHotkey> hotkeys = new List<AbilityHotkey>();

    public AbilitiesManager abilitiesManager;
    public List<AbilityScriptable> abilities => abilitiesManager.abilities;
    
    public event Action<List<AbilityHotkey>> OnHotkeysChanged;
    
    [System.Serializable]
    public class AbilityHotkey
    {
        public int hotkeyId;
        public int abilityId;
        public KeyCode key;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void SetHotkey(int id, int abilityId)
    {
        var hotkey = hotkeys.FirstOrDefault(h => h.hotkeyId == id);
        if (hotkey == null)
        {
            return;
        }

        hotkey.abilityId = abilityId;
        OnHotkeysChanged?.Invoke(hotkeys);
    }

    private void Start()
    {
        //1
        SetHotkey(0, 3);
        //2
        SetHotkey(1, 4);
        //3
        SetHotkey(2, 5);
        //E
        SetHotkey(3, 1);
        //F
        SetHotkey(4, 2);
    }

    public void UpdateInputs()
    {
        foreach (var hotkey in hotkeys)
        {
            if (Input.GetKeyDown(hotkey.key))
            {
                var stateMachine = GameRoomNetworkController.Instance.LocalPlayer.playerStateMachine;
                stateMachine.ChangeState(new AbilityCastState(new AbilityCastState.Data
                {
                    abilityId = hotkey.abilityId,
                    hitPoint = stateMachine.Target.Input.AimDirection,
                    startPos = stateMachine.Target.transform.position,
                }));
            }
        }
    }

    public AbilityData CastAbility(AbilityInfo abilityInfo)
    {
        if (!GetAbilityById(abilityInfo.abilityId, out var ability)) return null;

        abilityInfo.user.Input.abilityInfo = abilityInfo;
        abilityInfo.user.GetComponent<PlayableDirector>().Play(ability.Data.behaviour);
        return ability.Data;
    }

    public bool GetAbilityById(int id, out AbilityScriptable ability)
    {
        ability = abilities.FirstOrDefault(a => a.Id == id);
        if (ability == null)
        {
            return false;
        }

        return true;
    }
}   

public class AbilityInfo
{
    public Player user;
    public int abilityId;
    public Vector3 aimPoint;
}