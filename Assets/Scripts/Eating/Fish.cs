using UnityEngine;

public class Fish : EatableEntity, IEater
{

	[SerializeField] private float growthRate = 1.5f;
 	[SerializeField] private Collider eatCollider;
 	   
	public override bool CanBeEatenBy(IEater eater)
    {
        return eater.Size > this.Size * 1.2f;
    }

    public bool CanEat(IEatable target)
    {
        return target != this && target.CanBeEatenBy(this);
    }

    public void Eat(IEatable target)
    {
        if (!CanEat(target)) return;

        size += target.NutritionValue * growthRate;
		Debug.Log($"Fish grew to size: {size}");

        target.OnEaten(this);
    }

	void OnTriggerEnter(Collider other)
	{
    	var eatable = other.GetComponent<IEatable>();
    	if (eatable != null && CanEat(eatable))
    	{
        	Eat(eatable);
    	}
	}
}
