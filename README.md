# Algorithm Visualizer

Interactive, step-by-step visualizations of classic algorithms, built with Blazor WebAssembly and hosted as a static site on GitHub Pages.

| Category | Algorithms |
|---|---|
| Sorting | Quick, Merge, Heap, Bubble, Insertion, Selection |
| Pathfinding | A\*, Dijkstra, Greedy Best-First, Breadth-First, Depth-First |

Every algorithm can be played, paused, stepped one move at a time, and sped up or slowed down. In the pathfinding view you can draw walls and weighted cells and drag the start/end markers.

## Run locally

```bash
dotnet run --project AlgorithmVisualizer
dotnet test
```

## Project layout

- `AlgorithmVisualizer/Algorithms/` – the algorithms, written as lazy `IEnumerable` step sequences with no UI dependencies. The UI just pulls one step at a time, which is what makes play / pause / step trivial.
- `AlgorithmVisualizer/Pages/` – `Sorting.razor`, `Pathfinding.razor` (routes `/sorting/{id}`, `/pathfinding/{id}`) and the home page.
- `AlgorithmVisualizer/Components/` – shared tabs, legend and info panel.
- `AlgorithmVisualizer.Tests/` – xUnit tests that check every algorithm for correctness.

## Adding an algorithm

Subclass `SortAlgorithm` or `PathAlgorithm`, yield steps, and add it to the `All` list in the same file. The nav menu, tabs, home page and routes pick it up automatically.

## Deploying to GitHub Pages

1. In the repository, go to **Settings → Pages** and set **Source** to **GitHub Actions**.
2. Push to `master`. `.github/workflows/deploy.yml` runs the tests, publishes the app, sets `<base href>` to `/<repo-name>/`, adds a `404.html` fallback so deep links work, and deploys.

The site will be at `https://<user>.github.io/<repo-name>/`.
