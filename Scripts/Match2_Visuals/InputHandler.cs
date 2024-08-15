using UnityEngine;

public class InputHandler : IInputHandler
{
    private readonly IClickProcessor clickProcessor;

    public InputHandler(IClickProcessor clickProcessor)
    {
        this.clickProcessor = clickProcessor;
    }
    
    public void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 touchPos = new Vector2(worldPoint.x, worldPoint.y);
            RaycastHit2D hit = Physics2D.Raycast(touchPos, Vector2.zero);

            if (hit.collider != null)
            {
                GameObject touchedObject = hit.collider.gameObject;

                string[] parts = touchedObject.name.Split('_');
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);

                clickProcessor.ProcessClick(x, y);
            }
        }
    }
}