using AlgorithmVisualizer.Algorithms.Pathfinding;

namespace AlgorithmVisualizer.Tests;

public class PathfindingTests
{
    private static List<Point> Run(PathAlgorithm algo, Grid grid) =>
        algo.Find(grid).Where(s => s.Kind == PathStepKind.Path).Select(s => s.Cell).ToList();

    private static int Cost(Grid grid, List<Point> path) => path.Skip(1).Sum(grid.Cost);

    private static Grid RandomGrid(int seed)
    {
        var rng = new Random(seed);
        var grid = new Grid(15, 25);
        for (int r = 0; r < grid.Rows; r++)
            for (int c = 0; c < grid.Cols; c++)
            {
                var p = new Point(r, c);
                if (p == grid.Start || p == grid.End) continue;
                double x = rng.NextDouble();
                grid.Cells[r, c] = x < 0.25 ? CellType.Wall : x < 0.40 ? CellType.Weight : CellType.Empty;
            }
        return grid;
    }

    [Fact]
    public void Open_grid_path_is_straight_line()
    {
        var grid = new Grid(5, 10) { Start = new(2, 0), End = new(2, 9) };
        foreach (var algo in PathAlgorithms.All.Where(a => a.Optimal))
            Assert.Equal(10, Run(algo, grid).Count);
    }

    [Fact]
    public void Dijkstra_and_AStar_find_the_same_minimal_cost()
    {
        for (int seed = 0; seed < 50; seed++)
        {
            var grid = RandomGrid(seed);
            var d = Run(new Dijkstra(), grid);
            var a = Run(new AStar(), grid);
            Assert.Equal(d.Count == 0, a.Count == 0);
            if (d.Count > 0) Assert.Equal(Cost(grid, d), Cost(grid, a));
        }
    }

    [Fact]
    public void Dijkstra_prefers_going_around_weights()
    {
        var grid = new Grid(3, 5) { Start = new(1, 0), End = new(1, 4) };
        grid.Cells[1, 2] = CellType.Weight;
        var path = Run(new Dijkstra(), grid);
        Assert.Equal(6, Cost(grid, path)); // detour: 6 steps of cost 1 beats 3 + 5
        Assert.DoesNotContain(new Point(1, 2), path);
    }

    [Fact]
    public void BFS_finds_fewest_steps()
    {
        for (int seed = 0; seed < 50; seed++)
        {
            var grid = RandomGrid(seed);
            var bfs = Run(new BreadthFirst(), grid);
            var dij = Run(new Dijkstra(), grid);
            Assert.Equal(dij.Count == 0, bfs.Count == 0);
            if (bfs.Count > 0) Assert.True(bfs.Count <= dij.Count);
        }
    }

    [Fact]
    public void All_algorithms_agree_on_reachability_and_produce_valid_paths()
    {
        for (int seed = 0; seed < 50; seed++)
        {
            var grid = RandomGrid(seed);
            bool reachable = Run(new BreadthFirst(), grid).Count > 0;
            foreach (var algo in PathAlgorithms.All)
            {
                var path = Run(algo, grid);
                Assert.Equal(reachable, path.Count > 0);
                if (path.Count == 0) continue;
                Assert.Equal(grid.Start, path[0]);
                Assert.Equal(grid.End, path[^1]);
                for (int i = 1; i < path.Count; i++)
                {
                    Assert.Equal(1, Math.Abs(path[i].Row - path[i - 1].Row) + Math.Abs(path[i].Col - path[i - 1].Col));
                    Assert.NotEqual(CellType.Wall, grid.Cells[path[i].Row, path[i].Col]);
                }
            }
        }
    }

    [Fact]
    public void Walled_in_target_yields_no_path()
    {
        var grid = new Grid(5, 5) { Start = new(0, 0), End = new(4, 4) };
        grid.Cells[3, 4] = grid.Cells[4, 3] = CellType.Wall;
        foreach (var algo in PathAlgorithms.All)
            Assert.Empty(Run(algo, grid));
    }
}
