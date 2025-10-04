using UnityEngine;

public interface IEater
{
    float Size { get; }
    bool CanEat(IEatable target);
    void Eat(IEatable target);
}