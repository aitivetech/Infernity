using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Infernity.Framework.Core.Io.Paths;

// ReSharper disable once CheckNamespace
namespace PathLib
{
    /// <summary>
    /// Concrete path implementation for Posix machines (Linux, Unix, Mac).
    /// Unusable on other systems.
    /// </summary>
    public sealed class PosixPath : ConcretePath<PosixPath, PurePosixPath>, IEquatable<PosixPath>
    {
        /// <summary>
        /// Create a new path object for Posix-compliant machines.
        /// </summary>
        /// <param name="paths"></param>
        public PosixPath(params string[] paths)
            : base(new PurePosixPath(paths))
        {
        }

        /// <summary>
        /// Create a new path object for Posix-compliant machines.
        /// </summary>
        /// <param name="path"></param>
        public PosixPath(PurePosixPath path)
            : base(path)
        {
        }

        public PosixPath EnsureDirectory()
        {
            if (!Exists())
            {
                Mkdir(true);
            }

            return this;
        }

        public PosixPath EnsureParentDirectoryOnly()
        {
            var parent = Parent();
            
            if(parent != null && !parent.Exists())
            {
                parent.Mkdir(true);
            }

            return this;
        }

        /// <inheritdoc/>
        protected override PosixPath PathFactory(params string[] path)
        {
            if (path == null)
            {
                throw new NullReferenceException("Path passed to factory cannot be null");
            }

            return new PosixPath(path);
        }

        /// <inheritdoc/>
        protected override PosixPath PathFactory(PurePosixPath? path)
        {
            if (path == null)
            {
                throw new NullReferenceException("Path passed to factory cannot be null");
            }

            return new PosixPath(path);
        }

        /// <inheritdoc/>
        protected override PosixPath PathFactory(IPurePath? path)
        {
            if (path == null)
            {
                throw new NullReferenceException("Path passed to factory cannot be null");
            }

            var purePath = path as PurePosixPath;
            if (purePath != null)
            {
                return new PosixPath(purePath);
            }

            var parts = new List<string>();
            parts.AddRange(path.Parts);
            return new PosixPath(parts.ToArray());
        }

        public static implicit operator PosixPath(string path)
        {
            return new PosixPath(path);
        }
        
        /// <inheritdoc/>
        public override PosixPath ExpandUser()
        {
            var homeDir = new PurePosixPath("~");
            if (homeDir < PurePath)
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (!String.IsNullOrEmpty(home))
                {
                    return new PosixPath(
                        new PurePosixPath(home).Join(PurePath.RelativeTo(homeDir)));
                }

                throw new ApplicationException("Unable to find home directory for user");
            }

            return this;
        }

        /// <inheritdoc/>
        public bool Equals(PosixPath? other)
        {
            if (other is null)
            {
                return false;
            }
            return PurePath.Equals(other.PurePath);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return PurePath.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return PurePath.ToString();
        }
    }
}
