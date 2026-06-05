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

        public FileHelper fileHelper = new();
        
        public async void Awake()
        {
            Time.timeScale = 100f;

            int d = 0;
            try
            {
                for (d = 0; d < 36; d++)
                {
                    await SearchNext(d);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}, at depth: {d}");
            }

            Console.ReadKey();
        }

        public async Awaitable<(Balance, BlockState)> Measure(Board board)
        {
            await SceneManager.LoadSceneAsync(ExperimentDir + ExperimentName, LoadSceneMode.Additive);
            var measurement = await FindFirstObjectByType<Experiment>().RunAsync(board);
            await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(ExperimentName));

            return measurement;
        }

        public async Awaitable SearchNext(int depth)
        {
            for (int sector = 0; sector < fileHelper.GetSectorCount(depth, false); sector++)
            {
                await SearchSector(depth, sector);
            }
            
            fileHelper.FlushBuffer(depth + 1, true);
            fileHelper.FlushBuffer(depth + 1, false);
        }

        public async Awaitable SearchSector(int depth, int sector)
        {
            List<Board> mother = fileHelper.Import(depth, sector, false);
            foreach (var board in mother)
            {
                BoardExpander expander = new(board);
                expander.ExpandPosition();

                foreach (var newBoard in expander.ExpandedOnce)
                {
                    fileHelper.UpdateBuffer(newBoard, await Measure(newBoard), expander.motherBoard);
                }
            }
        }
    }
}
