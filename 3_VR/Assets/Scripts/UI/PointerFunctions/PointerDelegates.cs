using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerDelegates : MonoBehaviour {

	public delegate void voidDelegate();
	public delegate void voidV3Delegate(Vector3 v);

	public voidDelegate OnEnter;
	public voidDelegate OnHover;
	public voidV3Delegate OnHoverDragStart;
	public voidDelegate OnHoverDragEnd;
	public voidDelegate OnExit;
	public voidDelegate OnSet;

	public void Enter() {
		if (OnEnter != null) OnEnter();
	}

	public void Hover() {
		if (OnHover != null) OnHover(); 
	}

	public void HoverDragStart(Vector3 startPosition) {
		if (OnHoverDragStart != null) OnHoverDragStart(startPosition);
	}

	public void HoverDragEnd() {
		if (OnHoverDragEnd != null) OnHoverDragEnd();
	}

	public void Exit() {
		if (OnExit != null) OnExit();
	}

	public void Set() {
		if (OnSet != null) OnSet();
	}
}
