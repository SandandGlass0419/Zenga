using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class GenerateNext
{
    public List<Board> ExpandedOnce = new();
    public readonly Board motherBoard;

    public GenerateNext(Board board)
    {
        this.motherBoard = board;
    }
    
    public void ExpandPosition()
    {
        for (int i = 0; i <= motherBoard.heightIndex; i++)
        {
            ExpandedOnce.AddRange(ApplyLayer(motherBoard, ExpandLayer(motherBoard.Tower[i], i), i));
        }
    }

    public List<byte> ExpandLayer(byte layer, int layerIndex)
    {
        List<byte> expanded = new();
        
        for (int i = 0; i < motherBoard.Width; i++)
        {
            BlockMove move = new((byte)(1 << i), layerIndex);
            byte newLayer = move.RemoveFrom(layer);

            if (newLayer != layer)
            {
                expanded.Add(newLayer);
            }
        }

        return expanded;
    }

    public List<Board> ApplyLayer(Board board, List<byte> layers, int layerIndex)
    {
        List<Board> newBoards = new();

        foreach (var layer in layers)
        {
            byte[] newTower = board.Tower.ToArray();
            newTower[layerIndex] = layer;
            
            Board newBoard = new(board.Height, board.Width) { Tower = newTower };
            
            newBoards.Add(newBoard);
        }

        return newBoards;
    }
}

public class FileJuggler
{
    Dictionary<string, string[]> Buffer = new();
    
    public void FlushBuffer(string path)
    {
        File.AppendAllText(path, String.Empty);
        
        foreach (var keys in Buffer.Keys)
        {
            File.AppendAllText(path, String.Join(',', Buffer[keys]) + '\n');
        }

        Buffer.Clear();
    }

    public void UpdateBuffer(Board board, BlockState state, Tuple<decimal, Axis> balance)
    {
        string key = board.BoardToString();
        string[] value = new[] { key, state.time.ToString(), balance.Item1.ToString() };

        if (Buffer.ContainsKey(key)) return;
        
        Buffer.Add(key, value);
    }

    public static List<Board> Import(string path)
    {
        List<Board> Mothers = new();
        string[] file = File.ReadAllLines(path);

        foreach (var entry in file)
        {
            Mothers.Add(entry.Split(',').First().StringToBoard());
        }

        return Mothers;
    }
}
