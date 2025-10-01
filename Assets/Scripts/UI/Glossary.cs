using System.Collections;
using System.Collections.Generic;
using SerializableMethods;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Glossary : MonoBehaviour
{
    [SerializeField] private InfoReference mainGlossary;
    [SerializeField] private GlossaryButton referencesPrefab;
    [Header("Components")] 
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Button backButton;
    [SerializeField] private Button exitButton;

    private Stack<InfoReference> navigationPath = new();
    private List<GlossaryButton> references = new();

    private static Glossary instance;

    private void Awake()
    {
        if (ReferenceEquals(instance, this)) return;
        if (!ReferenceEquals(instance, null))
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        backButton.onClick.AddListener(GoBack);
        exitButton.onClick.AddListener(Close);
    }

    public static void OpenGlossary(InfoReference info = null)
    {
        instance.Open(info);
    }

    public static void CloseGlossary()
    {
        instance.Close();
    }

    [SerializeMethod]
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
        references.EnsureEnoughInstances(referencesPrefab, info.references.Count, container,
            glossaryButton =>
                glossaryButton.button.onClick.AddListener(() => OnClickReference(references.IndexOf(glossaryButton))));

        for (int i = 0; i < info.references.Count; i++)
            references[i].label.text = info.references[i].Name;
    }

    private void OnClickReference(int id)
    {
        InfoReference info = navigationPath.Peek();
        if(info.references.Count <= id) return;
        navigationPath.Push(info.references[id]);
        LoadInfo();
    }

    private void Close()
    {
        navigationPath.Clear();
        gameObject.SetActive(false);
    }

    private void GoBack()
    {
        navigationPath.Pop();
        if (navigationPath.Count > 0)
            LoadInfo();
        else
            Close();
    }
}
