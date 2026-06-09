using UnityEngine;

public static class GameplayUiSpriteUtility
{
    static Sprite whiteSprite;

    public static Sprite WhiteSprite
    {
        get
        {
            if (whiteSprite != null)
            {
                return whiteSprite;
            }

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            whiteSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                100f);
            whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return whiteSprite;
        }
    }

    public static void ApplyWhiteSprite(UnityEngine.UI.Image image)
    {
        if (image == null)
        {
            return;
        }

        if (image.sprite == null)
        {
            image.sprite = WhiteSprite;
        }

        image.type = UnityEngine.UI.Image.Type.Simple;
    }
}
