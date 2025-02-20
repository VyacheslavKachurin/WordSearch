using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public static class Extensions
{
    public static void Toggle(this VisualElement element, bool value)
    {
        element.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public static Texture2D GetTexture(int season, bool isSmall = false)
    {
        /*
        var texturePath = Path.Combine(Application.persistentDataPath, "Pictures", season + ".jpg");
        Texture2D texture = null;
        if (File.Exists(texturePath))
        {
            var format = isSmall ? TextureFormat.PVRTC_RGB2 : TextureFormat.ASTC_6x6;
            byte[] fileData = File.ReadAllBytes(texturePath);
            texture = new Texture2D(2, 2, format, false);
            texture.LoadImage(fileData);

        }
        */

        var texture = Resources.Load<Texture2D>($"Pictures/{season}");
        return texture;
    }


    public static Vector2 Pos(this Transform trans)
    {
        return trans.position;
    }

    public static void DeleteDirectory(string target_dir)
    {
        string[] files = Directory.GetFiles(target_dir);
        string[] dirs = Directory.GetDirectories(target_dir);

        foreach (string file in files)
        {
            //  File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(target_dir, false);
    }

    // Gets screen coordinates from a UI Toolkit ClickEvent position.
    public static Vector2 GetScreenCoordinate(this Vector2 clickPosition, VisualElement rootVisualElement)
    {
        // Adjust the clickPosition for the borders (for the SafeAreaBorder)
        float borderLeft = rootVisualElement.resolvedStyle.borderLeftWidth;
        float borderTop = rootVisualElement.resolvedStyle.borderTopWidth;
        clickPosition.x += borderLeft;
        clickPosition.y += borderTop;

        // Normalize the UI Toolkit position to account for Panel Match settings
        Vector2 normalizedPosition = clickPosition.NormalizeClickEventPosition(rootVisualElement);

        // Multiply by Screen dimensions to get screen coordinates in pixels
        float xValue = normalizedPosition.x * Screen.width;
        float yValue = normalizedPosition.y * Screen.height;
        return new Vector2(xValue, yValue);
    }

    // Normalizes a UI Toolkit ClickEvent position to a range between (0,0) to (1,1).
    public static Vector2 NormalizeClickEventPosition(this Vector2 clickPosition, VisualElement rootVisualElement)
    {
        // Get a Rect that represents the boundaries of the screen in UI Toolkit
        Rect rootWorldBound = rootVisualElement.worldBound;

        float normalizedX = clickPosition.x / rootWorldBound.xMax;

        // Flip the y value so y = 0 is at the bottom of the screen
        float normalizedY = 1 - clickPosition.y / rootWorldBound.yMax;

        return new Vector2(normalizedX, normalizedY);

    }

    public static Vector3 ScreenPosToWorldPos(this Vector2 screenPos, Camera camera = null, float zDepth = 10f)
    {

        if (camera == null)
            camera = Camera.main;

        if (camera == null)
            return Vector2.zero;

        float xPos = screenPos.x;
        float yPos = screenPos.y;
        Vector3 worldPos = Vector3.zero;

        if (!float.IsNaN(screenPos.x) && !float.IsNaN(screenPos.y) && !float.IsInfinity(screenPos.x) && !float.IsInfinity(screenPos.y))
        {
            // convert to world space position using Camera class
            Vector3 screenCoord = new Vector3(xPos, yPos, zDepth);
            worldPos = camera.ScreenToWorldPoint(screenCoord);
        }
        return worldPos;
    }

    public static Vector2 GetWorldPosition(this VisualElement ve, VisualElement root)
    {
        var worldBound = ve.worldBound;
        var centerPos = new Vector2(worldBound.x + worldBound.width / 2, worldBound.y + worldBound.height / 2);

        Vector2 screenPos = centerPos.GetScreenCoordinate(root);
        Vector3 worldPos = screenPos.ScreenPosToWorldPos(Camera.main, 10);
        return worldPos;
    }

    public static void SetRenderTexture(this Image overlayFX, Camera fxCam)
    {
        var deviceWidth = Screen.width;
        var deviceHeight = Screen.height;
        var renderTexture = new RenderTexture(deviceWidth, deviceHeight, 0);

        fxCam.targetTexture = renderTexture;
        overlayFX.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(renderTexture));

    }

    public static Vector2 FindClosestPoint(Dictionary<Direction, Vector2> points, Vector2 touchPoint)
    {

        Vector2 closestPoint = points.First().Value;
        float distance = Vector2.Distance(closestPoint, touchPoint);

        foreach (var pair in points)
        {
            float currentDistance = Vector2.Distance(pair.Value, touchPoint);
            if (currentDistance < distance)
            {
                closestPoint = pair.Value;
                distance = currentDistance;
            }
        }

        return closestPoint;
    }

}
