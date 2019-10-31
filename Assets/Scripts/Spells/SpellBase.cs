using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpellBase : MonoBehaviour
{
    public GestureRecognition.Gesture SpellGesture;
    public Color OrbColor = Color.white;
    public Texture PageTexture;
    public Texture SpellIcon;
    public virtual void UnleashSpell() {}
    public virtual void OnAimStart() {}
    public virtual void OnAimEnd() {}
}
