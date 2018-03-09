using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Flexible UI Data")]
public class FlexibleUIData : ScriptableObject {

    public Sprite buttonSprite;
    public SpriteState buttonSpriteState;
    public Sprite icon;

    public Color primaryColor;
    public Color secondaryColor;
}
