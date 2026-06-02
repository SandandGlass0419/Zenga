using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Experiment : MonoBehaviour
{
    public GameObject BlockPrefab;
    public GameObject PlaneObject;
    public BlockStateBuilder BlockStateBuilder;
    public List<GameObject> Tower;
    
    public void Awake()
    {
        BlockStateBuilder = new(BlockPrefab.transform.localScale, 3);

        Time.timeScale = 10f;
        
        //RunNew(new byte[] {5, 6, 2, 5, 3, 3, 1, 2, 0});
        //RunNew(new byte[] {0b101, 0b011, 0b101, 0b010, 0, 0, 0, 0, 0});
        //RunNew(new byte[] {2,2,2,2,2,2,2,2,2});

        //TestDepth(1);
        
        RunNew(new(3, 3) { Tower = new byte[] {5,5,7,7,7,7,7,7,7} });
    }
    
    public void PlaceLayer(byte layer, int index, Axis axis)
    {
        foreach (var blockAttribute in BlockStateBuilder.BuildLayerIdentity(layer, index, axis))
        {
            PlaceBlock(blockAttribute);
        }
    }
    
    public void PlaceBlock(BlockState block)
    {
        var instBlock = Instantiate(BlockPrefab, block.pos, block.rotation);
        instBlock.GetComponent<BlockObserver>().Initialize(block, BlockStateBuilder.BuildNeighborBlocks(block));
        instBlock.GetComponent<BlockObserver>().MotionFinishedEvent += MotionFinished;

        Tower.Add(instBlock);
    }

    public void PlaceTower(byte[] tower)
    {
        Axis axis = Axis.X;
        
        for (int i = 0; i < tower.Length; i++)
        {
            PlaceLayer(tower[i], i, axis);
            axis = axis.Cycle();
        }
    }

    public void DestroyTower()
    {
        Tower.ForEach(Destroy);
        Tower.Clear();
    }
}

public partial class Experiment
{
    public int FinishedCount { get; set; } = 0;
    public CoGBalancing Balancer { get; set; } = new(3, 3);
    
    private Board currentBoard;
    private Tuple<decimal, Axis> currentBalance;
    public Queue<Board> ExperimentQueue { get; set; } = new();
    private bool export = false;
    private int currentDepth;
    private bool running;
    public bool Running
    {
        get => running;
        private set
        {
            running = value;
            
            if (!value && ExperimentQueue.Count > 0)
            {
                RunNew(ExperimentQueue.Dequeue());
            }
            else if (!value && ExperimentQueue.Count == 0 && export)
            {
                Export(currentDepth);
            }
        }
    }

    public FileJuggler passResult = new();
    public FileJuggler fallResult = new();

    public void RunNew(Board board)
    {
        if (Running)
        {
            ExperimentQueue.Enqueue(board);
            return;
        }
        
        Running = true;

        currentBoard = board;
        
        PlaceTower(board.Tower);
        
        currentBalance = Balancer.Calculate(board);
        
        Debug.Log($"{currentBalance.Item1}, {currentBalance.Item2}");
    }
    
    public void MotionFinished(object sender, MotionFinishedEventArgs e)
    {
        if (e.BlockState.testState == TestState.PASS)
        { StatePass(e.BlockState); }
        
        else
        { StateFallen(e.BlockState); }
    }

    public void StatePass(BlockState blockState)
    {
        FinishedCount++;
        if (FinishedCount >= Tower.Count)
        {
            //Debug.Log($"Survived, last block: {blockState.axis}, {blockState.index}");
            
            passResult.UpdateBuffer(currentBoard, blockState, currentBalance);

            StartCoroutine(ResetExperiment());
        }
    }

    public void StateFallen(BlockState blockState)
    {
        //Debug.Log($"Dead, last block: {blockState.axis}, {blockState.index}");

        fallResult.UpdateBuffer(currentBoard, blockState, currentBalance);
        
        StartCoroutine(ResetExperiment());
    }

    public IEnumerator ResetExperiment()
    {
        Balancer.Reset();
        
        DestroyTower();
        yield return new WaitForFixedUpdate();
        
        Running = false;
    }

    public List<Board> Import(int depth)
    {
        return FileJuggler.Import($"/home/cinnamon/Projects/Zenga/DepthSearch/depth_{depth}_pass.csv");
    }

    public void Export(int depth)
    {
        passResult.FlushBuffer($"/home/cinnamon/Projects/Zenga/DepthSearch/depth_{depth}_pass.csv");
        fallResult.FlushBuffer($"/home/cinnamon/Projects/Zenga/DepthSearch/depth_{depth}_fall.csv");

        export = false;
    }

    public void TestDepth(int depth)
    {
        foreach (var board in Import(depth))
        {
            LayerExpander next = new(board);
            next.ExpandPosition();

            foreach (var nextBoard in next.ExpandedOnce)
            {
                RunNew(nextBoard);
            }
        }

        currentDepth = depth + 1;
        export = true;
    }
}

public class BlockStateBuilder
{
    private Vector3 blockScale;
    private int towerWidth;
    public float endBlockPos;
    public float endBlockY;
    
    public BlockStateBuilder(Vector3 blockScale, int towerWidth)
    {
        this.blockScale = blockScale;
        this.towerWidth = towerWidth;
        this.endBlockPos = (towerWidth - 1) * blockScale.x / 2f;
        this.endBlockY = blockScale.y / 2f;
    }

    public List<BlockState> BuildLayerIdentity(byte layer, int index, Axis axis) // height by index
    {
        List<BlockState> blockStates = new();
        
        for (int i = 0; i < 8; i++)
        {
            if ((layer >> i) % 2 != 1) continue;
            
            blockStates.Add(new(index, (byte)(1 << i), axis));
        }

        return blockStates;
    }

    public List<BlockState> BuildNeighborBlocks(BlockState block)
    {
        List<BlockState> blockStates = new();
        
        byte layer = (byte)((1 << (towerWidth - 1)) - 1);

        blockStates.AddRange(BuildLayerIdentity(layer, block.index + 1, block.axis.Cycle()));     // upper
        blockStates.AddRange(BuildLayerIdentity(layer, block.index, block.axis));                 // this
        blockStates.AddRange(BuildLayerIdentity(layer, block.index - 1, block.axis.Cycle()));     // under

        blockStates.Remove(block);

        return blockStates;
    }
}
