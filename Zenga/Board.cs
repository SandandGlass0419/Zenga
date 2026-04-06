using System.Runtime.CompilerServices;

namespace Zenga;

public class Board
{
    public byte[] Tower { get; init; }

    public int HeightIndex;
    
    public readonly int Height;
    public readonly int Width;
    public readonly int maxHeight;

    public Side side;

    public Board(int height = 18, int Width = 3, Side side = Side.W)
    {
        this.Height = height;
        this.Width = Width <= 8 ? Width : 8;
        this.maxHeight = this.Width * this.Height;

        this.Tower = new byte[maxHeight];
        this.HeightIndex = 0;

        this.side = side;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveBlock(BlockMove block)
    {
        Tower[block.slotIndex] = block.RemoveFrom(Tower[block.slotIndex]);
        UpdateHeightIndexRemoved(block);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PlaceBlock(BlockMove block)
    {
        Tower[block.slotIndex] = block.PlaceTo(Tower[block.slotIndex]);
        UpdateHeightIndexPlaced(block);
    }
    
    public void InitPos()
    {
        byte layer = (byte)((1 << Width) - 1);
        
        for (int i = 0; i < Height; i++)
        {
            Tower[i] = layer;
        }

        HeightIndex = Height - 1;
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

    public bool IsBlank(BlockMove block)
    {
        return (byte)((byte)(Tower[block.slotIndex] ^ block.movingBlock) & block.movingBlock) == block.movingBlock;
    }

    public bool IsBlank(byte towerBlock, byte movingBlock)
    {
        return (byte)((byte)(towerBlock ^ movingBlock) & movingBlock) == movingBlock;
    }

    public bool IsFilled(BlockMove block)
    {
        return (byte)(Tower[block.slotIndex] & block.movingBlock) == block.movingBlock;
    }

    public bool IsFilled(byte towerBlock, byte movingBlock)
    {
        return (byte)(towerBlock & movingBlock) == movingBlock;
    }

    public int GetEffectiveIndex()
    {
        for (int i = maxHeight - 1; i >= 0; i--)
        {
            if (Tower[i] != 0) return i;
        }

        return 0;   // set default variable for maxHeight if -1 is correct
    }

    public int UpdateHeightIndexPlaced(BlockMove block) // accounts the highest layer with at least 1 block.
    {
        if (block.slotIndex > HeightIndex && Tower[block.slotIndex] != 0)
        {
            HeightIndex = block.slotIndex;
        }
        
        return HeightIndex;
    }

    public int UpdateHeightIndexRemoved(BlockMove block)
    {
        if (block.slotIndex == HeightIndex && Tower[block.slotIndex] == 0)
        {
            HeightIndex -= block.slotIndex > 0 ? 1 : 0;
        }

        return HeightIndex;
    }

    // validate position (CoG)
    // validate move (block already there?, placed in valid?) ok
}

public record Move(BlockMove RemovingBlock, BlockMove PlacingBlock);

public readonly struct BlockMove
{
    public readonly byte movingBlock;
    public readonly int slotIndex;

    public BlockMove(byte movingBlock, int slotIndex)
    {
        this.movingBlock = movingBlock;
        this.slotIndex = slotIndex;
    }

    public BlockMove(int blockIndex, int slotIndex)
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
    X = 0,
    Z = 1,
}

public enum Side
{
    W = 0,
    B = 1,
}