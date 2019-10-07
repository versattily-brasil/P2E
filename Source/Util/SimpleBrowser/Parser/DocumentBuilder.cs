﻿// -----------------------------------------------------------------------
// <copyright file="DocumentBuilder.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;

    public class DocumentBuilder
    {
        /// <summary>
        /// Defines all of the tags that are self-closing (i.e., "empty" in HTML 4 or "void" in HTML 5.)
        /// </summary>
        /// <remarks>
        /// All tags exist in both HTML 4 and 5, except the following:
        /// HTML 4 only: basefont, frame, isindex
        /// HTML 5 only: embed, keygen, source, track, wbr
        /// </remarks>
        private static readonly string[] SelfClosing = new[] { "area", "base", "basefont", "br", "col", "command", "embed", "frame", "hr", "img", "input", "isindex", "keygen", "link", "meta", "param", "source", "track", "wbr" };

        /// <summary>
        /// Defines tags that are not allowed to be children of themself.
        /// </summary>
        /// <remarks>
        /// When attempting to render malformed HTML, these tags are not allowed to be nested within themselves. In
        /// cases where they are found, the previous opening tag is closed before the new tag is opened. Therefore,
        /// this:
        /// <select>
        ///     <option>1
        ///     <option>2
        /// </select>
        /// Is rendered as this:
        /// <select>
        ///     <option>1</option>
        ///     <option>2</option>
        /// </select>
        /// Rather than this:
        /// <select>
        ///     <option>1
        ///         <option>2</option>
        ///     </option>
        /// </select>
        /// </remarks>
        private static readonly string[] SiblingOnly = new[] { "select", "option", "optgroup" };

        private readonly List<HtmlParserToken> _tokens;
        private XDocument _doc;

        private DocumentBuilder(List<HtmlParserToken> tokens)
        {
            this._tokens = tokens;
            string doctype = string.Empty;
            HtmlParserToken doctypeToken = tokens.Where(t => t.Type == TokenType.DocTypeDeclaration).FirstOrDefault();
            if (doctypeToken != null)
            {
                doctype = doctypeToken.Raw;
            }

            try
            {
                this._doc = XDocument.Parse(string.Format("<?xml version=\"1.0\"?>{0}<html />", doctype));
            }
            catch (XmlException)
            {
                // System.Xml.Linq.XDocument throws an XmlException if it encounters a DOCTYPE it
                // can't parse. If this occurs, do not use the DOCTYPE from the page.
                this._doc = XDocument.Parse("<?xml version=\"1.0\"?><html />");
            }
            if (this._doc.DocumentType != null)
            {
#if !__MonoCS__
                this._doc.DocumentType.InternalSubset = null;
#endif
            }
        }

        public static XDocument Parse(List<HtmlParserToken> tokens)
        {
            DocumentBuilder hdb = new DocumentBuilder(tokens);
            hdb.Assemble();
            return hdb._doc;
        }

        private string SanitizeElementName(string name)
        {
            if (name.Contains(":"))
            {
                name = name.Substring(name.LastIndexOf(":") + 1);
            }

            return name.ToLowerInvariant();
        }

        private int _index;

        private void Assemble()
        {
            Stack<XElement> stack = new Stack<XElement>();
            Func<XElement> topOrRoot = () => stack.Count == 0 ? this._doc.Root : stack.Peek();
            while (this._index < this._tokens.Count)
            {
                HtmlParserToken token = this._tokens[this._index++];
                switch (token.Type)
                {
                    case TokenType.Element:
                        {
                            string name = this.SanitizeElementName(token.A);
                            if (SiblingOnly.Contains(name))
                            {
                                this.CloseElement(stack, name);
                            }

                            XElement current = null;
                            if (name == "html")
                            {
                                current = topOrRoot();
                            }
                            else
                            {
                                current = new XElement(name);
                                topOrRoot().Add(current);
                            }

                            this.ReadAttributes(current);
                            if (!SelfClosing.Contains(name))
                            {
                                stack.Push(current);
                            }

                            break;
                        }

                    case TokenType.CloseElement:
                        {
                            this.CloseElement(stack, this.SanitizeElementName(token.A));

                            break;
                        }

                    case TokenType.Comment:
                        {
                            topOrRoot().Add(new XComment(token.A));
                            break;
                        }

                    case TokenType.Cdata:
                        {
                            topOrRoot().Add(new XCData(token.A));
                            break;
                        }

                    case TokenType.Text:
                        {
                            XElement parent = topOrRoot();
                            if (parent.Name.LocalName.Equals("textarea", StringComparison.InvariantCultureIgnoreCase) ||
                                parent.Name.LocalName.Equals("pre", StringComparison.InvariantCultureIgnoreCase))
                            {
                                parent.Add(new XText(token.Raw));
                            }
                            else
                            {
                                parent.Add(new XText(token.A));
                            }

                            break;
                        }
                }
            }
        }

        private static readonly Regex RxValidAttrName = new Regex(@"^[A-Za-z_][A-Za-z0-9_\-\.]*$");

        private void ReadAttributes(XElement current)
        {
            while (this._index < this._tokens.Count && this._tokens[this._index].Type == TokenType.Attribute)
            {
                HtmlParserToken token = this._tokens[this._index++];
                string name = token.A.ToLowerInvariant();
                name = name.Replace(':', '_');
                if (name == "xmlns")
                {
                    name += "_";
                }

                if (RxValidAttrName.IsMatch(name))
                {
                    current.SetAttributeValue(name, HttpUtility.HtmlDecode(token.B ?? token.A ?? string.Empty));
                }
            }
        }

        private void CloseElement(Stack<XElement> stack, string name)
        {
            if (stack.Any(x => x.Name == name))
            {
                do
                {
                    XElement x = stack.Pop();
                    if (x.Name == name)
                    {
                        break;
                    }
                } while (stack.Count > 0);
            }
        }
    }
}