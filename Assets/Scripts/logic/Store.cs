using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Store
{
    public event Action<Order> OnOrderCreated;

    public Vector3Int Position { get; private set; }
    public Guid Id { get; private set; } = Guid.NewGuid();
    public List<Order> Orders { get; private set; }


    public StoreDefinition Definition { get; private set; }

    public Store(Vector3Int position, StoreDefinition definition)
    {
        this.Position = position;
        this.Definition = definition;
    }

    public bool CanCreateOrder()
    {
        float roll = UnityEngine.Random.Range(0f, 1f);
        return roll <= Definition.OrderChance;
    }

    public Order CreateOrder()
    {
        var reward = UnityEngine.Random.Range(Definition.RewardRange.x, Definition.RewardRange.y + 1);
        var newOrder = new Order(reward, this);
        Orders.Add(newOrder);
        OnOrderCreated?.Invoke(newOrder);
        return newOrder;
    }

    public bool HasPendingOrders => Orders.Count > 0;

    public class Order
    {
        public int Reward { get; private set; }
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Store Store { get; private set; }

        public Order(int reward, Store store)
        {
            this.Reward = reward;
        }

        public void Complete() {
            Store.Orders.Remove(this);
        }
    }
}

