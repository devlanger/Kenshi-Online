using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarterAssets;
using UnityEngine;

public class AnimationsController : MonoBehaviour
{
    public List<DashAnimation> dashAnimations;
    
    [System.Serializable]
    public class DashAnimation
    {
        public int value;
        public bool alternate;
        public DashState.Data.DashIndex index;
        public VfxScriptable fx;
    }

    public DashAnimation GetDashAnimation(DashState.Data.DashIndex index, bool alternate)
    {
        var d = dashAnimations.FirstOrDefault(d => d.index == index && d.alternate == alternate);
        return d;
    }
}
