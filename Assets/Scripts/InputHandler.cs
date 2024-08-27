using UnityEngine;

public class InputHandler : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider != null)
            {
                var letter = hit.collider.GetComponent<LetterUnit>();
                Debug.Log(letter.Letter);
            }
        }
    }
}