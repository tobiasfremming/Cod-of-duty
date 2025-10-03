using UnityEngine;
using UnityEngine.UI;

public class HungerUI : MonoBehaviour
{

    public Slider slider;

    [SerializeField] private Hunger hunger;
    

    private void Start()
    {
        slider.maxValue = 100f;
    }
    
    private void Update()
    {
        SetHunger(hunger.GetHunger());
    }

    public void SetHunger(float hunger)
    {
        slider.value = hunger;
    }
    
    public void SetHungerMax(float hungerMax)
    {
        slider.maxValue = hungerMax;
    }
}
