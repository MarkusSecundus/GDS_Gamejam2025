using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using UnityEngine;

public enum SpellType
{
	Neutral, Good, Bad
}

public abstract class AbstractSpell : AbstractProjectileController
{
	[field: SerializeField] public SpellType Type { get; private set; }
	public AbstractSpell()
	{
		this._canHitOtherBullets = false;
	}
}






