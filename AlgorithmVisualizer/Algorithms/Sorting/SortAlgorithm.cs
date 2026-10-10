namespace AlgorithmVisualizer.Algorithms.Sorting;

public enum SortStepKind
{
    /// <summary>Two positions are being compared.</summary>
    Compare,
    /// <summary>Two positions were swapped (already applied to the array).</summary>
    Swap,
    /// <summary>A position was overwritten (already applied to the array).</summary>
    Write,
    /// <summary>A position is the current pivot.</summary>
    Pivot,
    /// <summary>A position holds its final value.</summary>
    Sorted,
}

public readonly record struct SortStep(SortStepKind Kind, int A, int B = -1);

/// <summary>
/// A sorting algorithm expressed as a lazy sequence of steps. The array is mutated in place
/// as the sequence is enumerated, so a visualizer can pull one step at a time.
/// </summary>
public abstract class SortAlgorithm
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string BestCase { get; }
    public abstract string AverageCase { get; }
    public abstract string WorstCase { get; }
    public abstract string Space { get; }
    public abstract bool Stable { get; }

    public abstract IEnumerable<SortStep> Sort(int[] a);

    protected static void Swap(int[] a, int i, int j) => (a[i], a[j]) = (a[j], a[i]);
}

public static class SortingAlgorithms
{
    public static IReadOnlyList<SortAlgorithm> All { get; } =
    [
        new QuickSort(),
        new MergeSort(),
        new HeapSort(),
        new BubbleSort(),
        new InsertionSort(),
        new SelectionSort(),
    ];

    public static SortAlgorithm Find(string? id) =>
        All.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase)) ?? All[0];
}

public sealed class BubbleSort : SortAlgorithm
{
    public override string Id => "bubble";
    public override string Name => "Bubble Sort";
    public override string Description =>
        "Repeatedly walks the list, swapping adjacent elements that are out of order. After each pass the largest remaining element has \"bubbled\" to the end. Stops early when a pass makes no swaps.";
    public override string BestCase => "O(n)";
    public override string AverageCase => "O(n²)";
    public override string WorstCase => "O(n²)";
    public override string Space => "O(1)";
    public override bool Stable => true;

    public override IEnumerable<SortStep> Sort(int[] a)
    {
        int n = a.Length;
        for (int pass = 0; pass < n - 1; pass++)
        {
            bool swapped = false;
            int last = n - 1 - pass;
            for (int j = 0; j < last; j++)
            {
                yield return new(SortStepKind.Compare, j, j + 1);
                if (a[j] > a[j + 1])
                {
                    Swap(a, j, j + 1);
                    swapped = true;
                    yield return new(SortStepKind.Swap, j, j + 1);
                }
            }
            if (!swapped)
            {
                for (int k = 0; k <= last; k++) yield return new(SortStepKind.Sorted, k);
                yield break;
            }
            yield return new(SortStepKind.Sorted, last);
        }
    }
}

public sealed class SelectionSort : SortAlgorithm
{
    public override string Id => "selection";
    public override string Name => "Selection Sort";
    public override string Description =>
        "Scans the unsorted part for the smallest element and swaps it into the next position of the sorted prefix. Does very few swaps, but always scans everything.";
    public override string BestCase => "O(n²)";
    public override string AverageCase => "O(n²)";
    public override string WorstCase => "O(n²)";
    public override string Space => "O(1)";
    public override bool Stable => false;

    public override IEnumerable<SortStep> Sort(int[] a)
    {
        int n = a.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int min = i;
            yield return new(SortStepKind.Pivot, min);
            for (int j = i + 1; j < n; j++)
            {
                yield return new(SortStepKind.Compare, j, min);
                if (a[j] < a[min])
                {
                    min = j;
                    yield return new(SortStepKind.Pivot, min);
                }
            }
            if (min != i)
            {
                Swap(a, i, min);
                yield return new(SortStepKind.Swap, i, min);
            }
            yield return new(SortStepKind.Sorted, i);
        }
        if (n > 0) yield return new(SortStepKind.Sorted, n - 1);
    }
}

public sealed class InsertionSort : SortAlgorithm
{
    public override string Id => "insertion";
    public override string Name => "Insertion Sort";
    public override string Description =>
        "Builds the sorted list one element at a time, sliding each new element left until it sits in the right place — like sorting playing cards in your hand. Excellent on small or nearly-sorted input.";
    public override string BestCase => "O(n)";
    public override string AverageCase => "O(n²)";
    public override string WorstCase => "O(n²)";
    public override string Space => "O(1)";
    public override bool Stable => true;

    public override IEnumerable<SortStep> Sort(int[] a)
    {
        for (int i = 1; i < a.Length; i++)
        {
            for (int j = i; j > 0; j--)
            {
                yield return new(SortStepKind.Compare, j - 1, j);
                if (a[j - 1] <= a[j]) break;
                Swap(a, j - 1, j);
                yield return new(SortStepKind.Swap, j - 1, j);
            }
        }
    }
}

