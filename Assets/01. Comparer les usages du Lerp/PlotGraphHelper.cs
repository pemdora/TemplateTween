using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Live plot of progress (0 -> 1) for each cube over time.
// Attach to any GameObject and drag the LerpTest script into "source".
// Solid line = current run, faded line = previous run (to compare 30 and 144 FPS).
public class PlotGraphHelper : MonoBehaviour
{
    [SerializeField] float window = 2f;                       // Display duration on the X axis (s)
    [SerializeField] Vector2 size = new Vector2(480, 300);    // Frame size in pixels
    [SerializeField] float margin = 20f;                      // Margin from bottom-left corner
    [SerializeField] KeyCode toggleKey = KeyCode.G;
    [SerializeField] KeyCode cycleKey = KeyCode.H;             // Press to isolate/cycle curves

    const float YMax = 1.1f; // Extra space above 100%
    private IPlotGraphSource source;

    void Awake()
    {
        source = GetComponent<IPlotGraphSource>();
    }

    class Run
    {
        public List<Vector2>[] curves; // (time, progress) per cube
        public int targetFps;
        public float lastT;

        public Run(int count, int fps)
        {
            curves = new List<Vector2>[count];
            for (int i = 0; i < count; i++) curves[i] = new List<Vector2>(512);
            targetFps = fps;
        }

        public float MeasuredFps => lastT > 0.2f ? (curves[0].Count - 1) / lastT : 0f;
    }

