using UnityEngine;

namespace LWR.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "LWR/Game Config")]
    public class GameConfig : ScriptableObject
    {
        public string version = "0.0.1";
    }
}
