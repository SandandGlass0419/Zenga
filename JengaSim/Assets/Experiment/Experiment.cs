using System;
using System.Collections.Generic;
using UnityEngine;

public partial class Experiment : MonoBehaviour
{
    public GameObject BlockPrefab;
    public GameObject PlaneObject;
    public AttributeBuilder AttributeBuilder;
    public List<GameObject> Tower;
    
    public void Awake()
    {
        AttributeBuilder = new(BlockPrefab.transform.localScale, 3);

        Time.timeScale = 1;
        
        RunNew(new byte[] {5, 6, 2, 4, 3, 3, 1, 0, 0});
    }
    
    public void PlaceLayer(byte layer, int index, Axis axis)
    {
        foreach (var blockAttribute in AttributeBuilder.BuildLayerAttributes(layer, index, axis))
        {
            PlaceBlock(blockAttribute);
        }
    }
    
    public void PlaceBlock(BlockAttribute block)
    {
        var instBlock = Instantiate(BlockPrefab, block.startPos, block.startRotation);
        instBlock.GetComponent<BlockState>().Initialize(block, AttributeBuilder.BuildNeighborAttributes(block));
        instBlock.GetComponent<BlockState>().MotionFinishedEvent += MotionFinished;

        Tower.Add(instBlock);
    }

    public void PlaceTower(byte[] tower)
    {
        Axis axis = Axis.X;
        
        for (int i = 0; i < tower.Length; i++)
        {
            PlaceLayer(tower[i], i, axis);
            axis = BlockAttribute.CycleAxis(axis);
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
    public int LeftBlocks { get; set; }
    public CoGBalancing Balancer { get; set; } = new(3, 3);
    public decimal lastCoGResult;

    public void RunNew(byte[] tower)
    {
        PlaceTower(tower);
        LeftBlocks = Tower.Count;

        Board board = new(3, 3) { Tower = tower };

        lastCoGResult = Balancer.Calculate(board);
        Debug.Log(lastCoGResult);
    }
    
    public void MotionFinished(object sender, MotionFinishedEventArgs e)
    {
        StateOK(e.Snapshot);
        StateFallen(e.Snapshot);
    }

    public void StateOK(Snapshot snapshot)
    {
        if (snapshot.state != "ok") return;
        
        LeftBlocks--;
        if (LeftBlocks <= 0)
        {
            Debug.Log("Survived");// Export
            DestroyTower();
        }
    }

    public void StateFallen(Snapshot snapshot)
    {
        if (snapshot.state == "ok") return;

        Debug.Log("Dead");// Export
        DestroyTower();
    }
}

public class AttributeBuilder
{
    private Vector3 blockScale;

    private int towerWidth;
    private float endBlockPos;
    private float endBlockY;
    
    public AttributeBuilder(Vector3 blockScale, int towerWidth)
    {
        this.blockScale = blockScale;
        this.towerWidth = towerWidth;
        
        this.endBlockPos = (towerWidth - 1) * blockScale.x / 2f;
        this.endBlockY = blockScale.y / 2;
    }

    public List<BlockAttribute> BuildLayerAttributes(byte layer, int index, Axis axis) // height by index
    {
        List<BlockAttribute> attributes = new();
        
        for (int i = 0; i < 8; i++)
        {
            if ((layer >> i) % 2 != 1) continue;

            Vector3 basePos = new(0, endBlockY + index * blockScale.y, 0);
            Vector3 newPos = GetPosition(axis, basePos, endBlockPos - i);

            Quaternion quat = GetQuaternion(axis);

            bool isSupport = index == 0;
            
            attributes.Add(new(newPos, quat, isSupport, index, axis));
        }

        return attributes;
    }

    public List<BlockAttribute> BuildNeighborAttributes(BlockAttribute block)
    {
        List<BlockAttribute> attributes = new();
        
        byte layer = (byte)(Math.Pow(2, towerWidth) - 1);
        
        attributes.AddRange(BuildLayerAttributes(layer, block.index + 1, BlockAttribute.CycleAxis(block.axis)));    // upper
        attributes.AddRange(BuildLayerAttributes(layer, block.index, block.axis));                                  // this
        attributes.AddRange(BuildLayerAttributes(layer, block.index - 1, BlockAttribute.CycleAxis(block.axis)));    // under

        attributes.Remove(block);

        return attributes;
    }

    public static Quaternion GetQuaternion(Axis axis)
    {
        return axis == Axis.Z ? Quaternion.Euler(0, -90, 0) : Quaternion.identity;
    }

    public static Vector3 GetPosition(Axis axis, Vector3 basePos, float size)
    {
        return basePos + (axis == Axis.Z ? new(0, 0, size) : new(size, 0, 0));
    }
}

public struct BlockAttribute : IEquatable<BlockAttribute>
{
    public Vector3 startPos { get; set; }
    public Quaternion startRotation { get; set; }
    public bool isSupport { get; set; }
    public int index { get; set; }
    public Axis axis { get; set; }

    public BlockAttribute(Vector3 startPos, Quaternion startRotation, bool isSupport, int index, Axis axis)
    {
        this.startPos = startPos;
        this.startRotation = startRotation;
        this.isSupport = isSupport;
        this.index = index;
        this.axis = axis;
    }

    public static Axis CycleAxis(Axis axis)
    {
        return axis == Axis.Z ? Axis.X : Axis.Z;
    }

    public bool Equals(BlockAttribute other)
    {
        return startPos.Equals(other.startPos) && startRotation.Equals(other.startRotation);
    }

    public override bool Equals(object obj)
    {
        return obj is BlockAttribute other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(startPos, startRotation);
    }
}
