using UnityEngine;

public class CheckTrigger : MonoBehaviour
{
    public bool isTriggered = false;

    public bool isBattery = false;
    public bool isBulb = false;

    public Transform ConnectedOne;

    private void Start()
    {

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("CP"))
        {
            isTriggered = true;
            ConnectedOne = other.GetComponentInParent<Transform>().parent;

            if (ConnectedOne != null)
            {
                if (other.GetComponentInParent<Transform>().parent.CompareTag("Battery") || other.GetComponentInParent<CheckConnections>().batteryConnected)
                {
                    isBattery = true;
                }
                else if (other.GetComponentInParent<CheckConnections>().batteryConnected == false)
                {
                    isBattery = false;
                }
                if (other.GetComponentInParent<Transform>().parent.CompareTag("Bulb") || other.GetComponentInParent<CheckConnections>().bulbConnected)
                {
                    isBulb = true;
                }
                else if (other.GetComponentInParent<CheckConnections>().bulbConnected == false)
                {
                    isBulb = false;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("CP"))
        {
            isTriggered = false;

            isBattery = false;
            isBulb = false;
        }
    }
}
