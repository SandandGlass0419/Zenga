using Balancing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Experiment
{
    public class Lab : MonoBehaviour
    {
        public const string ExperimentDir = "Scenes/";
        public const string ExperimentName = "Experiment";
        
        public async void Awake()
        {
            Board exp1 = new(height: 3) { Tower = new byte[] { 5, 6, 2, 5, 3, 3, 1, 2, 0 } };
            Board exp2 = new(height: 3) { Tower = new byte[] { 2, 2, 2, 2, 2, 2, 2, 2, 2 } };
            Board exp3 = new(height: 3) { Tower = new byte[] { 5, 5, 7, 7, 5, 5, 5, 7, 7 } };

            Time.timeScale = 10f;
            
            await Experiment(exp1);
            await Experiment(exp3);
            await Experiment(exp1);
            await Experiment(exp3);
            await Experiment(exp2);
            await Experiment(exp2);
        }

        public async Awaitable Experiment(Board board)
        {
            await SceneManager.LoadSceneAsync(ExperimentDir + ExperimentName, LoadSceneMode.Additive);
            await FindFirstObjectByType<Experiment>().RunAsync(board);
            await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(ExperimentName));
        }
    }
}
