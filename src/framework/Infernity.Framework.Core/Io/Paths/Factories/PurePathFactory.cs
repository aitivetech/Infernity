
using System.Diagnostics.CodeAnalysis;

namespace Infernity.Framework.Core.Io.Paths.Factories
{
    /// <summary>
    /// Creates IPurePath implementations depending on the current platform.
    /// </summary>
    public class PurePathFactory
    {
        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public IPurePath Create(params string[] paths)
        {
            return Create(new PurePathFactoryOptions(), paths) ?? throw new ArgumentException($"Can't create path from parts: {string.Join(',',paths)}");
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public IPurePath? Create(PurePathFactoryOptions options, params string[] paths)
        {
            IPurePath? ret = null;
            switch (PlatformChooser.GetPlatform())
            {
                case Platform.Posix:
                    ret =  new PurePosixPath(paths);
                    break;
                case Platform.Windows:
                    ret = new PureWindowsPath(paths);
                    break;
            }

            if (ret != null)
            {

                return ApplyOptions(ret, options);
            }

            return null;
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public IPurePath Create(string path)
        {
            return Create(path, new PurePathFactoryOptions()) ?? throw new ArgumentException($"Can't create path {path}");
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryCreate(string path, [NotNullWhen(true)]out IPurePath? result)
        {
            return TryCreate(path, new PurePathFactoryOptions(), out result);
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IPurePath? Create(string?path, PurePathFactoryOptions options)
        {
            IPurePath? ret = null;

            if (path != null)
            {
                switch (PlatformChooser.GetPlatform())
                {
                    case Platform.Posix:
                        ret = new PurePosixPath(path);
                        break;
                    case Platform.Windows:
                        ret = new PureWindowsPath(path);
                        break;
                }
            }

            if (ret != null)
            {
                return ApplyOptions(ret, options);
            }

            return null;
        }

        /// <summary>
        /// Factory method to create a new <see cref="PurePath"/> instance
        /// based upon the current operating system.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryCreate(string path, PurePathFactoryOptions options, [NotNullWhen(true)]out IPurePath? result)
        {
            result = null;
            switch (PlatformChooser.GetPlatform())
            {
                case Platform.Posix:
                    if (PurePosixPath.TryParse(path, out var purePosixPath))
                    {
                        result = purePosixPath;
                        break;
                    }
                    return false;
                case Platform.Windows:
                    if (PureWindowsPath.TryParse(path, out var pureWindowsPath))
                    {
                        result = pureWindowsPath;
                        break;
                    }
                    return false;
            }

            if (result != null)
            {
                result = ApplyOptions(result, options);
                return true;
            }

            return false;
        }

        private static IPurePath ApplyOptions(IPurePath path, PurePathFactoryOptions options)
        {
            if (options.AutoNormalizeCase)
            {
                path = path.NormCase(options.Culture ?? System.Globalization.CultureInfo.CurrentCulture);
            }
            return path;
        }
    }
}
