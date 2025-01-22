using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropScript : MonoBehaviour
{
    public GameObject[] propPrefab;
    public GameObject currentProp;
    public bool isFilled = false;
    public int _propType;
    private int rotation;
    private const int min = 0;
    private const int max = 3;
    private const int rotationMultiplayer = 90;
    public SpriteRenderer emptyPropSprite;
    public SpriteRenderer fullPropSprite;
    public List<GameObject> checkBoxes;
    public List<PropScript> result;

    public AudioSource clickSound;

    bool soundPlayed = false;
    public AudioSource connectionSound;

    public void Initialize(int propType)
    {
        _propType = propType;
        currentProp = Instantiate(propPrefab[propType], transform);
        currentProp.transform.localPosition = Vector3.zero;

        //float scaleFactor = 1f;

        //currentProp.transform.localScale *= scaleFactor;

        // 0 act as a empty
        // 1 act as a battery
        // 6 act as a bulb
        // other numbera are wires

        if (propType == 1 /*|| propType == 2*/)
        {
            rotation = propType;
        }
        else
        {
            rotation = Random.Range(min, max + 1) * rotationMultiplayer;
        }
        currentProp.transform.eulerAngles = new Vector3(0, 0, rotation * rotationMultiplayer);
        if (propType == 0 /*|| propType == 1*/ /*Battery shouldn't be filled by default*/)
        {
            isFilled = true;
        }
        if (propType == 0)
        {
            return;
        }

        emptyPropSprite = currentProp.transform.GetChild(0).GetComponent<SpriteRenderer>();
        fullPropSprite = currentProp.transform.GetChild(1).GetComponent<SpriteRenderer>();
        emptyPropSprite.gameObject.SetActive(!isFilled);
        fullPropSprite.gameObject.SetActive(isFilled);
        for (int i = 2; i < currentProp.transform.childCount; i++)
        {
            checkBoxes.Add(currentProp.transform.GetChild(i).gameObject);
        }
    }

    public List<PropScript> connectProps()
    {
        result = new List<PropScript>();
        foreach (var box in checkBoxes)
        {
            RaycastHit2D[] hit = Physics2D.RaycastAll(box.transform.position, Vector2.zero, 0.1f);
            for (int i = 0; i < hit.Length; i++)
            {
                result.Add(hit[i].collider.gameObject.transform.parent.parent.GetComponent<PropScript>());
            }
        }
        return result;
    }

    public void UpdateFilled()
    {
        if (_propType == 0) { return; }
        emptyPropSprite.gameObject.SetActive(!isFilled);
        fullPropSprite.gameObject.SetActive(isFilled);

        if (_propType == 6)
        {
            if (isFilled && !soundPlayed)
            {
                if (connectionSound.isPlaying)
                {
                    connectionSound.Stop();
                }
                connectionSound.Play();
                soundPlayed = true;
            }

            if (!isFilled && soundPlayed)
            {
                soundPlayed = false;
            }
        }
    }

    public void UpdateInput()
    {
        if (_propType == 0)
        {
            return;
        }

        if (clickSound.isPlaying)
        {
            clickSound.Stop();
        }
        clickSound.Play();

        rotation = (rotation + 1) % (max + 1);
        currentProp.transform.eulerAngles = new Vector3(0, 0, rotation * rotationMultiplayer);
    }
}
