using UnityEngine;


public interface IEatable
{
    float Size { get; }
    float NutritionValue { get; }
    bool CanBeEatenBy(IEater eater);
    void OnEaten(IEater eater);
}
