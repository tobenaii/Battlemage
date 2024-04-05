using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Waddle.AbilitySystem.Attributes.Data;

namespace Waddle.AbilitySystem.Attributes.Authoring
{
    [CreateAssetMenu(menuName = "Waddle/AbilitySystem/Attribute Set", fileName = "AttributeSet")]
    public class AttributeSet : ScriptableObject
    {
        [Serializable]
        private class AttributeWithUIBindings
        {
            [SerializeField] private WaddleAttribute _attribute;
            
            //TODO: Decouple these bindings from the attribute set
            [SerializeField, LabelText("Local Player Bindings")] private bool _generateLocalPlayerBinding;
            [SerializeField, LabelText("Remote Player Bindings")] private bool _generateRemotePlayerBindings;
            [SerializeField, ShowIf(nameof(_generateLocalPlayerBinding))] private AttributeBindingObject _localPlayerBinding;
            [SerializeField, ShowIf(nameof(_generateRemotePlayerBindings))] private AttributeBindingObject _remotePlayerBindings;
        }
        
        [SerializeField, ListDrawerSettings(
            CustomAddFunction = nameof(AddAttribute), 
            CustomRemoveIndexFunction = nameof(RemoveAttribute),
            OnBeginListElementGUI = nameof(BeginDrawListElement), 
            OnEndListElementGUI = nameof(EndDrawListElement))]
        private List<WaddleAttribute> _attributes;
        
        public IReadOnlyList<WaddleAttribute> Attributes => _attributes;
        
        private void AddAttribute()
        {
            var attribute = CreateInstance<WaddleAttribute>();
            attribute.name = $"Attribute {_attributes.Count}";
            _attributes.Add(attribute);
            
            AssetDatabase.AddObjectToAsset(attribute, this);
            AssetDatabase.SaveAssets();
        }

        private void RemoveAttribute(int index)
        {
            var attribute = _attributes[index];
            _attributes.RemoveAt(index);
            AssetDatabase.RemoveObjectFromAsset(attribute);
            AssetDatabase.SaveAssets();
        }
        
        private void BeginDrawListElement(int index)
        {
            _attributes[index].name = EditorGUILayout.TextField("Name", _attributes[index].name);
        }
        
        private void EndDrawListElement(int index)
        {
        }
    }
}