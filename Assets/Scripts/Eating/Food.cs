using UnityEngine;

public class Food  : EatableEntity
{

    public override bool CanBeEatenBy(IEater eater)
    {
        return true; 
    }
}
