using System.Runtime.CompilerServices;
using System.Collections.Generic;

public class Board
{
    private byte[] tower;
    
    public byte[] Tower
    {
        get => tower;
        set
        {
            tower = value;
            this.heightIndex = GetHeightIndex();
        }
    }

    public int heightIndex;
    
    public readonly int Height;
    public readonly int Width;
    public readonly int maxHeight;

    public Board(int height = 18, int Width = 3)
    {
        this.Height = height;
        this.Width = Width <= 8 ? Width : 8;
        this.maxHeight = this.Width * this.Height;
        
        this.Tower = new byte[maxHeight];
        // heightIndex set on init; setter
    }
    
    public void InitPos()
    {
        byte layer = (byte)((1 << Width) - 1);
        
        for (int i = 0; i < Height; i++)
        {
            Tower[i] = layer;
        }

        heightIndex = Height - 1;
    }

    public void RemoveBlock(BlockMove block)
    {
        Tower[block.slotIndex] = block.RemoveFrom(Tower[block.slotIndex]);
        UpdateHeightIndexRemoved(block);
    }

    public void PlaceBlock(BlockMove block)
    {
        Tower[block.slotIndex] = block.PlaceTo(Tower[block.slotIndex]);
        UpdateHeightIndexPlaced(block);
    }

    public void ApplyMove(BlockMove removingBlock, BlockMove placingBlock)
    {
        RemoveBlock(removingBlock);

        PlaceBlock(placingBlock);
    }

    public void ApplyMove(Move move)
    {
        ApplyMove(move.RemovingBlock, move.PlacingBlock);
    }

    public void ApplyMove(IEnumerable<Move> moves)
    {
        foreach (var move in moves)
        {
            ApplyMove(move);
        }
    }

    public void UndoMove(BlockMove removingBlock, BlockMove placingBlock)
    {
         RemoveBlock(placingBlock);
         
         PlaceBlock(removingBlock);
    }

    public void UndoMove(Move move)
    {
        UndoMove(move.RemovingBlock, move.PlacingBlock);
    }

    public void UndoMove(IEnumerable<Move> moves)
    {
        foreach (var move in moves)
        {
            UndoMove(move);
        }
    }

    public bool ValidatePlace(BlockMove block)
    {
        return ValidatePlace(Tower[block.slotIndex], block.movingBlock);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ValidatePlace(byte towerBlock, byte movingBlock)  // 1, 1 => false
    {
        return (towerBlock & movingBlock) == 0;
    }

    public bool ValidateRemove(BlockMove block)
    {
        return ValidateRemove(Tower[block.slotIndex], block.movingBlock);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ValidateRemove(byte towerBlock, byte movingBlock) // 0, 1 => false
    {
        return (byte)(towerBlock & movingBlock) == movingBlock;
    }

    public int GetHeightIndex()
    {
        for (int i = maxHeight - 1; i >= 0; i--)
        {
            if (Tower[i] != 0) return i;
        }

        return 0;   // set default variable for maxHeight if -1 is correct
    }

    public int UpdateHeightIndexPlaced(BlockMove block) // accounts the highest layer with at least 1 block.
    {
        if (block.slotIndex > heightIndex && Tower[block.slotIndex] != 0)
        {
            heightIndex = block.slotIndex;
        }
        
        return heightIndex;
    }

    public int UpdateHeightIndexRemoved(BlockMove block)
    {
        if (block.slotIndex == heightIndex && Tower[block.slotIndex] == 0)
        {
            heightIndex -= block.slotIndex > 0 ? 1 : 0;
        }

        return heightIndex;
    }

    // logic testing
    // validate position (CoG)
    // validate move (block already there?, placed in valid?) ok
}

public struct Move
{
    public BlockMove RemovingBlock { get; set; }
    public BlockMove PlacingBlock { get; set; }

    public Move(BlockMove RemovingBlock, BlockMove PlacingBlock)
    {
        this.RemovingBlock = RemovingBlock;
        this.PlacingBlock = PlacingBlock;
    }
}

public readonly struct BlockMove
{
    public readonly byte movingBlock;
    public readonly int slotIndex;

    public BlockMove(byte movingBlock, int slotIndex)
    {
        this.movingBlock = movingBlock;
        this.slotIndex = slotIndex;
    }

    public BlockMove(int blockIndex, int slotIndex) // 0-7, -1 = blank
    {
        this.movingBlock = (byte)(1 << blockIndex);
        this.slotIndex = slotIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte PlaceTo(byte block)
    {
        return (byte)(block | movingBlock);
    }

    public byte PlaceTo(ref byte block)
    {
        block = (byte)(block | movingBlock);
        return block;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte RemoveFrom(byte block)
    {
        return (byte)(block & ~movingBlock);
    }

    public byte RemoveFrom(ref byte block)
    {
        block = (byte)(block & ~movingBlock);
        return block;
    }
}

public enum Axis
{
    NONE = -1,
    X = 0,
    Z = 1,
}

public enum Side
{
    W = 0,
    B = 1,
}

public static class EnumMethods
{
    public static Axis Cycle(this Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return Axis.Z;
            case Axis.Z:
                return Axis.X;
            default:
                return Axis.NONE;
        }
    }

    public static Side Cycle(this Side side)
    {
        return side == Side.W ? Side.B : Side.W;
    }
}