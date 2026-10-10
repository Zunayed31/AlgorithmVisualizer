namespace AlgorithmVisualizer.Algorithms.Pathfinding;

public enum CellType : byte { Empty, Wall, Weight }

public readonly record struct Point(int Row, int Col);

public enum PathStepKind
{
    /// <summary>A cell was discovered and queued for later.</summary>
    Frontier,
    /// <summary>A cell was taken from the queue and expanded.</summary>
    Visit,
    /// <summary>A cell belongs to the final path (emitted start to end).</summary>
    Path,
}

public readonly record struct PathStep(PathStepKind Kind, Point Cell);

public sealed class Grid
{
    public const int WeightCost = 5;

    public int Rows { get; }
    public int Cols { get; }
    public CellType[,] Cells { get; }
    public Point Start { get; set; }
    public Point End { get; set; }

    public Grid(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        Cells = new CellType[rows, cols];
        Start = new(rows / 2, cols / 4);
        End = new(rows / 2, cols * 3 / 4);
    }

    public bool InBounds(Point p) => p.Row >= 0 && p.Row < Rows && p.Col >= 0 && p.Col < Cols;

    public int Cost(Point p) => Cells[p.Row, p.Col] == CellType.Weight ? WeightCost : 1;

    public IEnumerable<Point> Neighbors(Point p)
    {
        ReadOnlySpan<(int, int)> dirs = [(-1, 0), (0, 1), (1, 0), (0, -1)];
        var result = new List<Point>(4);
        foreach (var (dr, dc) in dirs)
        {
            var n = new Point(p.Row + dr, p.Col + dc);
            if (InBounds(n) && Cells[n.Row, n.Col] != CellType.Wall) result.Add(n);
        }
        return result;
    }

    public void Clear()
    {
        Array.Clear(Cells);
    }
}

/// <summary>
/// A path-finding algorithm expressed as a lazy sequence of steps, ending with the
/// <see cref="PathStepKind.Path"/> steps when the target is reachable.
/// </summary>
public abstract class PathAlgorithm
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract bool Weighted { get; }
    public abstract bool Optimal { get; }

    public abstract IEnumerable<PathStep> Find(Grid grid);

    protected static int Manhattan(Point a, Point b) => Math.Abs(a.Row - b.Row) + Math.Abs(a.Col - b.Col);

    protected static IEnumerable<PathStep> Trace(Point[,] previous, Grid grid)
    {
        var path = new List<Point>();
        var cur = grid.End;
        while (cur != grid.Start)
        {
            path.Add(cur);
            cur = previous[cur.Row, cur.Col];
        }
        path.Add(grid.Start);
        for (int i = path.Count - 1; i >= 0; i--) yield return new(PathStepKind.Path, path[i]);
    }

    protected static Point[,] NewPrevious(Grid grid)
    {
        var prev = new Point[grid.Rows, grid.Cols];
        for (int r = 0; r < grid.Rows; r++)
            for (int c = 0; c < grid.Cols; c++)
                prev[r, c] = new(-1, -1);
        return prev;
    }
}

public static class PathAlgorithms
{
    public static IReadOnlyList<PathAlgorithm> All { get; } =
    [
        new AStar(),
        new Dijkstra(),
        new GreedyBestFirst(),
        new BreadthFirst(),
        new DepthFirst(),
    ];

    public static PathAlgorithm Find(string? id) =>
        All.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase)) ?? All[0];
}

public sealed class Dijkstra : PathAlgorithm
{
    public override string Id => "dijkstra";
    public override string Name => "Dijkstra's Algorithm";
    public override string Description =>
        "Always expands the unvisited cell with the lowest total cost from the start. It explores outward in all directions like a ripple and is guaranteed to find the cheapest path, including around weighted cells.";
    public override bool Weighted => true;
    public override bool Optimal => true;

    public override IEnumerable<PathStep> Find(Grid grid) => Search(grid, useHeuristic: false);

    internal static IEnumerable<PathStep> Search(Grid grid, bool useHeuristic)
    {
        var dist = new int[grid.Rows, grid.Cols];
        var closed = new bool[grid.Rows, grid.Cols];
        var prev = NewPrevious(grid);
        for (int r = 0; r < grid.Rows; r++)
            for (int c = 0; c < grid.Cols; c++)
                dist[r, c] = int.MaxValue;

        var queue = new PriorityQueue<Point, (int F, int H)>();
        dist[grid.Start.Row, grid.Start.Col] = 0;
        queue.Enqueue(grid.Start, (0, 0));

        while (queue.TryDequeue(out var cur, out _))
        {
            if (closed[cur.Row, cur.Col]) continue;
            closed[cur.Row, cur.Col] = true;
            yield return new(PathStepKind.Visit, cur);

            if (cur == grid.End)
            {
                foreach (var s in Trace(prev, grid)) yield return s;
                yield break;
            }

            foreach (var n in grid.Neighbors(cur))
            {
                if (closed[n.Row, n.Col]) continue;
                int nd = dist[cur.Row, cur.Col] + grid.Cost(n);
                if (nd >= dist[n.Row, n.Col]) continue;
                dist[n.Row, n.Col] = nd;
                prev[n.Row, n.Col] = cur;
                int h = useHeuristic ? Manhattan(n, grid.End) : 0;
                queue.Enqueue(n, (nd + h, h));
                yield return new(PathStepKind.Frontier, n);
            }
        }
    }
}

