﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;

namespace Versattily.WebDriver
{
	public class SimpleTargetLocator : ITargetLocator
	{
		public SimpleTargetLocator(IBrowser browser)
		{
			_browser = browser;
		}
		private IBrowser _browser = null;

		#region ITargetLocator Members

		public IWebElement ActiveElement()
		{
			return null;
		}

		public IAlert Alert()
		{
			return null;
		}

		public IWebDriver DefaultContent()
		{
			return null;
		}

		public IWebDriver Frame(IWebElement frameElement)
		{
			string windowHandle = frameElement.GetAttribute("Versattily.WebDriver:frameWindowHandle");
			return Frame(windowHandle);
		}

		public IWebDriver Frame(string frameName)
		{
			var frame = _browser.Frames.FirstOrDefault(b => b.WindowHandle == frameName);
			return new VersattilyDriver(frame);
		}

		public IWebDriver Frame(int frameIndex)
		{
			var frame = _browser.Frames.ToList()[frameIndex];
			return new VersattilyDriver(frame);
		}

		public IWebDriver Window(string windowName)
		{
			var window = _browser.Browsers.FirstOrDefault(b => b.WindowHandle == windowName);
			return new VersattilyDriver(window);
		}

        public IWebDriver ParentFrame()
        {
            throw new NotImplementedException();
        }

		#endregion
    }
}
