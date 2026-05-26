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

        Time.timeScale = 1f;
        
        RunNew(new byte[] {5, 6, 2, 5, 3, 3, 1, 0, 0});
        RunNew(new byte[] {0b101, 0b011, 0b101, 0b010, 0, 0, 0, 0, 0});
        RunNew(new byte[] {2,2,2,2,2,2,2,2,2});
    }
    
    public void PlaceLayer(byte layer, int index, Axis axis)
    {
        foreach (var blockAttribute in BlockStateBuilder.BuildLayerBlockState(layer, index, axis))
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

    public Queue<byte[]> ExperimentQueue { get; set; } = new();
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
        }
    }

    public void RunNew(byte[] tower)
    {
        if (Running)
        {
            ExperimentQueue.Enqueue(tower);
            return;
        }
        
        Running = true;
        
        PlaceTower(tower);

        Board board = new(3, 3) { Tower = tower };
        decimal lastCoGBalance = Balancer.Calculate(board);
        Balancer.Reset();
        
        Debug.Log($"{lastCoGBalance}");
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
            Debug.Log($"Survived, last block: {blockState.axis}, {blockState.index}");// Export

            StartCoroutine(ResetExperiment());
        }
    }

    public void StateFallen(BlockState blockState)
    {
        Debug.Log($"Dead, last block: {blockState.axis}, {blockState.index}");// Export

        StartCoroutine(ResetExperiment());
    }

    public IEnumerator ResetExperiment()
    {
        DestroyTower();
        yield return new WaitForFixedUpdate();
        Running = false;
    }
}

public class BlockStateBuilder
{
    private Vector3 blockScale;
    public Vector3 BlockScale
    {
        get { return blockScale; }
        set
        {
            blockScale = value;
            endBlockPos = (towerWidth - 1) * value.x / 2f;
            endBlockY = value.y / 2;
        }
    }

    private int towerWidth;
    public int TowerWidth
    {
        get { return towerWidth; }
        set
        {
            towerWidth = value;
            endBlockPos = (value - 1) * blockScale.x / 2f;
        }
    }

    public float endBlockPos { get; private set; }
    public float endBlockY { get; private set; }
    
    public BlockStateBuilder(Vector3 blockScale, int towerWidth)
    {
        this.BlockScale = blockScale;
        this.TowerWidth = towerWidth;
    }

    public List<BlockState> BuildLayerBlockState(byte layer, int index, Axis axis) // height by index
    {
        List<BlockState> blockStates = new();
        
        for (int i = 0; i < 8; i++)
        {
            if ((layer >> i) % 2 != 1) continue;

            Vector3 basePos = new(0, endBlockY + index * blockScale.y, 0);
            Vector3 newPos = GetPosition(axis, basePos, endBlockPos - i);

            Quaternion quat = GetQuaternion(axis);
            
            blockStates.Add(new(index, axis, newPos, quat));
        }

        return blockStates;
    }

    public List<BlockState> BuildNeighborBlocks(BlockState block)
    {
        List<BlockState> blockStates = new();
        
        byte layer = (byte)(Math.Pow(2, TowerWidth) - 1);

        blockStates.AddRange(BuildLayerBlockState(layer, block.index + 1, block.axis.Cycle()));     // upper
        blockStates.AddRange(BuildLayerBlockState(layer, block.index, block.axis));                 // this
        blockStates.AddRange(BuildLayerBlockState(layer, block.index - 1, block.axis.Cycle()));     // under

        blockStates.Remove(block);

        return blockStates;
    }

    public Quaternion GetQuaternion(Axis axis)
    {
        return axis == Axis.Z ? Quaternion.Euler(0, -90, 0) : Quaternion.identity;
    }

    public Vector3 GetPosition(Axis axis, Vector3 basePos, float size)
    {
        return basePos + (axis == Axis.Z ? new(0, 0, size) : new(size, 0, 0));
    }
}
