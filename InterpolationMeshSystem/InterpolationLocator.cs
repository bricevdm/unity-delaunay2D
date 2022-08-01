using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace WizardsRepublic.InterpolationMeshSystem
{
  public class InterpolationLocator : MonoBehaviour
  {
    // ReSharper disable once CollectionNeverUpdated.Local
    [SerializeReference] private List<IBaseData> data;

    private Dictionary<int, IBaseData> dict;

    public T GetData<T>(int dataType) where T : IBaseData
    {
      dict ??= new Dictionary<int, IBaseData>();
      if (dict.TryGetValue(dataType, out var value))
      {
        return (T)value;
      }

      T find = (T)data.Find(x => x.DataType == dataType);
      dict.Add(dataType, find);
      return find;
    }

    public bool HasData(int dataType)
    {
      return dict.ContainsKey(dataType);
    }
    
    public void RefreshData()
    {
      dict = data.ToDictionary(d => d.DataType, d => d);
    }

#if UNITY_EDITOR

    public void SetupData<TBaseDataType>(int dataType) where TBaseDataType:IBaseData
    {
      if (data.Exists(x => x?.DataType == dataType)) return;
      IBaseData instance = Activator.CreateInstance<TBaseDataType>();
      instance.SetDataType(dataType);
      data.Add(instance);
    }
    
    private void OnValidate()
    {
      if (data == null) return;
      
      foreach (IBaseData d in data)
        d?.Setup(this);
    }
    
#endif
  }

  public interface IBaseData
  {
    int DataType { get; }
    void Setup([UsedImplicitly]InterpolationLocator locator);
    void SetDataType(int dataType);
  }
  
  [Serializable]
  public abstract class AInterpolationData : IBaseData
  {
    [SerializeField] private int dataType;
    public int DataType => dataType;
    public virtual void Setup(InterpolationLocator locator) {}
    public void SetDataType(int dataType) => this.dataType = dataType;
  }
  
  [Serializable]
  public class ColorData : AInterpolationData
  {
    [SerializeField] private Color color = Color.white;
    
    public static Color Interpolate(Vector3 coord, ColorData a, ColorData b, ColorData c)
      => coord.x * a.color + coord.y * b.color + coord.z * c.color;    
  }
  
  [Serializable]
  public class FloatData : AInterpolationData
  {
    [SerializeField] private float value;
    
    public static float Interpolate(Vector3 coord, FloatData a, FloatData b, FloatData c)
      => coord.x * a.value + coord.y * b.value + coord.z * c.value;
  }
  
  [Serializable]
  public class Vector2Data : AInterpolationData
  {
    [SerializeField] private Vector2 value;
    
    public static Vector2 Interpolate(Vector3 coord, Vector2Data a, Vector2Data b, Vector2Data c)
      => coord.x * a.value + coord.y * b.value + coord.z * c.value;
  }
  
  [Serializable]
  public class Vector3Data : AInterpolationData
  {
    [SerializeField] private Vector3 value;
    
    public static Vector3 Interpolate(Vector3 coord, Vector3Data a, Vector3Data b, Vector3Data c)
      => coord.x * a.value + coord.y * b.value + coord.z * c.value;
  }
}