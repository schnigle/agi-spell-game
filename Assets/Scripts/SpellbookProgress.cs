using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellbookProgress", menuName = "ScriptableObjects/SpellbookProgress", order = 1)]
public class SpellbookProgress : ScriptableObject
{
    public List<Texture> spellTextures;
}
