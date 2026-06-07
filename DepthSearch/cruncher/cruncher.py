import pandas as pd
import matplotlib.pyplot as plt
import os.path

search_dir = "/home/cinnamon/Projects/Zenga/DepthSearch/zenga9a/"

def scatter(xname, yname, file_paths):
    colors = ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', '#9467bd', '#8c564b', '#e377c2', '#7f7f7f', '#bcbd22', '#17becf']
    
    d = 1
    for depth in file_paths:
        for path in depth:
            file = pd.read_csv(path)
            plt.scatter(file[xname], abs(file[yname]), color=colors[d - 1])
            
            plt.xlim(0, 2)
            plt.title(f"Depth: {d}")
            plt.savefig(f"/home/cinnamon/Projects/Zenga/DepthSearch/cruncher/plot_fall_d{d}_magnified.png")
            plt.clf()

        d += 1

def get_name(depth, sector, fallen):
    if fallen:
        return f"d{depth}/measurement_fall_{sector}.csv"
    else:
        return f"d{depth}/measurement_survive_{sector}.csv"

def get_depth_path(depth, fallen):
    count = 0

    path = search_dir + get_name(depth, count, fallen)
    paths = []

    while os.path.isfile(path):
        paths.append(path)
        count += 1
        path = search_dir + get_name(depth, count, fallen)

    return paths

def get_file_paths(maxdepth, fallen): # from depth 1
    depths = []

    for d in range(1, maxdepth + 1):
        depths.append(get_depth_path(d, fallen))

    return depths

scatter("unity_time", "cog_value", get_file_paths(5, True))