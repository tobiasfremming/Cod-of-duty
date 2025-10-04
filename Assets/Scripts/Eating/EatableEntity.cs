using UnityEngine;
public abstract class EatableEntity : MonoBehaviour, IEatable
{
    [SerializeField] protected float nutritionValue = 10f;
    [SerializeField] protected float size = 1f;
  
    public virtual float NutritionValue => nutritionValue;
    public float Size => size;

    public abstract bool CanBeEatenBy(IEater eater);

    public virtual void OnEaten(IEater eater)
    {
        Destroy(gameObject);
    }
    
    public void SetSize(float newSize)
    {
        size = newSize;
    }
}