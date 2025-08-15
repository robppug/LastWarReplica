using SL.Core;
using UnityEngine;

namespace SL.Core
{
    /// <summary>
    /// Attach this to an empty GameObject in the Boot scene.
    /// It ensures Game exists and starts the boot flow.
    /// </summary>
    public class Boot : MonoBehaviour
    {
        [SerializeField] private string homeScene = "HomeBase";

        private void Start()
        {
            // Ensure singleton exists
            var _ = Game.Instance;
            Game.Instance.Boot(homeScene);
        }
    }
}
