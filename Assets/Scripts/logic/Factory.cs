using System;
using UnityEngine;
using System.Collections;

public class Factory
{
    public event Action<int> OnMoneyChanged;

    private int _money;


    public Vector3Int Position { get; private set; }
    public int Money 
    { 
        get => _money;
        set
        {
            if (_money != value)
            {
                _money = value;
                OnMoneyChanged?.Invoke(_money);
            }
        }
    }
    public FactoryDefinition Definition { get; private set; }

    public Factory(Vector3Int position, FactoryDefinition definition)
    {
        this.Position = position;
        this.Definition = definition;
        this.Money = 0;
    }
}