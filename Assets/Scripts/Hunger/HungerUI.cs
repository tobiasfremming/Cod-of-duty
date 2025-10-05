using UnityEngine;
using UnityEngine.UI;

public class HungerUI : MonoBehaviour
{

    public Slider slider;

    [SerializeField] private Hunger hunger;
    

    private void Start()
    {
        slider.maxValue = hunger.GetMaxHunger();
    }
    
    private void Update()
    {
        if (!hunger) return;
        SetHunger(hunger.GetHunger());
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