public sealed class QuickSort : SortAlgorithm
{
    public override string Id => "quick";
    public override string Name => "Quick Sort";
    public override string Description =>
        "Picks a pivot (here the last element), partitions the range so smaller elements sit left of it and larger ones right, then recursively sorts each side. Usually the fastest comparison sort in practice.";
    public override string BestCase => "O(n log n)";
    public override string AverageCase => "O(n log n)";
    public override string WorstCase => "O(n²)";
    public override string Space => "O(log n)";
    public override bool Stable => false;

    public override IEnumerable<SortStep> Sort(int[] a)
    {
        var ranges = new Stack<(int Lo, int Hi)>();
        ranges.Push((0, a.Length - 1));
        while (ranges.Count > 0)
        {
            var (lo, hi) = ranges.Pop();
            if (lo > hi) continue;
            if (lo == hi)
            {
                yield return new(SortStepKind.Sorted, lo);
                continue;
            }

            int pivot = a[hi];
            yield return new(SortStepKind.Pivot, hi);
            int store = lo;
            for (int j = lo; j < hi; j++)
            {
                yield return new(SortStepKind.Compare, j, hi);
                if (a[j] < pivot)
                {
                    if (store != j)
                    {
                        Swap(a, store, j);
                        yield return new(SortStepKind.Swap, store, j);
                    }
                    store++;
                }
            }
            if (store != hi)
            {
                Swap(a, store, hi);
                yield return new(SortStepKind.Swap, store, hi);
            }
            yield return new(SortStepKind.Sorted, store);

            // Push the right side first so the left side is processed first.
            ranges.Push((store + 1, hi));
            ranges.Push((lo, store - 1));
        }
    }
}

public sealed class MergeSort : SortAlgorithm
{
    public override string Id => "merge";
    public override string Name => "Merge Sort";
    public override string Description =>
        "Splits the list in half, sorts each half recursively, then merges the two sorted halves back together. Guaranteed O(n log n) and stable, at the cost of extra memory for merging.";
    public override string BestCase => "O(n log n)";
    public override string AverageCase => "O(n log n)";
    public override string WorstCase => "O(n log n)";
    public override string Space => "O(n)";
    public override bool Stable => true;

    public override IEnumerable<SortStep> Sort(int[] a) => Sort(a, new int[a.Length], 0, a.Length);

    private static IEnumerable<SortStep> Sort(int[] a, int[] aux, int lo, int hi)
    {
        if (hi - lo < 2) yield break;
        int mid = lo + (hi - lo) / 2;
        foreach (var s in Sort(a, aux, lo, mid)) yield return s;
        foreach (var s in Sort(a, aux, mid, hi)) yield return s;

        Array.Copy(a, lo, aux, lo, hi - lo);
        int i = lo, j = mid;
        for (int k = lo; k < hi; k++)
        {
            if (i < mid && j < hi)
            {
                yield return new(SortStepKind.Compare, i, j);
                a[k] = aux[i] <= aux[j] ? aux[i++] : aux[j++];
            }
            else
            {
                a[k] = i < mid ? aux[i++] : aux[j++];
            }
            yield return new(SortStepKind.Write, k);
        }
    }
}

public sealed class HeapSort : SortAlgorithm
{
    public override string Id => "heap";
    public override string Name => "Heap Sort";
    public override string Description =>
        "Turns the array into a max-heap, then repeatedly swaps the largest element (the root) to the end and restores the heap on what remains. In-place with guaranteed O(n log n).";
    public override string BestCase => "O(n log n)";
    public override string AverageCase => "O(n log n)";
    public override string WorstCase => "O(n log n)";
    public override string Space => "O(1)";
    public override bool Stable => false;

    public override IEnumerable<SortStep> Sort(int[] a)
    {
        int n = a.Length;
        for (int i = n / 2 - 1; i >= 0; i--)
            foreach (var s in SiftDown(a, i, n)) yield return s;

        for (int end = n - 1; end > 0; end--)
        {
            Swap(a, 0, end);
            yield return new(SortStepKind.Swap, 0, end);
            yield return new(SortStepKind.Sorted, end);
            foreach (var s in SiftDown(a, 0, end)) yield return s;
        }
        if (n > 0) yield return new(SortStepKind.Sorted, 0);
    }

    private static IEnumerable<SortStep> SiftDown(int[] a, int root, int size)
    {
        while (true)
        {
            int largest = root;
            int left = 2 * root + 1, right = left + 1;
            if (left < size)
            {
                yield return new(SortStepKind.Compare, left, largest);
                if (a[left] > a[largest]) largest = left;
            }
            if (right < size)
            {
                yield return new(SortStepKind.Compare, right, largest);
                if (a[right] > a[largest]) largest = right;
            }
            if (largest == root) yield break;
            Swap(a, root, largest);
            yield return new(SortStepKind.Swap, root, largest);
            root = largest;
        }
    }
}
