using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Glossary : MonoBehaviour
{
    [SerializeField] private InfoReference mainGlossary;
    [SerializeField] private Button referencesPrefab;
    [Header("Components")]
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;

    private Stack<InfoReference> navigationPath = new();
    private List<GlossaryButton> references = new();

    private static Glossary instance;

    private void Awake()
    {
        if (ReferenceEquals(instance, this)) return;

        if(ReferenceEquals(instance, null))
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        Destroy(gameObject);
    }

    public static void OpenGlossary(InfoReference info = null)
    {
        instance.Open(info);
    }

    public static void CloseGlossary()
    {
        instance.Close();
    }

    private void Open(InfoReference info = null)
    {
        navigationPath.Push(info ?? mainGlossary);
        LoadInfo();
    }

    private void LoadInfo()
    {
        InfoReference info = navigationPath.Peek();
        title.text = info.Name;
        description.text = info.Description;

    }

    private void Close()
    {
        navigationPath.Clear();
    }
}
