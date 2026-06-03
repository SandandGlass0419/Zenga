using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

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

        await AverageExperiment(exp2, 4);
    }

    public async Awaitable AverageExperiment(Board board, int times)
    {
        Experiment experiment = new(TowerBuilder.GetComponent<TowerBuilder>(), 3, 3);

        for (int i = 0; i < times; i++)
        {
            await experiment.RunNewAsync(board);
        }

        Debug.Log($"Avg: {experiment.cogResults[0].value}, {experiment.unityResults.Average(state => state.time)}");
    }
}

public class Experiment
{
    public CoGBalancing Balancer { get; set; }
    public TowerBuilder Builder { get; set; }
    
    public bool Finished = false;
    public int Progress = 0;
    private TaskCompletionSource<BlockState> tcs = new();

    public List<Balance> cogResults = new();
    public List<BlockState> unityResults = new();
    
    public Experiment(TowerBuilder builder, int height, int width)
    {
        this.Balancer = new(height, width);
        this.Builder = builder;
        
        Builder.Initialize(width);
    }

    public async Awaitable RunNewAsync(Board board)
    {
        cogResults.Add(Balancer.Calculate(board));
        
        Builder.PlaceTower(board, Motion);
        unityResults.Add(await tcs.Task);
        
        ResetExperiment();

        await Awaitable.FixedUpdateAsync();
    }
    
    public void Motion(object sender, MotionEventArgs e)
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
        else
        {
            Finished = true;
        }

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