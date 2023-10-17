using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace FlowAvalonia.Controls
{
    public class SelectionItemsRepeater : ItemsRepeater
    {
        public static readonly DirectProperty<SelectionItemsRepeater, ObservableCollection<object?>?>
            SelectedItemsProperty =
                AvaloniaProperty.RegisterDirect<SelectionItemsRepeater, ObservableCollection<object?>?>(
                    nameof(SelectedItems),
                    o => o.SelectedItems,
                    (o, v) => o.SelectedItems = v);

        private ObservableCollection<object?>? selectedItems;
        private bool ignoreSelectedItemsChanged;

        public SelectionItemsRepeater()
        {
            selectedItems = new ObservableCollection<object?>();
            Selection = new SelectionModel<object?>
            {
                SingleSelect = false
            };
            Selection.SelectionChanged += SelectionModel_SelectionChanged;
            Selection.SourceReset += Selection_SourceReset;
            Selection.LostSelection += Selection_LostSelection;
            ElementPrepared += SelectionItemsRepeater_ElementPrepared;
            ElementIndexChanged += SelectionItemsRepeater_ElementIndexChanged;
            ElementClearing += SelectionItemsRepeater_ElementClearing;
        }

        public SelectionModel<object?> Selection { get; }

        public ObservableCollection<object?>? SelectedItems
        {
            get => selectedItems;
            set => SetAndRaise(SelectedItemsProperty, ref selectedItems, value);
        }
        //
        // protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        // {
        //     base.OnPropertyChanged(change);
        //
        //     if (change.Property == ItemsSourceProperty)
        //     {
        //         var newItems = change.NewValue.GetValueOrDefault<IEnumerable>();
        //         Selection.Source = newItems as IEnumerable<object?> ?? newItems?.OfType<object?>().ToArray();
        //     }
        //     else if (change.Property == SelectedItemsProperty
        //              && !ignoreSelectedItemsChanged)
        //     {
        //         ignoreSelectedItemsChanged = true;
        //         using (Selection.BatchUpdate())
        //         {
        //             if (change.OldValue.GetValueOrDefault<ObservableCollection<object?>>() is { } oldValue)
        //             {
        //                 oldValue.CollectionChanged -= SelectedItems_CollectionChanged;
        //                 Selection.Clear();
        //             }
        //
        //             if (change.NewValue.GetValueOrDefault<ObservableCollection<object?>>() is { } newValue)
        //             {
        //                 newValue.CollectionChanged += SelectedItems_CollectionChanged;
        //                 foreach (var item in newValue)
        //                 {
        //                     var index = ItemsSourceView?.IndexOf(item);
        //                     if (index >= 0)
        //                     {
        //                         Selection.Select(index.Value);
        //                     }
        //                 }
        //             }
        //         }
        //
        //         ignoreSelectedItemsChanged = false;
        //     }
        // }

        private void SelectedItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (ignoreSelectedItemsChanged)
            {
                return;
            }

            ignoreSelectedItemsChanged = true;

            var selectedItems = (ICollection<object>)sender!;
            if (selectedItems.Count == 0)
            {
                Selection.Clear();
            }
            else if (selectedItems.Count == ItemsSourceView?.Count
                     && Selection.Source is not null)
            {
                Selection.SelectAll();
            }
            else
            {
                using (Selection.BatchUpdate())
                {
                    if (e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        Selection.DeselectRange(0, Selection.Count);
                        foreach (var item in (IList)sender!)
                        {
                            var index = ItemsSourceView?.IndexOf(item);
                            if (index >= 0 && !Selection.IsSelected(index.Value))
                            {
                                Selection.Select(index.Value);
                            }
                        }
                    }

                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            var index = ItemsSourceView?.IndexOf(item);
                            if (index >= 0 && Selection.IsSelected(index.Value))
                            {
                                Selection.Deselect(index.Value);
                            }
                        }
                    }

                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            var index = ItemsSourceView?.IndexOf(item);
                            if (index >= 0 && !Selection.IsSelected(index.Value))
                            {
                                Selection.Select(index.Value);
                            }
                        }
                    }
                }
            }

            ignoreSelectedItemsChanged = false;
        }

        private void Selection_LostSelection(object? sender, EventArgs e)
        {
            if (!ignoreSelectedItemsChanged
                && SelectedItems != null)
            {
                ignoreSelectedItemsChanged = true;
                SelectedItems.Clear();
                ignoreSelectedItemsChanged = false;
            }
        }

        private void Selection_SourceReset(object? sender, EventArgs e)
        {
            if (!ignoreSelectedItemsChanged
                && SelectedItems != null)
            {
                ignoreSelectedItemsChanged = true;
                SelectedItems.Clear();
                ignoreSelectedItemsChanged = false;
            }
        }

        private void SelectionModel_SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<object?> e)
        {
            if (!ignoreSelectedItemsChanged
                && SelectedItems != null)
            {
                ignoreSelectedItemsChanged = true;
                foreach (var index in e.DeselectedIndexes)
                {
                    if (TryGetElement(index) is { } item)
                    {
                        _ = SelectedItems.Remove(item);
                        item.Classes.Set("selected", false);
                    }
                }

                foreach (var index in e.SelectedIndexes)
                {
                    if (TryGetElement(index) is { } item)
                    {
                        if (!SelectedItems.Contains(item))
                        {
                            SelectedItems.Add(item);
                            item.Classes.Set("selected", true);
                        }
                    }
                }

                ignoreSelectedItemsChanged = false;
            }
        }

        private void SelectionItemsRepeater_ElementClearing(object? sender, ItemsRepeaterElementClearingEventArgs e)
        {
            var element = e.Element;
            UpdateContainerSelection(-1, element);
        }

        private void SelectionItemsRepeater_ElementIndexChanged(object? sender,
            ItemsRepeaterElementIndexChangedEventArgs e)
        {
            var index = e.NewIndex;
            var element = e.Element;
            UpdateContainerSelection(index, element);
        }

        private void SelectionItemsRepeater_ElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs e)
        {
            var index = e.Index;
            var element = e.Element;
            UpdateContainerSelection(index, element);
        }

        private void UpdateContainerSelection(int index, Control element)
        {
            if (element != null)
            {
                var isSelected = index != -1 && Selection.IsSelected(index);
                element.Classes.Set("selected", isSelected);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Handled
                || (e.Source is Control source && GetElementIndex(source) < 0))
            {
                return;
            }

            var focus = TopLevel.GetTopLevel(this).FocusManager;
            var currentFocus = focus.GetFocusedElement();

            if (e.Key == Key.Space)
            {
                e.Handled = UpdateSelectionFromEventSource(currentFocus as Interactive, true, false, true);
            }
            else
            {
                var direction = e.Key.ToNavigationDirection();

                if (currentFocus == null
                    || direction == null
                    || !direction.Value.IsDirectional())
                {
                    return;
                }

                var from = currentFocus is Control control && GetElementIndex(control) > 0 ? control
                    : Selection.SelectedIndex != -1 ? GetOrCreateElement(Selection.SelectedIndex)
                    : TryGetElement(0);

                if (FindNextElement(direction.Value, from) is Control next)
                {
                    e.Handled = true;
                    next.Focus(NavigationMethod.Directional, e.KeyModifiers);

                    var multi = (e.KeyModifiers & KeyModifiers.Shift) != 0;
                    var toggle = (e.KeyModifiers & KeyModifiers.Control) != 0;

                    if (!toggle || multi)
                    {
                        _ = UpdateSelectionFromEventSource(next, true, multi, toggle);
                    }

                    next.BringIntoView();
                }
            }

            base.OnKeyUp(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (e.Source is Visual source)
            {
                var point = e.GetCurrentPoint(source);

                if (point.Properties.IsLeftButtonPressed || point.Properties.IsRightButtonPressed)
                {
                    e.Handled = UpdateSelectionFromEventSource(
                        source as Control,
                        true,
                        (e.KeyModifiers & KeyModifiers.Shift) != 0,
                        (e.KeyModifiers & KeyModifiers.Control) != 0,
                        point.Properties.IsRightButtonPressed);
                }
            }
        }

        private (int, bool) lastSingleSelectionIsSelected = (0, false);

        private bool UpdateSelectionFromEventSource(
            Interactive? eventSource,
            bool select = true,
            bool rangeModifier = false,
            bool toggleModifier = false,
            bool rightButton = false)
        {
            var index = GetContainerIndexFromEventSource(eventSource);

            if (index < 0 || index >= ItemsSourceView?.Count)
            {
                return false;
            }

            var mode = SelectionMode.Multiple;
            var multi = (mode & SelectionMode.Multiple) != 0;
            var toggle = (toggleModifier || (mode & SelectionMode.Toggle) != 0);
            var range = multi && rangeModifier;

            if (!select)
            {
                Selection.Deselect(index);
                lastSingleSelectionIsSelected = (index, false);
            }
            else if (rightButton)
            {
                if (Selection.IsSelected(index) == false)
                {
                    using var operation = Selection.BatchUpdate();
                    Selection.Clear();
                    Selection.Select(index);
                    lastSingleSelectionIsSelected = (index, true);
                }
            }
            else if (range)
            {
                using var operation = Selection.BatchUpdate();
                var (anchorIndex, isSelected) = lastSingleSelectionIsSelected;
                if (anchorIndex < 0)
                {
                    anchorIndex = Selection.AnchorIndex;
                }

                if (!toggle)
                {
                    Selection.Clear();
                    Selection.SelectRange(anchorIndex, index);
                }
                else
                {
                    if (isSelected)
                    {
                        Selection.SelectRange(anchorIndex, index);
                    }
                    else
                    {
                        Selection.DeselectRange(anchorIndex, index);
                    }
                }
            }
            else if (multi && toggle)
            {
                if (Selection.IsSelected(index) == true)
                {
                    Selection.Deselect(index);
                    lastSingleSelectionIsSelected = (index, false);
                }
                else
                {
                    Selection.Select(index);
                    lastSingleSelectionIsSelected = (index, true);
                }
            }
            else
            {
                using var operation = Selection.BatchUpdate();
                Selection.Clear();
                Selection.Select(index);
                lastSingleSelectionIsSelected = (index, true);
            }

            return true;
        }

        protected int GetContainerIndexFromEventSource(Interactive? eventSource)
        {
            for (var current = eventSource as Visual; current != null; current = current.GetVisualParent())
            {
                if (current is Control control
                    && control.GetLogicalParent() == this
                    && GetElementIndex(control) is var index and >= 0)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Retrieves the element that should receive focus based on the specified navigation direction.
        /// </summary>
        /// <param name="direction">The direction to move in.</param>
        /// <returns>The next element, or null if no element was found.</returns>
        public static InputElement? FindNextElement(NavigationDirection direction, InputElement current)
        {
            var container = current.GetVisualParent();

            if (container is null)
            {
                return null;
            }

            if (container is ICustomKeyboardNavigation custom)
            {
                var (handled, next) = custom.GetNext(current, direction);

                if (handled)
                {
                    return (InputElement)next;
                }
            }

            static InputElement? GetFirst(Visual container)
            {
                return container.GetVisualChildren().FirstOrDefault(x => x is InputElement ie && ie.CanFocus()) as
                    InputElement;
            }

            static InputElement? GetLast(Visual container)
            {
                return container.GetVisualChildren().LastOrDefault(x => x is InputElement ie && ie.CanFocus()) as
                    InputElement;
                ;
            }

            return direction switch
            {
                NavigationDirection.Next => TabNavigation.GetNextInTabOrder(current, direction),
                NavigationDirection.Previous => TabNavigation.GetNextInTabOrder(current, direction),
                NavigationDirection.First => GetFirst(container),
                NavigationDirection.Last => GetLast(container),
                NavigationDirection.PageDown => null,
                NavigationDirection.PageUp => null,
                _ => FindInDirection(container, current, direction),
            };
        }

        private static InputElement? FindInDirection(
            Visual container,
            InputElement from,
            NavigationDirection direction)
        {
            static double Distance(NavigationDirection direction, InputElement from, InputElement to)
            {
                return direction switch
                {
                    NavigationDirection.Left => from.Bounds.Right - to.Bounds.Right,
                    NavigationDirection.Right => to.Bounds.X - from.Bounds.X,
                    NavigationDirection.Up => from.Bounds.Bottom - to.Bounds.Bottom,
                    NavigationDirection.Down => to.Bounds.Y - from.Bounds.Y,
                    _ => throw new NotSupportedException("direction must be Up, Down, Left or Right"),
                };
            }

            InputElement? result = null;
            var resultDistance = double.MaxValue;

            foreach (var visual in container.GetVisualChildren())
            {
                if (visual is not InputElement child || child == from || !child.CanFocus()) continue;

                var distance = Distance(direction, from, child);

                if (distance > 0 && distance < resultDistance)
                {
                    result = child;
                    resultDistance = distance;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// The implementation for default tab navigation.
    /// </summary>
    internal static class TabNavigation
    {
        /// <summary>
        /// Gets the next control in the specified tab direction.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="direction">The tab direction. Must be Next or Previous.</param>
        /// <param name="outsideElement">
        /// If true will not descend into <paramref name="element"/> to find next control.
        /// </param>
        /// <returns>
        /// The next element in the specified direction, or null if <paramref name="element"/>
        /// was the last in the requested direction.
        /// </returns>
        public static InputElement? GetNextInTabOrder(
            InputElement element,
            NavigationDirection direction,
            bool outsideElement = false)
        {
            element = element ?? throw new ArgumentNullException(nameof(element));

            if (direction != NavigationDirection.Next && direction != NavigationDirection.Previous)
            {
                throw new ArgumentException("Invalid direction: must be Next or Previous.");
            }

            var container = element.GetVisualParent<InputElement>();

            if (container != null)
            {
                var mode = KeyboardNavigation.GetTabNavigation((InputElement)container);

                return mode switch
                {
                    KeyboardNavigationMode.Continue =>
                        GetNextInContainer(element, container, direction, outsideElement)
                        ?? GetFirstInNextContainer(element, element, direction),
                    KeyboardNavigationMode.Cycle =>
                        GetNextInContainer(element, container, direction, outsideElement)
                        ?? GetFocusableDescendant(container, direction),
                    KeyboardNavigationMode.Contained => GetNextInContainer(element, container, direction,
                        outsideElement),
                    _ => GetFirstInNextContainer(element, container, direction),
                };
            }
            else
            {
                return GetFocusableDescendants(element, direction).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the first or last focusable descendant of the specified element.
        /// </summary>
        /// <param name="container">The element.</param>
        /// <param name="direction">The direction to search.</param>
        /// <returns>The element or null if not found.##</returns>
        private static InputElement? GetFocusableDescendant(InputElement container, NavigationDirection direction)
        {
            return direction == NavigationDirection.Next
                ? GetFocusableDescendants(container, direction).FirstOrDefault()
                : GetFocusableDescendants(container, direction).LastOrDefault();
        }

        /// <summary>
        /// Gets the focusable descendants of the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="direction">The tab direction. Must be Next or Previous.</param>
        /// <returns>The element's focusable descendants.</returns>
        private static IEnumerable<InputElement> GetFocusableDescendants(InputElement element,
            NavigationDirection direction)
        {
            var mode = KeyboardNavigation.GetTabNavigation((InputElement)element);

            if (mode == KeyboardNavigationMode.None)
            {
                yield break;
            }

            var children = element.GetVisualChildren().OfType<InputElement>();

            if (mode == KeyboardNavigationMode.Once)
            {
                var active = KeyboardNavigation.GetTabOnceActiveElement(element);

                if (active is InputElement activeElement)
                {
                    yield return activeElement;
                    yield break;
                }
                else
                {
                    children = children.Take(1);
                }
            }

            foreach (var child in children)
            {
                var (handled, next) = GetCustomNext(child, direction);

                if (handled)
                {
                    yield return next!;
                }
                else
                {
                    if (child.CanFocus() && KeyboardNavigation.GetIsTabStop((InputElement)child))
                    {
                        yield return child;
                    }

                    if (child.CanFocusDescendants())
                    {
                        foreach (var descendant in GetFocusableDescendants(child, direction))
                        {
                            if (KeyboardNavigation.GetIsTabStop((InputElement)descendant))
                            {
                                yield return descendant;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the next item that should be focused in the specified container.
        /// </summary>
        /// <param name="element">The starting element/</param>
        /// <param name="container">The container.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="outsideElement">
        /// If true will not descend into <paramref name="element"/> to find next control.
        /// </param>
        /// <returns>The next element, or null if the element is the last.</returns>
        private static InputElement? GetNextInContainer(
            InputElement element,
            InputElement container,
            NavigationDirection direction,
            bool outsideElement)
        {
            var e = element;

            if (direction == NavigationDirection.Next && !outsideElement)
            {
                var descendant = GetFocusableDescendants(element, direction).FirstOrDefault();

                if (descendant != null)
                {
                    return descendant;
                }
            }

            if (container != null)
            {
                // TODO: Do a spatial search here if the container doesn't implement
                // INavigableContainer.
                if (container is INavigableContainer navigable)
                {
                    while (e != null)
                    {
                        e = (InputElement?)navigable.GetControl(direction, e, false);

                        if (e != null &&
                            e.CanFocus() &&
                            KeyboardNavigation.GetIsTabStop((InputElement)e))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // TODO: Do a spatial search here if the container doesn't implement
                    // INavigableContainer.
                    e = null;
                }

                if (e != null && direction == NavigationDirection.Previous)
                {
                    var descendant = GetFocusableDescendants(e, direction).LastOrDefault();

                    if (descendant != null)
                    {
                        return descendant;
                    }
                }

                return e;
            }

            return null;
        }

        /// <summary>
        /// Gets the first item that should be focused in the next container.
        /// </summary>
        /// <param name="element">The element being navigated away from.</param>
        /// <param name="container">The container.</param>
        /// <param name="direction">The direction of the search.</param>
        /// <returns>The first element, or null if there are no more elements.</returns>
        private static InputElement? GetFirstInNextContainer(
            InputElement element,
            InputElement container,
            NavigationDirection direction)
        {
            var parent = container.GetVisualParent<InputElement>();
            InputElement? next = null;

            if (parent != null)
            {
                if (direction == NavigationDirection.Previous &&
                    parent.CanFocus() &&
                    KeyboardNavigation.GetIsTabStop((InputElement)parent))
                {
                    return parent;
                }

                var allSiblings = parent.GetVisualChildren()
                    .OfType<InputElement>()
                    .Where(FocusExtensions.CanFocusDescendants);
                var siblings = direction == NavigationDirection.Next
                    ? allSiblings.SkipWhile(x => x != container).Skip(1)
                    : allSiblings.TakeWhile(x => x != container).Reverse();

                foreach (var sibling in siblings)
                {
                    var customNext = GetCustomNext(sibling, direction);
                    if (customNext.handled)
                    {
                        return customNext.next;
                    }

                    if (sibling.CanFocus() && KeyboardNavigation.GetIsTabStop((InputElement)sibling))
                    {
                        return sibling;
                    }

                    next = direction == NavigationDirection.Next
                        ? GetFocusableDescendants(sibling, direction).FirstOrDefault()
                        : GetFocusableDescendants(sibling, direction).LastOrDefault();

                    if (next != null)
                    {
                        return next;
                    }
                }

                next = GetFirstInNextContainer(element, parent, direction);
            }
            else
            {
                next = direction == NavigationDirection.Next
                    ? GetFocusableDescendants(container, direction).FirstOrDefault()
                    : GetFocusableDescendants(container, direction).LastOrDefault();
            }

            return next;
        }

        private static (bool handled, InputElement? next) GetCustomNext(InputElement element,
            NavigationDirection direction)
        {
            if (element is ICustomKeyboardNavigation custom)
            {
                var (handled, iNext) = custom.GetNext(element, direction);
                return (handled, (InputElement?)iNext);
            }

            return (false, null);
        }
    }

    internal static class FocusExtensions
    {
        /// <summary>
        /// Checks if the specified element can be focused.
        /// </summary>
        /// <param name="e">The element.</param>
        /// <returns>True if the element can be focused.</returns>
        public static bool CanFocus(this InputElement e)
        {
            return e.Focusable && e.IsEffectivelyEnabled && e.IsVisible;
        }

        /// <summary>
        /// Checks if descendants of the specified element can be focused.
        /// </summary>
        /// <param name="e">The element.</param>
        /// <returns>True if descendants of the element can be focused.</returns>
        public static bool CanFocusDescendants(this InputElement e)
        {
            return e.IsEffectivelyEnabled && e.IsVisible;
        }
    }
}