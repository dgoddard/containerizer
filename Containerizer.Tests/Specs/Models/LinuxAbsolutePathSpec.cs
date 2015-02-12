using System;
using Containerizer.Models;
using NSpec;

namespace Containerizer.Tests.Specs.Models
{
    internal class LinuxAbsolutePathSpec : nspec
    {
        public void describe_()
        {
            describe["constructor"] = () =>
            {
                context["invalid absolute path"] = () =>
                {
                    it["throws an exception"] = () =>
                    {
                        bool caughtCorrectException = false;
                        try
                        {
                            new LinuxAbsolutePath(@"asdfsfd");
                        }
                        catch (ArgumentException)
                        {
                            caughtCorrectException = true;
                        }
                        catch (Exception)
                        {
                        }

                        caughtCorrectException.should_be_true();
                    };
                };
            };

            context["path of the form empty string"] = () =>
            {
                it["throws an exception"] = () =>
                {
                    bool caughtCorrectException = false;
                    try
                    {
                        new LinuxAbsolutePath(@"");
                    }
                    catch (ArgumentException)
                    {
                        caughtCorrectException = true;
                    }
                    catch (Exception)
                    {
                    }

                    caughtCorrectException.should_be_true();
                };
            };
            context["path of the form '/*'"] =
                () => { it["sets Value to the path"] = () => { new LinuxAbsolutePath("/hi").Value.should_be("/hi"); }; };
        }
    }
}