using System.Collections.Generic;
using UnityEngine;

public class RedLightHighlighter : MonoBehaviour
{
    
    public void UpdateState(IEnumerable<string> allowedIds, IEnumerable<string> disallowedIds)
    {
        var disallow = new HashSet<string>(disallowedIds ?? System.Array.Empty<string>());

        
        var activities = FindObjectsOfType<ViewActivity>(includeInactive: true);

        foreach (var act in activities)
        {
            
            var light = act.GetComponentInChildren<ActivityLight>(includeInactive: true);
            if (!light) continue;

            
            bool isInvalid = disallow.Contains(act.Id);
            light.ShowInvalid(isInvalid);
        }
    }
}