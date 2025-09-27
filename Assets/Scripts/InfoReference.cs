using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new InfoReference", menuName = "Info Reference")]
public class InfoReference : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;
    public List<InfoReference> references = new();
}
