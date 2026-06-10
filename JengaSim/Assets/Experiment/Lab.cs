using System;
using System.Collections.Generic;
using Balancing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Experiment
{
    public class Lab : MonoBehaviour
    {
        public const string ExperimentDir = "Scenes/";
        public const string ExperimentName = "Experiment";

        public BFSFileHelper bfsFileHelper = new();
        
        public async void Awake()
        {
            Time.timeScale = 100f;

            for (int d = 0; d < 18; d++) 
            { 
                await SearchNext(d); 
                Console.WriteLine($"Finished {d} at {DateTime.Now}");
            }
        }
        
        public async Awaitable SearchNext(int depth)
        {
            for (int sector = 0; sector < bfsFileHelper.GetSectorCount(depth, false); sector++)
            {
                await SearchSector(depth, sector);
            }
            
            bfsFileHelper.FlushBuffer(depth + 1, true);
            bfsFileHelper.FlushBuffer(depth + 1, false);
        }

        public async Awaitable SearchSector(int depth, int sector)
        {
            List<Board> mother = bfsFileHelper.Import(depth, sector, false);
            foreach (var board in mother)
            {
                BoardExpander expander = new(board);
                expander.ExpandPosition();

                foreach (var newBoard in expander.ExpandedOnce)
                {
                    bfsFileHelper.UpdateBuffer(newBoard, await Measure(newBoard), expander.motherBoard);
                }
            }
        }

        public async Awaitable<(Balance, BlockState)> Measure(Board board)
        {
            try
            {
                await SceneManager.LoadSceneAsync(ExperimentDir + ExperimentName, LoadSceneMode.Additive);
                var measurement = await FindFirstObjectByType<Experiment>().RunAsync(board);
                await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(ExperimentName));
                
                return measurement;
            }
            catch (Exception e)
            {
                Debug.Log($"board {board.BoardToString()}: {e}");
                throw;
            }
        }
    }
}
