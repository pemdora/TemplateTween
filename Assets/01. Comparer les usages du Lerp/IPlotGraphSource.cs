using UnityEngine;

public interface IPlotGraphSource
{
    int Count { get; }
    string[] Names { get; }
    float StartTime { get; }
    Color[] Colors { get; }
    float Progress(int index);
}
