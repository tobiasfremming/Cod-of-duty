using System;
using UnityEngine;

public class Fish : EatableEntity, IEater
{

	[SerializeField] private float growthRate = 0.1f;
	[SerializeField] private float eatSizeThreshold = 1.5f;
 	[SerializeField] private Collider eatCollider;
    
    [SerializeField] private float scaleLerpSpeed = 5f;
    private Vector3 targetScale;

    private Hunger hunger;

    private void Start()
    {
	    targetScale = transform.localScale;
	    hunger = GetComponent<Hunger>();
    }

    private void Update()
    {
	    transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleLerpSpeed * Time.deltaTime);
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
        targetScale = Vector3.one * size;
		Debug.Log($"Fish grew to size: {size}");
        target.OnEaten(this);

        bool targetIsPlayer = target.gameObject.GetComponent<Hunger>() != null;
        if (targetIsPlayer)
        {
	        GameStateManager.Instance.EndGame();
        }
        
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
