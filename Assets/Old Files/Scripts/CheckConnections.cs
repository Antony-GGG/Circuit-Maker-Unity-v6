/*using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Triggers
{
    public bool Triggered;
    public CheckTrigger t_obj;
}

public class CheckConnections : MonoBehaviour
{
    [SerializeField] List<Triggers> ChildConnection = new();

    [SerializeField] SpriteRenderer _spriteRenderer;

    public bool Lited = false;

    public Transform PowerConnection;


    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void UpdateColor()
    {
        _spriteRenderer.color = Color.green;
        Lited = true;
    }

    public void UpdateConnection(bool lit)
    {
        if (lit == true)
        {
            UpdateColor();
            foreach (Triggers trigger in ChildConnection)
            {
                if (trigger.t_obj != null)
                {
                    if (trigger.t_obj.ConnectedOne != PowerConnection)
                    {
                        trigger.t_obj.Process(trigger.t_obj.ConnectedOne);
                    }
                }
            }
        }
        else
        {
            ResetState();
        }
    }

    public void ResetState()
    {
        _spriteRenderer.color = Color.white;
        Lited = false;
    }
}
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Triggers
{
    public bool Triggered;
    public CheckTrigger t_obj;
}

public class CheckConnections : MonoBehaviour
{
    [SerializeField] List<Triggers> ChildConnections = new();

    [SerializeField] List<bool> _isConnected;
    [SerializeField] List<bool> _isBattery;
    [SerializeField] List<bool> _isBulb;

    [SerializeField] SpriteRenderer _spriteRenderer;

    [SerializeField] Sprite littedBulb;
    [SerializeField] Sprite unlittedBulb;

    public bool isLevelCompleted = false;

    private bool Lited = false;

    public Transform PowerConnection;

    public bool connected;
    public bool batteryConnected;
    public bool bulbConnected;

    private bool am_I_Battery = false;
    private bool am_I_Bulb = false; 

    private Quaternion targetRotation;
    private float rotationSpeed = 5f;

    public bool isRotating = false;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (this.CompareTag("Battery"))
        {
            am_I_Battery = true;
            _spriteRenderer.color = Color.green;
        }
        if (this.CompareTag("Bulb"))
        {
            am_I_Bulb = true;
        }

        _isConnected = GetAllConnection(ChildConnections);

        BatteryConnection();
    }
    private void Update()
    {
        if (!am_I_Battery)
        {
            if (batteryConnected)
            {
                if (!Lited)
                {
                    UpdateColor();
                }
            }
            else
            {
                if (Lited)
                {
                    ResetState();
                }
            }
        }

        if (isRotating)
        {
            RotateAction();
        }

        BatteryConnection();

        if(am_I_Bulb && batteryConnected && bulbConnected)
        {
            isLevelCompleted = true;
        }
    }

    private void UpdateColor()
    {
        _spriteRenderer.color = Color.green;
        Lited = true;
        if (am_I_Bulb) 
        {
            GameObject _parentBulb = gameObject.GetComponentInParent<Transform>().parent.gameObject;
            _parentBulb.GetComponent<SpriteRenderer>().sprite = littedBulb;
        }
    }
    private void ResetState()
    {
        _spriteRenderer.color = Color.white;
        Lited = false;
        if (am_I_Bulb)
        {
            GameObject _parentBulb = gameObject.GetComponentInParent<Transform>().parent.gameObject;
            _parentBulb.GetComponent<SpriteRenderer>().sprite = unlittedBulb;
        } 
    }

    public void BatteryConnection()
    {
        for (int i = 0; i < ChildConnections.Count; i++)
        {
            _isConnected[i] = ChildConnections[i].t_obj.isTriggered;
            _isBattery[i] = ChildConnections[i].t_obj.isBattery;
            _isBulb[i] = ChildConnections[i].t_obj.isBulb;
        }

        if (_isConnected.Any(b => b == true))
        {
            connected = true;
        }
        else
        {
            connected = false;
        }

        if (_isBattery.Any(b => b == true))
        {
            batteryConnected = true;
        }
        else
        {
            batteryConnected = false;
        }

        if (_isBulb.Any(b => b == true))
        {
            bulbConnected = true;
        }
        else
        {
            bulbConnected = false;
        }
    }

    private void RotateAction()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.2f)
        {
            transform.rotation = targetRotation;

            isRotating = false;
        }
    }

    public IEnumerator RotateObject(Vector3 rotationAxis, float rotationAngle)
    {
        targetRotation = Quaternion.Euler(transform.eulerAngles + rotationAxis * rotationAngle);

        isRotating = true;

        yield return null;
    }

    private List<bool> GetAllConnection(List<Triggers> connectionPoints)
    {
        _isConnected = new List<bool>();
        _isBattery = new List<bool>();
        _isBulb = new List<bool>();

        for (int i = 0; i < connectionPoints.Count; i++)
        {
            _isConnected.Add(ChildConnections[i].t_obj.isTriggered);
            _isBattery.Add(ChildConnections[i].t_obj.isBattery);
            _isBulb.Add(ChildConnections[i].t_obj.isBulb);
        }
        return _isConnected;
    }

}
