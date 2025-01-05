using UnityEngine;

public class Contact : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("moveable") || collision.CompareTag("player"))
        {
            GameObject parent = this.transform.parent.gameObject; // necessary

            collision.transform.SetParent(parent.transform); // necessary
            Debug.Log("Stuck!"); // necessary

        }
    }

    private void OnTriggerExit(Collider collision)
    {
        collision.transform.SetParent(null);
        collision.transform.rotation = Quaternion.Euler(Vector3.zero);
        Physics.SyncTransforms();
    }
}
