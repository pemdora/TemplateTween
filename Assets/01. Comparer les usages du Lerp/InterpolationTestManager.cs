using UnityEngine;

namespace TP1
{
    public class InterpolationTestManager : MonoBehaviour, IPlotGraphSource
    {
        [SerializeField] private Transform naive, frameRateIndpt, classic; // Les différentes interpolations à tester sur les différents cubes
        [SerializeField] private float targetDistanceX = 10f; // Distance Target pour chaque lerp
        [SerializeField] private float duration = 1f; // Durée du Lerp

        // --- Data pour l'affichage du graph ---
        static readonly string[] names = { "Faux Lerp", "Frame Rate Idpt + Smooth", "Vrai Lerp"};
        private Transform[] all; // tous les transforms à déplacer pour le lerp
        private Vector3[] froms; // position de depart
        private Vector3[] arrivals; // position de fin de lerp
        private float startTime;
        private Color[] colors;

        #region Implémentation IPlotGraphSource
        public int Count => all != null ? all.Length : 0;
        public string[] Names => names;
        public float StartTime => startTime;
        public Color[] Colors => colors;

        public float Progress(int i)
        {
            float start = froms[i].x;
            float total = targetDistanceX - start;
            return Mathf.Approximately(total, 0f) ? 1f : (all[i].position.x - start) / total;
        }
        #endregion

        void Start()
        {
            QualitySettings.vSyncCount = 0; // la vsync a besoin d'être désactivée si on peut forcer une frame rate (sinon se synchro avec la fréquence de display)
            Application.targetFrameRate = 144;

            // 1. On liste les cubes dans le même ordre que les 'names' du graph
            all = new Transform[] { naive, frameRateIndpt, classic };
            froms = new Vector3[all.Length];
            arrivals = new Vector3[all.Length];
            colors = new Color[all.Length];

            for (int i = 0; i < all.Length; i++)
            {
                froms[i] = all[i].position;
                arrivals[i] = new Vector3(targetDistanceX, froms[i].y, froms[i].z);

                // Récupération automatique de la couleur du matériau du cube pour le graph
                if (all[i] != null && all[i].TryGetComponent<Renderer>(out var rend) && rend.sharedMaterial != null)
                    colors[i] = rend.sharedMaterial.color;
                else
                    colors[i] = Color.white;
            }

            Restart();
        }

        void Restart()
        {
            startTime = Time.time;
            for (int i = 0; i < all.Length; i++)
            {
                all[i].position = froms[i]; // On reset toutes les positions au point de départ
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
                Application.targetFrameRate = Application.targetFrameRate == 30 ? 144 : 30;
            if (Input.GetKeyDown(KeyCode.R)) Restart();

            // ==========================================
            // IMPLEMENTATION ICI :
            // ==========================================

            // 1. Variante "Naive" : Faux Lerp avec facteur constant
            // TODO: Regarde la slide variante 1 : naive et adapte le code pour déplacer le transfrom naive
            // naive.position = ...;

            // 2. Variante "Frame-Rate Independent" : Lissage exponentiel avec Mathf.Exp
            // TODO: Regarde la slide variante 2 : naive et adapte le code pour déplacer le transfrom frameRateIndpt
            // frameRateIndpt.position = ...;

            // 3. Interpolation linéaire classique : de 0 à 1 sur une durée fixe
            // TODO: Utilise Vector3.Lerp de façon clasique, pout t allant de 0 à duration (ici = 1) 
            // classic.position = ...;

            // --- BONUS ---
            // 4. Test avec SmoothDamp
            // smoothDamp.position = ...;

            // 5. Test avec MoveTowards
            // moveTowards.position = ...;
        }

        void OnGUI()
        {
            GUILayout.Label($"Target FPS: {Application.targetFrameRate}  (Press 'F' to toggle 30/144 FPS, 'R' to restart, 'G' = show graph, 'H' = focus 1 curve (graph enabled)");
        }
    }
}