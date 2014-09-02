#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fungus.Script
{

	public class Sequence : MonoBehaviour 
	{
		[HideInInspector]
		public Rect nodeRect = new Rect(10, 10, 100, 40);

		[HideInInspector]
		public string description = "";

		[System.NonSerialized]
		public FungusScript fungusScript;

		[System.NonSerialized]
		public FungusCommand activeCommand;

		int executionCount;
	
		public virtual void Start()
		{
			fungusScript = GetFungusScript();
		}

		public FungusScript GetFungusScript()
		{
			FungusScript sc = null;
			Transform parent = transform.parent;		
			while (parent != null)
			{
				sc = parent.gameObject.GetComponent<FungusScript>();
		
				if (sc != null)
				{
					break;
				}
			}
			return sc;
		}

		public bool HasError()
		{
			FungusCommand[] commands = GetComponents<FungusCommand>();
			foreach (FungusCommand command in commands)
			{
				if (command.errorMessage.Length > 0)
				{
					return true;
				}
			}

			return false;
		}

		public bool IsRunning()
		{
			if (fungusScript == null ||
			    fungusScript.executingSequence == null)
			{
				return false;
			}

			return (fungusScript.executingSequence == this);
		}

		public int GetExecutionCount()
		{
			return executionCount;
		}

		public void ExecuteNextCommand(FungusCommand currentCommand = null)
		{
			if (currentCommand == null)
			{
				executionCount++;
			}

			activeCommand = null;
			FungusCommand nextCommand = null;

			bool executeNext = (currentCommand == null);
			FungusCommand[] commands = GetComponents<FungusCommand>();
			foreach (FungusCommand command in commands)
			{
				if (command == currentCommand)
				{
					executeNext = true;
				}
				else if (executeNext)
				{
					if (command.enabled)
					{
						nextCommand = command;
						break;
					}
				}
			}

			if (nextCommand == null)
			{
				Stop();
			}
			else
			{
				if (GetFungusScript().stepTime == 0f)
				{
					activeCommand = nextCommand;
					nextCommand.Execute();
				}
				else
				{
					StartCoroutine(ExecuteAfterDelay(nextCommand, GetFungusScript().stepTime));
				}
			}

		}

		IEnumerator ExecuteAfterDelay(FungusCommand command, float delay)
		{
			activeCommand = command;
			yield return new WaitForSeconds(delay);
			command.Execute();
		}

		public void Stop()
		{
			activeCommand = null;
			fungusScript.executingSequence = null;
			fungusScript.selectedSequence = null;
			fungusScript.selectedCommand = null;
		}
	}
}
