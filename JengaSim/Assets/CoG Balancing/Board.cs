// modified version for unity

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
        this.Width = Width; // <= 8 (byte)
        this.maxHeight = this.Width * this.Height;
        
        this.Tower = new byte[maxHeight];
        // heightIndex set on setter of Tower
    }
    
    public void InitPos()
    {
        byte layer = (byte)((1 << Width) - 1);  // this is ok since bitwise opperation defaults to int. (byte)(256 - 1)
        
        for (int i = 0; i < Height; i++)
        {
            Tower[i] = layer;
        }

        heightIndex = Height - 1;
    }
    
    public void RemoveBlock(byte block, int index)
    {
        Tower[index].RefRemoveBlock(block);
        UpdateHeightIndexRemoved(index);
    }
    
    public void PlaceBlock(byte block, int index)
    {
        Tower[index].RefPlaceBlock(block);
        UpdateHeightIndexPlaced(index);
    }

    public void ApplyMove(Move move)
    {
        RemoveBlock(move.removing.block, move.removing.index);
        PlaceBlock(move.adding.block, move.adding.index);
    }

    public void ApplyMove(IEnumerable<Move> moves)
    {
        foreach (var move in moves)
        {
            ApplyMove(move);
        }
    }

    public void UndoMove(Move move)
    {
        RemoveBlock(move.adding.block, move.adding.index);
        PlaceBlock(move.removing.block, move.removing.index);
    }

    public void UndoMove(IEnumerable<Move> moves)
    {
        foreach (var move in moves)
        {
            UndoMove(move);
        }
    }

    public int GetHeightIndex()
    {
        for (int i = maxHeight - 1; i >= 0; i--)
        {
            if (Tower[i] != 0) return i;
        }

        return 0;   // set default variable for maxHeight if -1 is correct
    }

    public void UpdateHeightIndexPlaced(int index) // accounts the highest layer with at least 1 block.
    {
        if (index > heightIndex && Tower[index] != 0)
        {
            heightIndex = index;
        }
    }

    public void UpdateHeightIndexRemoved(int index)
    {
        if (index == heightIndex && Tower[index] == 0)
        {
            heightIndex -= index > 0 ? 1 : 0;   // don't decrease index if heightIndex = 0
        }
    }
}

public struct Move
{
    public (byte block, int index) removing;
    public (byte block, int index) adding;

    public Move((byte, int) removing, (byte, int) adding)
    {
        this.removing = removing;
        this.adding = adding;
    }

    public static readonly (byte, int) None = (0, 0);
}

public static class BlockExt
{
    public static byte RemoveBlock(this byte layer, byte block)
    {
        return (byte)(layer & ~block);
    }

    public static void RefRemoveBlock(ref this byte layer, byte block)
    {
        layer = (byte)(layer & ~block);
    }
    
    public static byte PlaceBlock(this byte layer, byte block)
    {
        return (byte)(layer | block);
    }

    public static void RefPlaceBlock(ref this byte layer, byte block)
    {
        layer = (byte)(layer | block);
    }
    
    public static bool ValidateRemove(this byte towerBlock, byte movingBlock) // 0, 1 => false
    {
        return (byte)(towerBlock & movingBlock) == movingBlock;
    }
    
    public static bool ValidatePlace(this byte layer, byte block)  // 1, 1 => false
    {
        return (layer & block) == 0;
    }
}

public enum Axis
{
    NONE = 0,
    X = 1,
    Z = 2,
}

public static class AxisMethods
{
    public static Axis Cycle(this Axis axis)
    {
        return axis == Axis.Z ? Axis.X : Axis.Z;
    }
    
    public static void RefCycle(ref this Axis axis)
    {
        axis = axis == Axis.Z ? Axis.X : Axis.Z;
    }
}