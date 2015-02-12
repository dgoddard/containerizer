using System;
using Containerizer.Models;
using NSpec;

namespace Containerizer.Tests.Specs.Models
{
    internal class LinuxAbsoluteUserPathSpec : nspec
    {
        public void describe_()
        {
            describe[".FromLinuxAbsolutePath"] = () =>
            {
                it["prepends /user to the path"] = () =>
                {
                    var result = LinuxAbsoluteUserPath.FromLinuxAbsolutePath(new LinuxAbsolutePath("/hi"));
                    result.Value.should_be("/user/hi");
                };

                it["prepends /user to if the path is  /user"] = () =>
                {
                    var result = LinuxAbsoluteUserPath.FromLinuxAbsolutePath(new LinuxAbsolutePath("/user"));
                    result.Value.should_be("/user/user");
                };
            };

            describe["constructor"] = () =>
            {
                context["invalid absolute path"] = () =>
                {
                    it["throws an exception"] = () =>
                    {
                        bool caughtCorrectException = false;
                        try
                        {
                            new LinuxAbsoluteUserPath(@"asdfsfd");
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

            context["path of the form '/' (without user)"] = () =>
            {
                it["throws an exception"] = () =>
                {
                    bool caughtCorrectException = false;
                    try
                    {
                        new LinuxAbsoluteUserPath(@"/hi");
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

            context["path of the form empty string"] = () =>
            {
                it["throws an exception"] = () =>
                {
                    bool caughtCorrectException = false;
                    try
                    {
                        new LinuxAbsoluteUserPath(@"");
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
            context["path of the form '/user'"] =
                () =>
                {
                    it["sets Value to the path"] =
                        () => { new LinuxAbsoluteUserPath("/user/hi").Value.should_be("/user/hi"); };
                };
        }
    }
}