using System;
using UnityEngine;

public class Fish : EatableEntity, IEater
{

	[SerializeField] private float growthRate = 0.1f;
	[SerializeField] private float eatSizeThreshold = 1.5f;
 	[SerializeField] private Collider eatCollider;

    private Hunger hunger;

    private void Start()
    {
	    hunger = GetComponent<Hunger>();
    }

    public override bool CanBeEatenBy(IEater eater)
    {
        return eater.Size > this.Size * eatSizeThreshold;
    }

    public bool CanEat(IEatable target)
    {
        return target != this && target.CanBeEatenBy(this);
    }

    public void Eat(IEatable target)
    {
        if (!CanEat(target)) return;
		
        size += (target.NutritionValue * growthRate);
        transform.localScale = Vector3.one * size;
		Debug.Log($"Fish grew to size: {size}");
        target.OnEaten(this);
        
        if(!hunger) return;
        hunger.IncreaseHunger(target.NutritionValue);
    }

	void OnTriggerEnter(Collider other)
	{
		
    	var eatable = other.GetComponent<IEatable>();
	    
	    if (eatable == null) return;
	    
    	if (CanEat(eatable))
    	{
        	Eat(eatable);
    	}
	}
}
