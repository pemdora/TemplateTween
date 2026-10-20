using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class UIDisplay : MonoBehaviour, IUIDisplay
{
    [Header("Références")]
    [SerializeField] private TMP_Text m_textMeshValue;

    [Header("Paramètres d'Animation")]
    [SerializeField] private float m_duration = 1f;

    private RectTransform m_rectTransform;
    private Coroutine m_currentAnimation;

    private void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    // Ouvre l'UI avec un message
    public void Open(string message)
    {
        // TODO : Assigner le texte à m_textMesh si la référence n'est pas nulle
        // TODO : Activer le GameObject
        // TODO : Si une animation est déjà en cours (m_currentAnimation != null), l'arrêter avec StopCoroutine
        // TODO : Lancer la coroutine d'animation (ScaleRoutine) de Vector3.zero à Vector3.one
    }

    // Coroutine d'animation simple avec un Lerp
    private IEnumerator ScaleRoutine(Vector3 startScale, Vector3 targetScale, float duration)
    {
        float time = 0f;
        m_rectTransform.localScale = startScale;

        while (time < duration)
        {
            // TODO : Incrémenter le temps avec Time.deltaTime
            // TODO : Calculer la progression (entre 0 et 1) en divisant le temps par la durée
            // TODO : Appliquer un Vector3.Lerp sur l'échelle (localScale) de m_rectTransform

            yield return null;
        }

        // TODO : S'assurer qu'à la fin, l'échelle est exactement égale à targetScale
        // TODO : Remettre m_currentAnimation à null
    }
}