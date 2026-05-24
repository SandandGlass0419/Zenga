using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class NewEmptyCSharpScript : MonoBehaviour
{
    public GameObject BlockPrefab;
    public PositionManager Manager;

    public void Awake()
    {
        Manager = new(BlockPrefab.transform.localScale, 3);
    }

    public void PlaceLayer(byte layer, int index, Axis axis)
    {
        foreach (var block in Manager.GetLayer(layer, index, axis))
        {
            Instantiate(BlockPrefab, block.Item1, block.Item2);
        }
    }

    public void PlaceTower(byte[] tower)
    {
        Axis axis = Axis.X;
        
        for (int i = 0; i < tower.Length; i++)
        {
            PlaceLayer(tower[i], i, axis);
            axis = axis == Axis.Z ? Axis.X : Axis.Z;
        }
    }
}

public class PositionManager
{
    public readonly Vector3 blockScale;
    public readonly int towerWidth;

    public readonly float endBlockPos;
    public readonly float endBlockHeight;
    
    public PositionManager(Vector3 blockScale, int towerWidth)
    {
        this.blockScale = blockScale;
        this.towerWidth = towerWidth;

        this.endBlockPos = (towerWidth - 1) * blockScale.x / 2f;
        this.endBlockHeight = blockScale.y / 2;
    }

    public List<(Vector3 pos, Quaternion quat)> GetLayer(byte layer, int index, Axis axis) // height by index
    {
        List<(Vector3, Quaternion)> transforms = new();
        
        for (int i = 0; i < 8; i++)
        {
            if ((layer >> i) % 2 != 1) continue;

            Vector3 newPos = new(0, endBlockHeight + index * blockScale.y, 0);
            
            newPos += axis == Axis.Z ? new(0, 0, endBlockPos - i) : new(endBlockPos - i, 0, 0);
            Quaternion quat = axis == Axis.Z ? Quaternion.Euler(0, -90, 0) : Quaternion.identity;
            
            transforms.Add((newPos, quat));
        }

        return transforms;
    }
}
