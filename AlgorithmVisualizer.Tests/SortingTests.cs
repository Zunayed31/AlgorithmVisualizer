using AlgorithmVisualizer.Algorithms.Sorting;

namespace AlgorithmVisualizer.Tests;

public class SortingTests
{
    public static IEnumerable<object[]> Algorithms => SortingAlgorithms.All.Select(a => new object[] { a.Id });

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Sorts_random_arrays(string id)
    {
        var algo = SortingAlgorithms.Find(id);
        var rng = new Random(42);
        foreach (var n in new[] { 0, 1, 2, 3, 10, 57, 150 })
        {
            var data = Enumerable.Range(0, n).Select(_ => rng.Next(0, 20)).ToArray();
            var expected = data.OrderBy(x => x).ToArray();

            foreach (var _ in algo.Sort(data)) { }

            Assert.Equal(expected, data);
        }
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Sorts_already_sorted_and_reversed(string id)
    {
        var algo = SortingAlgorithms.Find(id);
        var asc = Enumerable.Range(1, 40).ToArray();
        var desc = asc.Reverse().ToArray();

        foreach (var _ in algo.Sort(asc)) { }
        foreach (var _ in algo.Sort(desc)) { }

        Assert.Equal(Enumerable.Range(1, 40), asc);
        Assert.Equal(Enumerable.Range(1, 40), desc);
    }

    [Theory]
    [MemberData(nameof(Algorithms))]
    public void Step_indices_stay_in_bounds(string id)
    {
        var algo = SortingAlgorithms.Find(id);
        var data = Enumerable.Range(1, 30).OrderBy(_ => Random.Shared.Next()).ToArray();

        foreach (var s in algo.Sort(data))
        {
            Assert.InRange(s.A, 0, data.Length - 1);
            if (s.Kind is SortStepKind.Compare or SortStepKind.Swap)
                Assert.InRange(s.B, 0, data.Length - 1);
        }
    }
}
