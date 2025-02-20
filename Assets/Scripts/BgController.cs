
using System.IO;
using UnityEngine;

public class BgController : MonoBehaviour
{
    [SerializeField] Canvas _canvas;
    [SerializeField] UnityEngine.UI.Image _image;

    public Texture2D GetBackView()
    {
        var texturePath = Path.Combine(Application.persistentDataPath, "Pictures", ProgressService.Progress.Season + ".jpg");
        Texture2D texture = null;
        if (File.Exists(texturePath))
        {
            byte[] fileData = File.ReadAllBytes(texturePath);
            texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
        }
        return texture;
    }

    public void CreateBackView()
    {
        var mainCamera = Camera.main;

        var texture = Extensions.GetTexture(ProgressService.Progress.Season);

        RectTransform canvasRect = _canvas.GetComponent<RectTransform>();

        float height = mainCamera.orthographicSize * 2;
        float width = height * mainCamera.aspect;
        canvasRect.sizeDelta = new Vector2(width, height);
        _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

    }



}