    Run current, ghost;
    float lastStart;
    bool visible = true;
    int activeCurveIndex = -1; // -1 = show all, 0..N-1 = isolate curve
    Material mat;
    GUIStyle left, center, right;
    string legend;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) visible = !visible;

        // Cycle through individual curves or show all
        if (Input.GetKeyDown(cycleKey) && source != null && source.Count > 0)
        {
            activeCurveIndex++;
            if (activeCurveIndex >= source.Count) activeCurveIndex = -1;
            legend = null; // Force legend rebuild
        }
    }

    // LateUpdate: read positions after LerpTest.Update updates them
    void LateUpdate()
    {
        if (source == null || source.Count == 0) return;

        if (current == null || source.StartTime != lastStart)
        {
            if (current != null && current.lastT > 0f) ghost = current;
            current = new Run(source.Count, Application.targetFrameRate);
            lastStart = source.StartTime;
        }

        float t = Time.time - source.StartTime;
        if (t > window) return;

        current.lastT = t;
        for (int i = 0; i < source.Count; i++)
            current.curves[i].Add(new Vector2(t, source.Progress(i)));
    }

    void OnGUI()
    {
        if (!visible || current == null || Event.current.type != EventType.Repaint) return;
        EnsureResources();

        // Frame + plot area
        var box = new Rect(margin, Screen.height - size.y - margin, size.x, size.y);
        var plot = new Rect(box.x + 44, box.y + 40, box.width - 58, box.height - 80);

        var old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.78f);
        GUI.DrawTexture(box, Texture2D.whiteTexture);
        GUI.color = old;

        // Curves (GL)
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadPixelMatrix();
        GL.Begin(GL.QUADS);

        var grid = new Color(1f, 1f, 1f, 0.15f);
        var axis = new Color(1f, 1f, 1f, 0.45f);
        Line(ToGL(plot, 0, 0.5f), ToGL(plot, window, 0.5f), 1f, grid);
        Line(ToGL(plot, 0, 1f), ToGL(plot, window, 1f), 1f, new Color(1f, 1f, 1f, 0.35f)); // target line
        for (float t = 0.5f; t < window + 0.001f; t += 0.5f)
            Line(ToGL(plot, t, 0), ToGL(plot, t, YMax), 1f, grid);
        Line(ToGL(plot, 0, 0), ToGL(plot, window, 0), 1.5f, axis);
        Line(ToGL(plot, 0, 0), ToGL(plot, 0, YMax), 1.5f, axis);

        if (ghost != null) DrawRun(plot, ghost, 0.3f, 2f);
        DrawRun(plot, current, 1f, 2.5f);

        // Time cursor
        float tc = Mathf.Min(current.lastT, window);
        Line(ToGL(plot, tc, 0), ToGL(plot, tc, YMax), 1f, new Color(1f, 1f, 1f, 0.6f));

        GL.End();
        GL.PopMatrix();

        // Text labels (GUI)
        GUI.Label(new Rect(box.x + 8, box.y + 4, box.width - 16, 34), legend, left);

        Label(plot, 0, 0, "0 %");
        Label(plot, 0, 0.5f, "50 %");
        Label(plot, 0, 1f, "100 %");

        for (float t = 0f; t < window + 0.001f; t += 0.5f)
            GUI.Label(new Rect(plot.x + t / window * plot.width - 20, plot.yMax + 2, 40, 16), t.ToString("0.#"), center);

        string info = $"solid: {current.targetFps} target FPS ({current.MeasuredFps:F0} measured)";
        if (ghost != null) info += $"   |   faded: {ghost.targetFps} target FPS ({ghost.MeasuredFps:F0} measured)";
        GUI.Label(new Rect(box.x + 8, box.yMax - 22, box.width - 16, 18), info, left);
    }

    void DrawRun(Rect plot, Run run, float alpha, float width)
    {
        for (int i = 0; i < run.curves.Length; i++)
        {
            // Skip curve if filtering is active and doesn't match index
            if (activeCurveIndex != -1 && activeCurveIndex != i) continue;

            var c = source.Colors[i % source.Colors.Length];
            c.a = alpha;
            var pts = run.curves[i];
            for (int k = 1; k < pts.Count; k++)
                Line(ToGL(plot, pts[k - 1].x, pts[k - 1].y), ToGL(plot, pts[k].x, pts[k].y), width, c);
        }
    }

    // Coordinates (time, progress) -> GL pixels (bottom-left origin)
    Vector2 ToGL(Rect plot, float t, float p)
    {
        float x = plot.x + Mathf.Clamp01(t / window) * plot.width;
        float yGui = plot.yMax - Mathf.Clamp(p, 0f, YMax) / YMax * plot.height;
        return new Vector2(x, Screen.height - yGui);
    }

    void Label(Rect plot, float t, float p, string text)
    {
        float y = plot.yMax - p / YMax * plot.height;
        GUI.Label(new Rect(plot.x - 44, y - 8, 40, 16), text, right);
    }

    // Thick line segment drawn as a quad
    static void Line(Vector2 a, Vector2 b, float width, Color c)
    {
        Vector2 d = b - a;
        if (d.sqrMagnitude < 1e-6f) return;
        Vector2 n = new Vector2(-d.y, d.x).normalized * (width * 0.5f);
        GL.Color(c);
        GL.Vertex3(a.x + n.x, a.y + n.y, 0);
        GL.Vertex3(b.x + n.x, b.y + n.y, 0);
        GL.Vertex3(b.x - n.x, b.y - n.y, 0);
        GL.Vertex3(a.x - n.x, a.y - n.y, 0);
    }

    void EnsureResources()
    {
        if (mat == null)
        {
            mat = new Material(Shader.Find("Hidden/Internal-Colored")) { hideFlags = HideFlags.HideAndDontSave };
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            mat.SetInt("_ZWrite", 0);
        }

        if (left == null)
        {
            left = new GUIStyle(GUI.skin.label) { fontSize = 11, richText = true, wordWrap = true, alignment = TextAnchor.UpperLeft };
            center = new GUIStyle(left) { alignment = TextAnchor.UpperCenter };
            right = new GUIStyle(left) { alignment = TextAnchor.UpperRight };
        }

        if (legend == null && source != null)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < source.Count; i++)
            {
                // Dim or hide inactive curves from the legend when isolated
                if (activeCurveIndex != -1 && activeCurveIndex != i) continue;

                sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(source.Colors[i % source.Colors.Length])}>■</color> {source.Names[i]}    ");
            }
            legend = sb.ToString();
        }
    }

    void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }
}