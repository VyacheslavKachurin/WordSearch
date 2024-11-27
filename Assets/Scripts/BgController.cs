
using UnityEngine;

public class BgController : MonoBehaviour
{
    [SerializeField] Canvas _canvas;
    [SerializeField] UnityEngine.UI.Image _image;

    public Texture2D GetBackView()
    {

        var texture = Resources.Load("BG/" + GameDataService.GameData.Season) as Texture2D;
        return texture;
    }


    public void CreateBackView()
    {
        var mainCamera = Camera.main;
        var texture = Resources.Load("BG/" + GameDataService.GameData.Season) as Texture2D;
        var screenWidth = Screen.width;

        RectTransform canvasRect = _canvas.GetComponent<RectTransform>();

        float height = mainCamera.orthographicSize * 2;
        float width = height * mainCamera.aspect;
        canvasRect.sizeDelta = new Vector2(width, height);
        _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));



    }



}

