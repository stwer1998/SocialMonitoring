using Models;
using PlaywrightSharp;
using System;
using System.Threading;

namespace Parser.Extentions
{
    public static class PageExtentions
    {
        public static Result ExtClickElement(this IPage page, string selector, int timeout = 0) 
        {
            try 
            {
                var element = page.QuerySelectorAsync(selector).Result;
                if (element == null)
                {
                    return Result.Fail("Element not found");
                }
                element.ClickAsync().Wait();
                if (timeout > 0)
                {
                    Thread.Sleep(timeout);
                }
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public static Result ExtPrintTextToElement(this IPage page, string selector, string text, int timeout = 0) 
        {
            try
            {
                var element = page.QuerySelectorAsync(selector).Result;
                if (element == null)
                {
                    return Result.Fail("Element not found");
                }
                element.TypeAsync(text).Wait();
                if (timeout > 0)
                {
                    Thread.Sleep(timeout);
                }
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public static Result ExtFillTextToElement(this IPage page, string selector, string text, int timeout = 0)
        {
            try
            {
                var element = page.QuerySelectorAsync(selector).Result;
                if (element == null)
                {
                    return Result.Fail("Element not found");
                }
                element.FillAsync(text).Wait();
                if (timeout > 0)
                {
                    Thread.Sleep(timeout);
                }
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public static Result ExtFocusToElement(this IPage page, string selector, int timeout = 0) 
        {
            try
            {
                var element = page.QuerySelectorAsync(selector).Result;
                if (element == null)
                {
                    return Result.Fail("Element not found");
                }
                element.FocusAsync().Wait();
                if (timeout > 0)
                {
                    Thread.Sleep(timeout);
                }
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public static Result ExtClearinput(this IPage page, string selector, int sybolsnum)
        {
            try
            {
                var element = page.QuerySelectorAsync(selector).Result;
                if (element == null)
                {
                    return Result.Fail("Element not found");
                }
                element.FocusAsync().Wait();

                for (int i = 0; i < sybolsnum; i++)
                {
                    page.Keyboard.PressAsync("Delete");
                }
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

    }
}
