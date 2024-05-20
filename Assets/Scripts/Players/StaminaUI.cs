using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    public Slider slider;
    public GameObject heroKnight;
    public Gradient gradient;
    public Image fill;

    // Start of class
    public void Start()
    {
        SetMaxStamina(10);
    }

    /// <summary>
    /// Set this scripts knight gameobject to real knight gameobject
    /// </summary>
    /// <param name="gameObject">Knight gameobject</param>
    public void SetPlayerGameObject(GameObject gameObject) 
    {
        heroKnight = gameObject;
    }

    // Checking the stamina of player
    public void Update()
    {
        if (heroKnight.transform.GetComponent<Paladin>().currentStamina <= 10) 
        {
            SetStamina(heroKnight.transform.GetComponent<Paladin>().currentStamina);
        }
    }

    // Setting stamina at start
    public void SetMaxStamina(int stamina) 
    {
        slider.maxValue = stamina;
        slider.value = stamina;
        fill.color = gradient.Evaluate(10);
    }

    // Set stamina
    public void SetStamina(int stamina) 
    {
        slider.value = stamina;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
