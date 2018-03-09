using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class FlexibleUIButton : FlexibleUI {

    Image image;
    Image icon;
    Button button;

    protected override void OnSkinUI() {
        base.OnSkinUI();

        image = GetComponent<Image>();
        icon = transform.Find("Icon").GetComponent<Image>();
        button = GetComponent<Button>();

        button.transition = Selectable.Transition.SpriteSwap;
        button.targetGraphic = image;

        image.sprite = skinData.buttonSprite;
        if (icon != null) {
            icon.sprite = skinData.icon;
        }
        image.type = Image.Type.Sliced;
        button.spriteState = skinData.buttonSpriteState;
        
    }

}
