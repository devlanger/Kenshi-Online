using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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

    public List<int> skillMap = new List<int>();
    
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
        LoadSkillmap();
    }

    public void SetHotkey(int id, int abilityId)
    {
        var hotkey = hotkeys.FirstOrDefault(h => h.hotkeyId == id);
        if (hotkey == null)
        {
            return;
        }

        skillMap[id] = abilityId;
        hotkey.abilityId = abilityId;
        OnHotkeysChanged?.Invoke(hotkeys);
    }

    private void LoadSkillmap()
    {
        var skillsString = PlayerPrefs.GetString("skillmap");
        if (!String.IsNullOrEmpty(skillsString))
        {
            skillMap = JsonConvert.DeserializeObject<List<int>>(skillsString);
        }
        else
        {
            skillMap = new List<int>()
            {
                3, 4, 5, 1, 2
            };

            SaveSkillmap();
        }
            
        for (int i = 0; i < hotkeys.Count; i++)
        {
            SetHotkey(i, skillMap[i]);
        }
    }

    public void SaveSkillmap()
    {
        PlayerPrefs.SetString("skillmap", JsonConvert.SerializeObject(skillMap));
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
                    hitPoint = stateMachine.Target.Input.HitPoint,
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
    public Vector3 hitPoint;
}