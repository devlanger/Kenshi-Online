using Kenshi.Shared.Packets.GameServer;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarterAssets
{
    public class PlayerStatSlider : MonoBehaviour
    {
        [SerializeField] private StatEventPacket.StatId statId;
        [SerializeField] private Slider slider;
        
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
            if (statId == obj.statId && obj.playerId == GameRoomNetworkController.Instance.LocalPlayer.NetworkId)
            {
                slider.maxValue = (ushort)obj.maxValue;
                slider.value = (ushort)obj.value;
            }
        }
    }
}