using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace SL.Systems
{
    public interface ISceneLoader
    {
        Task LoadSingleAsync(string sceneName);
    }

    public sealed class SceneLoader : ISceneLoader
    {
        public async Task LoadSingleAsync(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (op == null) return;
            op.allowSceneActivation = true;
            while (!op.isDone)
                await System.Threading.Tasks.Task.Yield();
        }
    }
}
