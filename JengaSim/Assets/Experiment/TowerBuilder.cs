using System;
using System.Collections.Generic;
using Balancing;
using UnityEngine;

namespace Experiment
{
    public class TowerBuilder : MonoBehaviour
    {
        public GameObject BlockPrefab;
        public BlockStateBuilder BlockStateBuilder;
        public List<GameObject> Tower;

        public void Initialize(int width)
        {
            this.BlockStateBuilder = new(BlockPrefab.transform.localScale, width);
            this.Tower = new();
        }
    
        public void PlaceLayer(byte layer, int index, Axis axis, EventHandler<ActionEventArgs> OnFinished)
        {
            foreach (var blockAttribute in BlockStateBuilder.BuildLayerBlockState(layer, index, axis))
            {
                PlaceBlock(blockAttribute, OnFinished);
            }
        }
    
        public void PlaceBlock(BlockState block, EventHandler<ActionEventArgs> OnFinished)
        {
            var instBlock = Instantiate(BlockPrefab, block.pos, block.rotation);
            instBlock.GetComponent<BlockObserver>().Initialize(block, BlockStateBuilder.BuildNeighborBlocks(block));
            instBlock.GetComponent<BlockObserver>().MotionFinishedEvent += OnFinished;

            Tower.Add(instBlock);
        }

        public void PlaceTower(Board board, EventHandler<ActionEventArgs> OnFinished)
        {
            Axis axis = Axis.X;
        
            for (int i = 0; i < board.Tower.Length; i++)
            {
                PlaceLayer(board.Tower[i], i, axis, OnFinished);
                axis = axis.Cycle();
            }
        }

        public void DestroyTower()
        {
            Tower.ForEach(Destroy);
            Tower.Clear();
        }
    }

    public class BlockStateBuilder
    {
        private Vector3 blockScale;
        private int width;
        public float endBlockPos;
        public float endBlockY;
    
        public BlockStateBuilder(Vector3 blockScale, int width)
        {
            this.blockScale = blockScale;
            this.width = width;
            this.endBlockPos = (width - 1) * blockScale.x / 2f;
            this.endBlockY = blockScale.y / 2f;
        }

        public List<BlockState> BuildLayerBlockState(byte layer, int index, Axis axis) // height by index
        {
            List<BlockState> blockStates = new();
        
            for (int i = 0; i < 8; i++)
            {
                if ((layer >> i) % 2 != 1) continue;

                BlockState identity = new(index, (byte)(1 << i), axis);
            
                identity.pos = GetPosition(axis, index, i);
                identity.rotation = GetQuaternion(axis);
            
                blockStates.Add(identity);
            }

            return blockStates;
        }

        public List<BlockState> BuildNeighborBlocks(BlockState block)
        {
            List<BlockState> blockStates = new();
        
            byte layer = (byte)((1 << width) - 1);

            blockStates.AddRange(BuildLayerBlockState(layer, block.index + 1, block.axis.Cycle()));     // upper
            blockStates.AddRange(BuildLayerBlockState(layer, block.index, block.axis));                 // this
            blockStates.AddRange(BuildLayerBlockState(layer, block.index - 1, block.axis.Cycle()));     // under

            blockStates.Remove(block);

            return blockStates;
        }
    
        public Vector3 GetPosition(Axis axis, int index, int blockIndex)
        {
            Vector3 basePos = new(0, endBlockY + index * blockScale.y, 0);
            return basePos + (axis == Axis.Z ? new(0, 0, endBlockPos - blockIndex) : new(endBlockPos - blockIndex, 0, 0));
        }
    
        public Quaternion GetQuaternion(Axis axis)
        {
            return axis == Axis.Z ? Quaternion.Euler(0, -90, 0) : Quaternion.identity;
        }
    }
}