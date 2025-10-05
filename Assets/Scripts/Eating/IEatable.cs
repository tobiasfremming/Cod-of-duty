using UnityEngine;


public interface IEatable
{
    GameObject gameObject { get; }
    float Size { get; }
    float NutritionValue { get; }
    bool CanBeEatenBy(IEater eater);
    void OnEaten(IEater eater);
}
