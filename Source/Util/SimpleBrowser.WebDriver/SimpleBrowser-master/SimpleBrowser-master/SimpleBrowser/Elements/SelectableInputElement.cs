﻿// -----------------------------------------------------------------------
// <copyright file="SelectableInputElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    /// <summary>
    /// Implements an abstract selectable input element, not corresponding with any specific input type.
    /// </summary>
    internal abstract class SelectableInputElement : InputElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectableInputElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public SelectableInputElement(XElement element)
            : base(element)
        { }

        /// <summary>
        /// Gets or sets a value indicating whether the selectable input element is selected.
        /// </summary>
        public virtual bool Selected { get; set; }

        /// <summary>
        /// Gets the form values to submit for this input
        /// </summary>
        /// <param name="isClickedElement">True, if the action to submit the form was clicking this element. Otherwise, false.</param>
        /// <returns>A collection of <see cref="UserVariableEntry"/> objects.</returns>
        public override IEnumerable<UserVariableEntry> ValuesToSubmit(bool isClickedElement, bool validate)
        {
            if (this.Selected && !string.IsNullOrEmpty(this.Name) && !this.Disabled)
            {
                yield return new UserVariableEntry() { Name = Name, Value = string.IsNullOrEmpty(this.Value) ? "on" : this.Value };
            }

            yield break;
        }
    }
}