using Kenshi.Shared.Packets.GameServer;
using TMPro;
using UnityEngine;

namespace StarterAssets
{
    public class PlayerStatText
    {
        [SerializeField] private StatEventPacket.StatId statId;
        [SerializeField] private TextMeshProUGUI text;
        
        private void Awake()
        {
            if (CombatController.Instance != null)
            {
                CombatController.Instance.OnStatsChanged += InstanceOnOnStatsChanged;
            }
        }

        private void OnDestroy()
        {
            if (CombatController.Instance != null)
            {
                CombatController.Instance.OnStatsChanged -= InstanceOnOnStatsChanged;
            }
        }
        
        private void InstanceOnOnStatsChanged(StatEventPacket.Data obj)
        {
            if (statId == obj.statId)
            {
                text.SetText(obj.value.ToString());
            }
        }
    }
}