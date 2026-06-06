using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Balancing;

namespace Experiment
{
    public class BoardExpander
    {
        public List<Board> ExpandedOnce = new();
        public Board motherBoard;

        public BoardExpander(Board board)
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

                if (newLayer != layer && newLayer != 0)
                {
                    expanded.Add(newLayer);
                }
            }

            return expanded;
        }
    }

    public class FileHelper
    {
        public Dictionary<string, (string[] measurement, List<string> mother)> FallenBuffer = new();
        public Dictionary<string, (string[] measurement, List<string> mother)> SurvivedBuffer = new();
    
        public const string SearchDir = "/home/cinnamon/Projects/Zenga/DepthSearch/";    // will be set soon

        public static string GetMeasurementFilePath(int depth, int sector, bool fallen) => fallen
            ? $"d{depth}/measurement_fall_{sector}.csv"
            : $"d{depth}/measurement_survive_{sector}.csv";
        
        public string CreateMeasurementFile(int depth, int sector, bool fallen)
        {
            string name = GetMeasurementFilePath(depth, sector, fallen);
            Directory.CreateDirectory(Path.GetDirectoryName(SearchDir + name) ?? "");
            File.WriteAllText(SearchDir + name, String.Empty);

            return name;
        }

        public static string GetDepthMapFilePath(int depth, int sector, bool fallen) => fallen
            ? $"d{depth}/depthmap_fall_{sector}.csv"
            : $"d{depth}/depthmap_survive_{sector}.csv";
        
        public string CreateDepthMapFile(int depth, int sector, bool fallen)
        {
            string name = GetDepthMapFilePath(depth, sector, fallen);
            Directory.CreateDirectory(Path.GetDirectoryName(SearchDir + name) ?? "");
            File.WriteAllText(SearchDir + name, String.Empty);

            return name;
        }
        
        public void FlushBuffer(int depth, bool fallen)
        {
            int count = 0;
            int sector = 0;
            string measurementFileName = CreateMeasurementFile(depth, sector, fallen);
            //string depthmapFileName = CreateDepthMapFile(depth, sector, fallen);

            var buffer = fallen ? FallenBuffer : SurvivedBuffer;
            
            foreach (var key in buffer.Keys)
            {
                if (count >= 10000)
                {
                    count = 0;
                    sector++;
                    measurementFileName = CreateMeasurementFile(depth, sector, fallen);
                    //depthmapFileName = CreateDepthMapFile(depth, sector, fallen);
                }
                
                File.AppendAllText(SearchDir + measurementFileName, $"{key},{string.Join(',', buffer[key].measurement)}" + '\n');
                //File.AppendAllText(SearchDir + depthmapFileName, $"{key},{string.Join(',', buffer[key].mother)}" + '\n');
                
                count++;
            }

            buffer.Clear();
        }

        public void UpdateBuffer(Board board, (Balance, BlockState) result, Board motherBoard)
        {
            string key = board.BoardToString();

            var buffer = result.Item2.testState != TestStates.SLEEP ? FallenBuffer : SurvivedBuffer;
            
            if (buffer.ContainsKey(key))
            {
                //buffer[key].mother.Add(motherBoard.BoardToString());
                return;
            }
            
            string[] measurement =
            new[] {
                result.Item1.axis.ToString(),
                result.Item1.value.ToString(),
                result.Item2.fixedTime.ToString(),
                result.Item2.testState.ToString(),
                result.Item2.axis.Cycle().ToString(),   // cycle to represent rotation direction
                result.Item2.angularVelocity.ToString(),
                result.Item2.maxAngularVelocity.ToString(),
                result.Item2.linearVelocity.ToString(),
                result.Item2.maxLinearVelocity.ToString()
            };

            //List<string> mother = new List<string>() { motherBoard.BoardToString() };
            List<string> mother = new();
            
            buffer.Add(key, (measurement, mother));
        }
        
        public List<Board> Import(int depth, int sector, bool fallen)    // imports measurements
        {
            if (!File.Exists(SearchDir + GetMeasurementFilePath(depth, sector, fallen))) return new();
            
            List<Board> boards = new();
            string[] file = File.ReadAllLines(SearchDir + GetMeasurementFilePath(depth, sector, fallen));

            foreach (var entry in file)
            {
                boards.Add(entry.Split(',').First().StringToBoard());
            }

            return boards;
        }

        public int GetSectorCount(int depth, bool fallen)
        {
            int count = 0;
            
            while (true)
            {
                if (!File.Exists(SearchDir + GetMeasurementFilePath(depth, count, fallen))) 
                    return count == 0 ? 0 : count;
                
                count++;
            }
        }
    }
}