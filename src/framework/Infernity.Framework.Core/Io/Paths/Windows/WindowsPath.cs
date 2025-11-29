using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Infernity.Framework.Core.Io.Paths;
using Infernity.Framework.Core.Io.Paths.Windows;

// ReSharper disable once CheckNamespace
namespace PathLib
{
    /// <summary>
    /// Concrete path implementation for Windows machines. Unusable on
    /// other systems.
    /// </summary>
    [TypeConverter(typeof(WindowsPathConverter))]
    public sealed class WindowsPath : ConcretePath<WindowsPath, PureWindowsPath>, IEquatable<WindowsPath>
    {
        private const string ExtendedLengthPrefix = @"\\?\";

        /// <summary>
        /// Create a new path object for Windows machines.
        /// </summary>
        /// <param name="paths"></param>
        public WindowsPath(params string[] paths)
            : base(new PureWindowsPath(paths))
        {
        }

        /// <summary>
        /// Create a new path object for Windows machines.
        /// </summary>
        /// <param name="path"></param>
        public WindowsPath(PureWindowsPath path)
            : base(path)
        {
        }

        /// <inheritdoc/>
        protected override WindowsPath PathFactory(params string[] paths)
        {
            if (paths == null)
            {
                throw new NullReferenceException(
                    "Path passed to factory cannot be null.");
            }
            return new WindowsPath(paths);
        }

        /// <inheritdoc/>
        protected override WindowsPath PathFactory(PureWindowsPath? path)
        {
            if (path == null)
            {
                throw new NullReferenceException(
                    "Path passed to factory cannot be null.");
            }
            return new WindowsPath(path);
        }

        /// <inheritdoc/>
        protected override WindowsPath PathFactory(IPurePath? path)
        {
            if (path == null)
            {
                throw new NullReferenceException(
                    "Path passed to factory cannot be null.");
            }
            var purePath = path as PureWindowsPath;
            if (purePath != null)
            {
                return new WindowsPath(purePath);
            }
            var parts = new List<string>();
            parts.AddRange(path.Parts);
            return new WindowsPath(parts.ToArray());
        }

        /// <inheritdoc/>
        public override WindowsPath ExpandUser()
        {
            var homeDir = new PureWindowsPath("~");
            if (homeDir < PurePath)
            {
                var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");

                if (userProfile == null)
                {
                    return this;
                }

                var newDir = new PureWindowsPath(userProfile);
                return new WindowsPath(newDir.Join(PurePath.RelativeTo(homeDir)));
            }
            return this;
        }

        #region Equality Members

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(WindowsPath first, WindowsPath second)
        {
            return ReferenceEquals(first, null) ?
                ReferenceEquals(second, null) :
                first.Equals(second);
        }

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for inequality.
        /// Case insensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator !=(WindowsPath first, WindowsPath second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Return true if <paramref name="first"/> is a parent path
        /// of <paramref name="second"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator <(WindowsPath first, WindowsPath second)
        {
            return first.PurePath < second.PurePath;
        }

        /// <summary>
        /// Return true if <paramref name="second"/> is a parent of
        /// <paramref name="first"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator >(WindowsPath first, WindowsPath second)
        {
            return second < first;
        }

        /// <summary>
        /// Return true if <paramref name="first"/> is equal to or a parent
        /// of <paramref name="second"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator <=(WindowsPath first, WindowsPath second)
        {
            return first.PurePath <= second.PurePath;
        }

        /// <summary>
        /// Return true if <paramref name="second"/> is equal to or a parent
        /// of <paramref name="first"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator >=(WindowsPath first, WindowsPath second)
        {
            return first.PurePath >= second.PurePath;
        }

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(WindowsPath? other)
        {
            if (other is null)
            {
                return false;
            }
            return PurePath.Equals(other.PurePath);
        }

        /// <summary>
        /// Compare two <see cref="PureWindowsPath"/> for equality.
        /// Case insensitive.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object? other)
        {
            var obj = other as WindowsPath;
            return !ReferenceEquals(obj, null) && Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return PurePath.GetHashCode();
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return PurePath.ToString();
        }
    }
}
