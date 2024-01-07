using System;
using System.Reflection;

namespace Suzuryg.LocalSwitchGenerator
{
    internal static class ReflectionUtility
    {
        public static void SetValue<T>(object targetObject, string fieldName, T newValue)
        {
            // Get the type of the target object
            Type targetType = targetObject.GetType();

            // Get the FieldInfo for the private field
            FieldInfo fieldInfo = targetType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                throw new InvalidOperationException($"Field '{fieldName}' not found in type {targetType}.");
            }

            // Set the new value to the field
            fieldInfo.SetValue(targetObject, newValue);
        }

        public static void AddElementsToArray<T>(object targetObject, string fieldName, T[] newElements)
        {
            // Get the type of the target object
            Type targetType = targetObject.GetType();

            // Get the FieldInfo for the private field
            FieldInfo fieldInfo = targetType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                throw new InvalidOperationException($"Field '{fieldName}' not found in type {targetType}.");
            }

            // Get the current array value of the field
            T[] currentArray = fieldInfo.GetValue(targetObject) as T[];
            if (currentArray != null)
            {
                // Create a new array with additional elements
                T[] newArray = new T[currentArray.Length + newElements.Length];
                currentArray.CopyTo(newArray, 0);
                newElements.CopyTo(newArray, currentArray.Length);

                // Set the new array back to the field
                fieldInfo.SetValue(targetObject, newArray);
            }
            else
            {
                // Set the new array to the field
                fieldInfo.SetValue(targetObject, newElements);
            }
        }
    }
}
