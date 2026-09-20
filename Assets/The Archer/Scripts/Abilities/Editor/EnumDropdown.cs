using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace OctoberStudio
{
    public class EnumDropdown : AdvancedDropdown
    {
        protected SerializedProperty property;

        public EnumDropdown(AdvancedDropdownState state, SerializedProperty property) : base(state)
        {
            this.property = property;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Value");

            var names = property.enumDisplayNames;
            foreach (var name in names)
            {
                string fullName;
                string groupName;
                if (name.Contains("_"))
                {
                    var parts = name.Split('_');
                    groupName = parts[0];
                    fullName = parts[1];
                } else
                {
                    fullName = name;
                    groupName = "Other";
                }

#if !UNITY_6000_5_OR_NEWER
                var group = root.children.FirstOrDefault(c => c.name == groupName) ?? new AdvancedDropdownItem(groupName);
                if (!root.children.Contains(group))
                    root.AddChild(group);
#else
                var group = root.childList.FirstOrDefault(c => c.name == groupName) ?? new AdvancedDropdownItem(groupName);
                if (!root.childList.Contains(group))
                    root.AddChild(group);
#endif

                group.AddChild(new EnumDropdownItem(name, fullName));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if(item is EnumDropdownItem enumItem)
            {
                var index = System.Array.IndexOf(property.enumDisplayNames, enumItem.Name);

                if (index >= 0)
                {
                    property.enumValueIndex = index;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        
    }

    public class EnumDropdownItem: AdvancedDropdownItem
    {
        public string Name { get; protected set; }
        public EnumDropdownItem(string name, string displayName): base(displayName)
        {
            Name = name;
        }
    }
}
