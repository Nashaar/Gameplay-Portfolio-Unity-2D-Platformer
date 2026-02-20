using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimationData", menuName = "Scriptable Objects/AnimationData")]
public class AnimationData : ScriptableObject
{
    [System.Serializable] public class Animation
    {
        public string animationName;
        public string triggerName;
        public int animationID;
    }

    public List<Animation> animData;

    public Animation Get(string name)
    {
        return animData.Find(a => a.animationName == name);
    }
}
