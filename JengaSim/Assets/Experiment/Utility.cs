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

    public class BFSFileHelper
    {
        public Dictionary<string, string[]> FallenBuffer = new();
        public Dictionary<string, string[]> SurvivedBuffer = new();
    
        public const string SearchDir = "/";
        public const string Indexes = "board,cog_axis,cog_value,unity_time,unity_state,unity_axis,unity_mav,unity_mlv\n";

        public static string GetMeasurementFilePath(int depth, int sector, bool fallen) => fallen
            ? $"d{depth}/measurement_fall_{sector}.csv"
            : $"d{depth}/measurement_survive_{sector}.csv";
        
        public string CreateMeasurementFile(int depth, int sector, bool fallen)
        {
            string name = GetMeasurementFilePath(depth, sector, fallen);
            Directory.CreateDirectory(Path.GetDirectoryName(SearchDir + name) ?? "");
            File.WriteAllText(SearchDir + name, Indexes);

            return name;
        }
        
        public void FlushBuffer(int depth, bool fallen)
        {
            int count = 0;
            int sector = 0;
            string measurementFileName = CreateMeasurementFile(depth, sector, fallen);

            var buffer = fallen ? FallenBuffer : SurvivedBuffer;
            
            foreach (var key in buffer.Keys)
            {
                if (count >= 10000)
                {
                    count = 0;
                    sector++;
                    measurementFileName = CreateMeasurementFile(depth, sector, fallen);
                }
                
                File.AppendAllText(SearchDir + measurementFileName, $"{key},{string.Join(',', buffer[key])}" + '\n');
           
                count++;
            }

            buffer.Clear();
        }

        public void UpdateBuffer(Board board, (Balance, BlockState) result, Board motherBoard)
        {
            string key = board.BoardToString();

            var buffer = result.Item2.testState != TestStates.SLEEP ? FallenBuffer : SurvivedBuffer;

            if (buffer.ContainsKey(key)) return;
            
            string[] measurement =
            new[] {
                result.Item1.axis.ToString(),
                result.Item1.value.ToString(),
                result.Item2.fixedTime.ToString(),
                result.Item2.testState.ToString(),
                result.Item2.axis.Cycle().ToString(),   // cycle to represent rotation direction
                result.Item2.maxAngularVelocity.ToString(),
                result.Item2.maxLinearVelocity.ToString()
            };
            
            buffer.Add(key, measurement);
        }
        
        public List<Board> Import(int depth, int sector, bool fallen)    // imports measurements
        {
            if (!File.Exists(SearchDir + GetMeasurementFilePath(depth, sector, fallen))) return new();
            
            List<Board> boards = new();
            string[] file = File.ReadAllLines(SearchDir + GetMeasurementFilePath(depth, sector, fallen));

            foreach (var entry in file)
            {
                if (entry == Indexes.TrimEnd()) continue;
                
                boards.Add(entry.Split(',').First().StringToBoard());
            }

            return boards;
        }

        public int GetSectorCount(int depth, bool fallen)
        {
            int count = 0;
            
            while (File.Exists(SearchDir + GetMeasurementFilePath(depth, count, fallen)))
            { count++; }

            return count;
        }
    }
}