using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LabelPreviewer
{
    public static class VisualTreeHelperExtensions
    {
        /// <summary>
        /// Finds a child of a given item in the visual tree.
        /// </summary>
        public static T FindChild<T>(this DependencyObject parent)
            where T : DependencyObject
        {
            // Confirm parent is valid.
            if (parent == null) return null;

            // First child
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                // If the child is the requested type, return it
                if (child != null && child is T)
                    return (T)child;

                // Otherwise, recursively drill down the tree
                T foundChild = FindChild<T>(child);
                if (foundChild != null)
                    return foundChild;
            }

            return null;
        }

        /// <summary>
        /// Finds all children of a given type in the visual tree.
        /// </summary>
        public static IEnumerable<T> FindChildren<T>(this DependencyObject parent)
            where T : DependencyObject
        {
            // Confirm parent is valid.
            if (parent == null) yield break;

            // First child
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                // If the child is the requested type, return it
                if (child != null && child is T)
                    yield return (T)child;

                // Recursively drill down the tree
                foreach (T foundChild in FindChildren<T>(child))
                    yield return foundChild;
            }
        }
    }
}