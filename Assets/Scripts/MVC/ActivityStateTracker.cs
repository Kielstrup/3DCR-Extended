using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

namespace MVC
{
    /// <summary>
    /// Tracks which activities are allowed/disallowed at each step in the DCR graph execution
    /// </summary>
    public class ActivityStateTracker
    {
        private Dictionary<string, ActivityState> _activityStates = new Dictionary<string, ActivityState>();
        // Singleton pattern for global access you guys cannot mess this up.
        private static ActivityStateTracker _instance;
        public static ActivityStateTracker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ActivityStateTracker();
                }
                return _instance;
            }
        }

 
 
 
        /// <summary>
        /// Records the current state of all activities
        /// </summary>
        public void RecordActivityStates(Dictionary<string, ActivityState> activityStates)
        {
            // logs the current state of all activities
            _activityStates = new Dictionary<string, ActivityState>(activityStates);
        }

        /// <summary>
        /// Get all allowed activities at the current step
        /// </summary>
        public List<string> GetAllowedActivities()
        {
            return _activityStates.Count == 0 ? new List<string>() : (from kvp in _activityStates where kvp.Value.isAllowed select kvp.Key).ToList();
        }

        /// <summary>
        /// Get all disallowed activities at the current step
        /// </summary>
        public List<string> GetDisallowedActivities()
        {
            return _activityStates.Count == 0 ? new List<string>() : (from kvp in _activityStates where !kvp.Value.isAllowed select kvp.Key).ToList();
        }


        public static void ResetInstance()
        {
            _instance = null;
        }
    }

    /// <summary>
    /// Individual activity state information
    /// </summary>
    [Serializable]
    public class ActivityState
    {
        [FormerlySerializedAs("ActivityId")] public string activityId;
        [FormerlySerializedAs("ActivityName")] public string activityName;
        [FormerlySerializedAs("IsAllowed")] public bool isAllowed;
        [FormerlySerializedAs("IsIncluded")] public bool isIncluded;
        [FormerlySerializedAs("IsExecuted")] public bool isExecuted;
  
        public ActivityState(string id, string name, bool allowed, bool included, bool executed)
        {
            activityId = id;
            activityName = name;
            isAllowed = allowed;
            isIncluded = included;
            isExecuted = executed;
  

        }
    }
}