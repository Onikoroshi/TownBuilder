using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CityOffice
{
	public City city;
	public Queue<CityBill> bills;

	public CityOffice(City givenCity)
	{
		city = givenCity;
		bills = new Queue<CityBill>();
	}

	public World ourWorld()
	{
		return city.world;
	}

	public void HandleNextUnfinishedBill()
	{
		onHandleNextUnfinishedBill();
	}

	protected virtual void onHandleNextUnfinishedBill()
	{
		CityBill toHandle = getNextUnfinishedBill();

		if (toHandle == null) return;
	}

	protected CityBill getNextUnfinishedBill()
	{
		return onGetNextUnfinishedBill();
	}

	protected virtual CityBill onGetNextUnfinishedBill()
	{
		if (bills == null || bills.Count == 0) return null;

		CityBill found = bills.Dequeue();

		while (found != null && found.Done())
		{
			found = bills.Dequeue();
		}

		return found;
	}

	public void requestNextBill()
	{
		onRequestNextBill();
	}

	protected virtual void onRequestNextBill()
	{
		// Add an appropriate bill to the list.
	}
}
