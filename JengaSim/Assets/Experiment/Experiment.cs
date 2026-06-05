using System.Threading.Tasks;
using Balancing;
using UnityEngine;

namespace Experiment
{
    public class Experiment : MonoBehaviour
    {
        public GameObject TowerBuilderObject;

        public int height { get; set; } = 4;
        public int width { get; set; } = 3;
        
        public CoGBalancing Balancer { get; set; }
        public TowerBuilder TowerBuilder { get; set; }

        public bool Finished { get; private set; } = false;
        public int Progress { get; private set; } = 0;
        private TaskCompletionSource<BlockState> tcs = new();

        public void Awake()
        {
            Balancer = new(height, width);
            TowerBuilder = TowerBuilderObject.GetComponent<TowerBuilder>();
            TowerBuilder.Initialize(width);
        }

        public async Awaitable<(Balance, BlockState)> RunAsync(Board board)
        {
            (Balance, BlockState) result;
        
            result.Item1 = Balancer.Calculate(board);
            
            TowerBuilder.PlaceTower(board, ActionEventHandler);
            result.Item2 = await tcs.Task;
            
            ResetExperiment();

            return result;
        }
    
        public void ActionEventHandler(object sender, ActionEventArgs e)
        {
            if (Finished) return;
        
            if (e.BlockState.testState == TestStates.SLEEP)
            {
                Progress++;
                Finished = Progress >= TowerBuilder.Tower.Count;
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
            TowerBuilder.DestroyTower();
        
            tcs = new();
            Finished = false;
            Progress = 0;
        }
    }
}