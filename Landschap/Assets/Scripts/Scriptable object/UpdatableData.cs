﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject {

    public event System.Action OnValuesUpdated;

    #if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
        
    }

    public void NotifyOfUpdatedValues()
    {
         UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

    #endif

}
