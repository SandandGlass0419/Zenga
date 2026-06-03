using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LayerExpander
{
    public List<Board> ExpandedOnce = new();
    public Board motherBoard;

    public LayerExpander(Board board)
    {
        this.motherBoard = board;
    }
    
    public void ExpandPosition()
    {
        for (int i = 0; i <= motherBoard.heightIndex; i++)
        {
            ExpandedOnce.AddRange(ApplyLayers(motherBoard, ExpandLayer(motherBoard.Tower[i]), i));
        }
    }
    
    public List<Board> ApplyLayers(Board board, List<byte> layers, int layerIndex)
    {
        List<Board> newBoards = new();

        foreach (var layer in layers)
        {
            byte[] newTower = board.Tower.ToArray();    // deep copy
            newTower[layerIndex] = layer;
            
            Board newBoard = new(board.height, board.width) { Tower = newTower };
            
            newBoards.Add(newBoard);
        }

        return newBoards;
    }

    public List<byte> ExpandLayer(byte layer)
    {
        List<byte> expanded = new();
        
        for (int i = 0; i < motherBoard.width; i++)
        {
            byte move = (byte)(1 << i);
            byte newLayer = layer.RemoveBlock(move);

            if (newLayer != layer)
            {
                expanded.Add(newLayer);
            }
        }

        return expanded;
    }
}

public class FileJuggler
{
    Dictionary<string, string> Buffer = new();
    
    public void FlushBuffer(string path)
    {
        File.AppendAllText(path, String.Empty);
        
        foreach (var keys in Buffer.Keys)
        {
            File.AppendAllText(path, Buffer[keys] + '\n');
        }

        Buffer.Clear();
    }

    public void UpdateBuffer(Board board, BlockState state, Balance balance)
    {
        string key = board.ToString();
        string value = $"{key},{balance.value},{balance.axis},{state.time},{state.linearVelocity},{state.maxLinearVelocity},{state.angularVelocity},{state.maxAngularVelocity}";

        if (Buffer.ContainsKey(key)) return;
        
        Buffer.Add(key, value);
    }

    public static List<Board> Import(string path)
    {
        List<Board> boards = new();
        string[] file = File.ReadAllLines(path);

        foreach (var entry in file)
        {
            boards.Add(entry.Split(',').First().ToBoard());
        }

        return boards;
    }
}