public sealed class AStar : PathAlgorithm
{
    public override string Id => "astar";
    public override string Name => "A* Search";
    public override string Description =>
        "Like Dijkstra, but ranks cells by cost-so-far plus a Manhattan-distance estimate to the goal. The heuristic pulls the search toward the target, so it usually visits far fewer cells while still finding the cheapest path.";
    public override bool Weighted => true;
    public override bool Optimal => true;

    public override IEnumerable<PathStep> Find(Grid grid) => Dijkstra.Search(grid, useHeuristic: true);
}

public sealed class GreedyBestFirst : PathAlgorithm
{
    public override string Id => "greedy";
    public override string Name => "Greedy Best-First";
    public override string Description =>
        "Only looks at the estimated distance to the goal and ignores the cost travelled so far. It is very fast and heads straight for the target, but can be led into dead ends and does not guarantee the shortest path.";
    public override bool Weighted => false;
    public override bool Optimal => false;

    public override IEnumerable<PathStep> Find(Grid grid)
    {
        var seen = new bool[grid.Rows, grid.Cols];
        var prev = NewPrevious(grid);
        var queue = new PriorityQueue<Point, int>();
        queue.Enqueue(grid.Start, Manhattan(grid.Start, grid.End));
        seen[grid.Start.Row, grid.Start.Col] = true;

        while (queue.TryDequeue(out var cur, out _))
        {
            yield return new(PathStepKind.Visit, cur);
            if (cur == grid.End)
            {
                foreach (var s in Trace(prev, grid)) yield return s;
                yield break;
            }
            foreach (var n in grid.Neighbors(cur))
            {
                if (seen[n.Row, n.Col]) continue;
                seen[n.Row, n.Col] = true;
                prev[n.Row, n.Col] = cur;
                queue.Enqueue(n, Manhattan(n, grid.End));
                yield return new(PathStepKind.Frontier, n);
            }
        }
    }
}

public sealed class BreadthFirst : PathAlgorithm
{
    public override string Id => "bfs";
    public override string Name => "Breadth-First Search";
    public override string Description =>
        "Explores the grid layer by layer, visiting every cell one step away, then two steps away, and so on. Guarantees the path with the fewest steps, but treats weighted cells like any other.";
    public override bool Weighted => false;
    public override bool Optimal => true;

    public override IEnumerable<PathStep> Find(Grid grid)
    {
        var seen = new bool[grid.Rows, grid.Cols];
        var prev = NewPrevious(grid);
        var queue = new Queue<Point>();
        queue.Enqueue(grid.Start);
        seen[grid.Start.Row, grid.Start.Col] = true;

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            yield return new(PathStepKind.Visit, cur);
            if (cur == grid.End)
            {
                foreach (var s in Trace(prev, grid)) yield return s;
                yield break;
            }
            foreach (var n in grid.Neighbors(cur))
            {
                if (seen[n.Row, n.Col]) continue;
                seen[n.Row, n.Col] = true;
                prev[n.Row, n.Col] = cur;
                queue.Enqueue(n);
                yield return new(PathStepKind.Frontier, n);
            }
        }
    }
}

public sealed class DepthFirst : PathAlgorithm
{
    public override string Id => "dfs";
    public override string Name => "Depth-First Search";
    public override string Description =>
        "Dives as deep as it can down one branch before backtracking. It will find a path if one exists, but the path is usually long and winding, and it ignores weights.";
    public override bool Weighted => false;
    public override bool Optimal => false;

    public override IEnumerable<PathStep> Find(Grid grid)
    {
        var closed = new bool[grid.Rows, grid.Cols];
        var prev = NewPrevious(grid);
        var stack = new Stack<Point>();
        stack.Push(grid.Start);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            if (closed[cur.Row, cur.Col]) continue;
            closed[cur.Row, cur.Col] = true;
            yield return new(PathStepKind.Visit, cur);
            if (cur == grid.End)
            {
                foreach (var s in Trace(prev, grid)) yield return s;
                yield break;
            }
            foreach (var n in grid.Neighbors(cur))
            {
                if (closed[n.Row, n.Col]) continue;
                // Last writer wins: the entry popped first is the most recent push.
                prev[n.Row, n.Col] = cur;
                stack.Push(n);
                yield return new(PathStepKind.Frontier, n);
            }
        }
    }
}
