using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR.Database
{
	public interface IDatabaseIndexable
	{
		DBIdentity DatabaseIdentity
		{
			get;
		}
	}
}
