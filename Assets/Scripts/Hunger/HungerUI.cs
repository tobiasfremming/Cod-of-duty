using UnityEngine;
using UnityEngine.UI;

public class HungerUI : MonoBehaviour
{

    public Slider slider;

    [SerializeField] private Hunger hunger;
    

    private void Start()
    {
        // Try to find Hunger component if not assigned
        if (hunger == null)
        {
            hunger = FindObjectOfType<Hunger>();
        }
        
        if (hunger != null && slider != null)
        {
            slider.maxValue = hunger.GetMaxHunger();
        }
        else
        {
            Debug.LogWarning("HungerUI: Missing Hunger or Slider component!");
        }
    }
    
    private void Update()
    {
        // Only update if both hunger and slider are assigned
        if (hunger != null && slider != null)
        {
            SetHunger(hunger.GetHunger());
        }
    }

    public void SetHunger(float newHunger)
    {
        slider.value = newHunger;
    }
    
    public void SetHungerMax(float hungerMax)
    {
        slider.maxValue = hungerMax;
    }
}
