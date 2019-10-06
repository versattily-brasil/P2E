﻿// -----------------------------------------------------------------------
// <copyright file="LabelElement.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Elements
{
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Implements a label element.
    /// </summary>
    internal class LabelElement : FormElementElement
    {
        /// <summary>
        /// The element associated with this label element.
        /// </summary>
        private HtmlElement associatedElement = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelElement"/> class.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> associated with this element.</param>
        public LabelElement(XElement element)
            : base(element)
        { }

        /// <summary>
        /// Gets the value of the input element associated with this label element.
        /// </summary>
        public HtmlElement For
        {
            get
            {
                if (this.associatedElement == null)
                {
                    string id = Element.GetAttributeCI("for");
                    if (id == null)
                    {
                        return null;
                    }

                    var element = Element.Document.Descendants().Where(e => e.GetAttributeCI("id") == id).FirstOrDefault();
                    if (element == null)
                    {
                        return null;
                    }

                    this.associatedElement = OwningBrowser.CreateHtmlElement<HtmlElement>(element);
                }

                return this.associatedElement;
            }
        }

        /// <summary>
        /// Perform a click action on the label element.
        /// </summary>
        /// <returns>The <see cref="ClickResult"/> of the operation.</returns>
        public override ClickResult Click()
        {
            if (Disabled)
            {
                return ClickResult.SucceededNoOp;
            }

            base.Click();

            // Click on the associated (For) item or else return success without any operation
            return For?.Click() ?? ClickResult.SucceededNoOp;
        }
    }
}