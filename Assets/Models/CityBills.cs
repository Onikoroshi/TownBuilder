using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CityBill
{
	public enum BillState { Proposed, InProgress, Complete }
	public enum BillAction { Request, Build }
	public enum BillObject { Road, House, CityCenter }

	public Stack<CityOffice> owners;
	public BillState state;
	public BillAction action;
	public BillObject target;

	public CityBill(CityOffice givenOwner, BillAction requestedAction, BillObject requestedTarget)
	{
		owners = new Stack<CityOffice>();
		owners.Push(givenOwner);
		state = BillState.Proposed;
		action = requestedAction;
		target = requestedTarget;
	}

	public void Begin()
	{
		state = BillState.InProgress;
	}

	public void Finish()
	{
		state = BillState.Complete;
		AssignBack();
	}

	public void Reject()
	{
		state = BillState.Proposed;
		AssignBack();
	}

	public void AssignBack()
	{
		if (owners.Count <= 1) return;

		CityOffice currentOwner = owners.Pop();
		CityOffice prevOwner = owners.Peek();
		if (prevOwner != null && !prevOwner.bills.Contains(this))
		{
			prevOwner.bills.Enqueue(this);
		}
		else owners.Push(currentOwner);
	}

	public void AssignTo(CityOffice nextOwner)
	{
		if (nextOwner != null && nextOwner != owners.Peek())
		{
			owners.Push(nextOwner);
		}
	}

	public bool UnStarted()
	{
		return state == BillState.Proposed;
	}

	public bool Begun()
	{
		return state == BillState.InProgress;
	}

	public bool Done()
	{
		return state == BillState.Complete;
	}
}

public class StructureRequest : CityBill
{
	public Int2 size;

	public StructureRequest(CityOffice requester, BillObject requestedType, Int2 requestedSize) : base(requester, CityBill.BillAction.Request, requestedType)
	{
		size = requestedSize;
	}

	public override string ToString()
	{
		return target + " of size " + size.ToString();
	}
}
