using System.Linq;
using System.Threading.Tasks;
using Balancing;
using UnityEngine;

namespace Experiment
{
    public class ExperimentManager : MonoBehaviour
    {
        public GameObject TowerBuilder;
    
        public async void Awake()
        {
            Experiment Experiment = new(TowerBuilder.GetComponent<TowerBuilder>(), 3, 3);
        
            Time.timeScale = 20f;

            Board exp1 = new(height: 3) { Tower = new byte[] { 5, 6, 2, 5, 3, 3, 1, 2, 0 } };
            Board exp2 = new(height: 3) { Tower = new byte[] { 2, 2, 2, 2, 2, 2, 2, 2, 2 } };
            Board exp3 = new(height: 3) { Tower = new byte[] { 5, 5, 7, 7, 5, 5, 5, 7, 7 } };

            await Experiment.RunNewAverageAsync(exp1, 4);
            await Experiment.RunNewAverageAsync(exp2, 4);
            await Experiment.RunNewAverageAsync(exp3, 4);
        
            await Experiment.RunNewAverageAsync(exp1, 4);
            await Experiment.RunNewAverageAsync(exp2, 4);
            await Experiment.RunNewAverageAsync(exp3, 4);
        }
    }

    public class Experiment
    {
        public CoGBalancing Balancer { get; set; }
        public TowerBuilder Builder { get; set; }
    
        public bool Finished = false;
        public int Progress = 0;
        private TaskCompletionSource<BlockState> tcs = new();
    
        public Experiment(TowerBuilder builder, int height, int width)
        {
            this.Balancer = new(height, width);
            this.Builder = builder;
        
            Builder.Initialize(width);
        }

        public async Awaitable<(Balance, BlockState)> RunNewAsync(Board board)
        {
            (Balance, BlockState) result;
        
            result.Item1 = Balancer.Calculate(board);
        
            Builder.PlaceTower(board, MotionEventHandler);
            result.Item2 = await tcs.Task;
        
            ResetExperiment();
            await Awaitable.FixedUpdateAsync();

            return result;
        }
    
        public async Awaitable<(Balance, BlockState)> RunNewAverageAsync(Board board, int times)
        {
            (Balance, BlockState)[] results = new (Balance, BlockState)[times];
        
            for (int i = 0; i < times; i++)
            {
                results[i] = await RunNewAsync(board);
            }

            var balance = results.First().Item1;
            var blockstate = BlockState.Average(results.Select(r => r.Item2).ToArray());
        
            return (balance, blockstate);
        }
    
        public void MotionEventHandler(object sender, MotionEventArgs e)
        {
            if (Finished) return;
        
            if (e.BlockState.testState == TestStates.SLEEP)
            {
                Progress++;
                Finished = Progress >= Builder.Tower.Count;
            }
            else if (e.BlockState.testState == TestStates.AWAKE)
            {
                Progress--;
            }
        
            else Finished = true;

            if (Finished)
            {
                tcs.SetResult(e.BlockState);
            }
        }
    
        public void ResetExperiment()
        {
            Balancer.Reset();
            Builder.DestroyTower();
        
            tcs = new();
            Finished = false;
            Progress = 0;
        }
    }
}