using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using PathLib;

namespace Infernity.Framework.Core.Io.Paths
{
    /// <summary>
    /// Base class for common methods in concrete paths.
    /// </summary>
    public abstract class ConcretePath<TPath, TPurePath> : IPath<TPath, TPurePath>
        where TPath : ConcretePath<TPath, TPurePath>, IPath
        where TPurePath : IPurePath<TPurePath>
    {
        protected readonly TPurePath PurePath;

        protected ConcretePath(TPurePath purePath)
        {
            PurePath = purePath;
        }

        /// <inheritdoc/>
        public FileInfo? FileInfo
        {
            get
            {
                if (_fileInfoCache != null)
                {
                    return _fileInfoCache;
                }
                if (Exists() && !IsFile())
                {
                    return null;
                }

                var stringPath = ToString();

                if (stringPath != null)
                {
                    _fileInfoCache = new FileInfo(stringPath);
                    return _fileInfoCache;
                }

                return null;
            }
        }

        private FileInfo? _fileInfoCache;

        /// <inheritdoc/>
        public DirectoryInfo? DirectoryInfo
        {
            get
            {
                if (_directoryInfoCache != null)
                {
                    return _directoryInfoCache;
                }
                if (Exists() && !IsDir())
                {
                    return null;
                }

                var stringPath = ToString();

                if (stringPath != null)
                {
                    _directoryInfoCache = new DirectoryInfo(stringPath);
                    return _directoryInfoCache;
                }

                return null;
            }
        }

        private DirectoryInfo? _directoryInfoCache;

        /// <inheritdoc/>
        public bool Exists()
        {
            return IsDir() || IsFile();
        }

        /// <inheritdoc/>
        public bool IsFile()
        {
            return File.Exists(PurePath.ToPosix());
        }

        /// <inheritdoc/>
        public bool IsDir()
        {
            return System.IO.Directory.Exists(PurePath.ToPosix());
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> ListDir()
        {
            return ListDir("*", SearchOption.TopDirectoryOnly);
        }

        /// <inheritdoc/>
        IEnumerable<IPath> IPath.ListDir()
        {
            return ListDir().Select(p => (IPath)p);
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> ListDir(string pattern)
        {
            return ListDir(pattern, SearchOption.TopDirectoryOnly);
        }

        IEnumerable<IPath> IPath.ListDir(string pattern)
        {
            return ListDir(pattern).Select(p => (IPath)p);
        }

        public IEnumerable<TPath> ListDir(SearchOption scope)
        {
            return ListDir("*", scope);
        }

        IEnumerable<IPath> IPath.ListDir(SearchOption scope)
        {
            return ListDir(scope).Select(p => (IPath)p);
        }

        public IEnumerable<TPath> ListDir(string pattern, SearchOption scope)
        {
            if (!IsDir())
            {
                throw new ArgumentException("Glob may only be called on directories.");
            }

            var directoryInfo = DirectoryInfo;

            if(directoryInfo == null)
            {
                throw new InvalidOperationException("DirectoryInfo is null, cannot list directory contents.");
            }

            foreach (var dir in directoryInfo.GetDirectories(pattern, scope))
            {
                var path = PathFactory(dir.FullName);

                if (path != null)
                {
                    yield return path;
                }
            }
            foreach (var file in directoryInfo.GetFiles(pattern, scope))
            {
                var path = PathFactory(file.FullName);

                if (path != null)
                {
                    yield return path;
                }
            }
        }

        IEnumerable<IPath> IPath.ListDir(string pattern, SearchOption scope)
        {
            return ListDir(pattern, scope).Select(p => (IPath)p);
        }

        protected abstract TPath? PathFactory(params string[] path);

        protected abstract TPath? PathFactory(TPurePath? path);

        protected abstract TPath? PathFactory(IPurePath? path);

        /// <inheritdoc/>
        public void Mkdir(bool makeParents = false)
        {
            // Iteratively check whether or not each directory in the path exists
            // and create them if they do not.
            if (makeParents)
            {
                foreach (var dir in Parents())
                {
                    if(!dir.IsDir())
                    {
                        var path = dir.ToString();

                        if (path != null)
                        {
                            System.IO.Directory.CreateDirectory(path);
                        }
                    }
                }
            }
            if (!IsDir())
            {
                System.IO.Directory.CreateDirectory(PurePath.ToPosix());
            }
        }

        /// <inheritdoc/>
        public void Delete(bool recursive = false)
        {
            if (!Exists())
            {
                return;
            }
            var file = FileInfo;
            if (file != null)
            {
                file.Delete();
                return;
            }
            var dir = DirectoryInfo;
            if (dir != null)
            {
                dir.Delete(recursive);
            }
        }

        /// <inheritdoc/>
        public FileStream Open(FileMode mode)
        {
            return File.Open(PurePath.ToString() ?? throw new InvalidOperationException("Invalid path"), mode);
        }

        /// <inheritdoc/>
        public string ReadAsText()
        {
            return File.ReadAllText(PurePath.ToString() ?? throw new InvalidOperationException("Invalid path"));
        }

        /// <inheritdoc/>
        public abstract TPath ExpandUser();

        IPath IPath.ExpandUser()
        {
            return ExpandUser();
        }

        IPath? IPath.ExpandUser(IPath homeDir)
        {
            return ExpandUser(homeDir);
        }

        /// <inheritdoc/>
        public TPath? ExpandUser(IPath? homeDir)
        {
            if (homeDir == null || PurePath.IsAbsolute())
            {
                return (TPath)this;
            }

            var parts = new List<string>();
            parts.AddRange(PurePath.Parts);
            if (parts.Count == 0 || parts[0] != "~")
            {
                return (TPath)this;
            }
            parts.RemoveAt(0);

            return PathFactory(homeDir.Join(parts.ToArray()));
        }

        IPath? IPath.ExpandEnvironmentVars()
        {
            return ExpandEnvironmentVars();
        }

        /// <inheritdoc/>
        public TPath? ExpandEnvironmentVars()
        {
            return PathFactory(
                Environment.ExpandEnvironmentVariables(ToString()?? throw new InvalidOperationException("Invalid path")));
        }

        /// <inheritdoc/>
        /// <inheritdoc/>
        public IDisposable SetCurrentDirectory()
        {
            return new CurrentDirectorySetter(PurePath.ToString() ?? throw new InvalidOperationException("Invalid path"));
        }

        private class CurrentDirectorySetter : IDisposable
        {
            private readonly string _oldCwd;
            private readonly string _newCwd;

            public CurrentDirectorySetter(string newCwd)
            {
                _oldCwd = Environment.CurrentDirectory;
                Environment.CurrentDirectory = _newCwd = newCwd;
            }
            public void Dispose()
            {
                if (Environment.CurrentDirectory == _newCwd)
                {
                    Environment.CurrentDirectory = _oldCwd;
                }
            }
        }


        #region IPurePath -> IPath implementation

        IPath? IPath.Join(params string[] paths)
        {
            return Join(paths);
        }

        IPath? IPath.Join(params IPurePath[] paths)
        {
            return Join(paths);
        }

        public static TPath operator/(ConcretePath<TPath, TPurePath> lvalue, IPath rvalue)
        {
            return lvalue.Join(rvalue) ?? throw new InvalidOperationException($"Cannot join {lvalue} with {rvalue}.");
        }

        public static TPath operator/(ConcretePath<TPath, TPurePath> lvalue, string rvalue)
        {
            return lvalue.Join(rvalue) ?? throw new InvalidOperationException($"Cannot join {lvalue} with {rvalue}.");
        }

        IPath? IPath.NormCase()
        {
            return NormCase();
        }

        IPath? IPath.NormCase(CultureInfo currentCulture)
        {
            return NormCase(currentCulture);
        }

        IPath? IPath.Parent()
        {
            return Parent();
        }

        IPath? IPath.Parent(int nthParent)
        {
            return Parent(nthParent);
        }

        IEnumerable<IPath> IPath.Parents()
        {
            return Parents().Select(p => (IPath) p);
        }

        IPath? IPath.Relative()
        {
            return Relative();
        }

        IPath? IPath.RelativeTo(IPurePath parent)
        {
            return RelativeTo(parent);
        }

        IPath? IPath.WithDirname(string newDirName)
        {
            return WithDirname(newDirName);
        }

        IPath? IPath.WithDirname(IPurePath newDirName)
        {
            return WithDirname(newDirName);
        }

        IPath? IPath.WithFilename(string newFilename)
        {
            return WithFilename(newFilename);
        }

        IPath? IPath.WithExtension(string newExtension)
        {
            return WithExtension(newExtension);
        }

        #endregion


        #region IPurePath implementation

        /// <inheritdoc/>
        public string? Dirname => PurePath.Dirname;

        /// <inheritdoc/>
        public string? Directory => PurePath.Directory;

        /// <inheritdoc/>
        public string? Filename => PurePath.Filename;

        /// <inheritdoc/>
        public string? Basename => PurePath.Basename;

        /// <inheritdoc/>
        public string BasenameWithoutExtensions => PurePath.BasenameWithoutExtensions;

        /// <inheritdoc/>
        public string? Extension => PurePath.Extension;

        /// <inheritdoc/>
        public string[] Extensions => PurePath.Extensions;

        /// <inheritdoc/>
        public string? Root => PurePath.Root;

        /// <inheritdoc/>
        public string? Drive => PurePath.Drive;

        /// <inheritdoc/>
        public string? Anchor => PurePath.Anchor;

        /// <inheritdoc/>
        public string ToPosix()
        {
            return PurePath.ToPosix();
        }

        /// <inheritdoc/>
        public bool IsAbsolute()
        {
            return PurePath.IsAbsolute();
        }

        /// <inheritdoc/>
        public bool IsReserved()
        {
            return PurePath.IsReserved();
        }

        /// <inheritdoc/>
        IPurePath IPurePath.Join(params string[] paths)
        {
            return PurePath.Join(paths);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.Join(params IPurePath[] paths)
        {
            return PurePath.Join(paths);
        }

        /// <inheritdoc/>
        bool IPurePath.TrySafeJoin(string relativePath, [NotNullWhen(true)]out IPurePath? joined)
        {
            return PurePath.TrySafeJoin(relativePath, out joined);
        }

        /// <inheritdoc/>
        bool IPurePath.TrySafeJoin(IPurePath relativePath, [NotNullWhen(true)]out IPurePath? joined)
        {
            return PurePath.TrySafeJoin(relativePath, out joined);
        }

        /// <inheritdoc/>
        public bool Match(string pattern)
        {
            return PurePath.Match(pattern);
        }

        /// <inheritdoc/>
        public IEnumerable<string> Parts => PurePath.Parts;

        /// <inheritdoc/>
        IPurePath IPurePath.NormCase()
        {
            return PurePath.NormCase();
        }

        /// <inheritdoc/>
        IPurePath IPurePath.NormCase(CultureInfo currentCulture)
        {
            return PurePath.NormCase(currentCulture);
        }

        /// <inheritdoc/>
        IPurePath? IPurePath.Parent()
        {
            return PurePath.Parent();
        }

        /// <inheritdoc/>
        IPurePath? IPurePath.Parent(int nthParent)
        {
            return PurePath.Parent(nthParent);
        }

        /// <inheritdoc/>
        IEnumerable<IPurePath> IPurePath.Parents()
        {
            return PurePath.Parents().Select(p => (IPurePath)p);
        }

        /// <inheritdoc/>
        public Uri ToUri()
        {
            return PurePath.ToUri();
        }

        /// <inheritdoc/>
        IPurePath IPurePath.Relative()
        {
            return PurePath.Relative();
        }

        /// <inheritdoc/>
        IPurePath IPurePath.RelativeTo(IPurePath parent)
        {
            return PurePath.RelativeTo(parent);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.WithDirname(string newDirName)
        {
            return PurePath.WithDirname(newDirName);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.WithDirname(IPurePath newDirName)
        {
            return PurePath.WithDirname(newDirName);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.WithFilename(string newFilename)
        {
            return PurePath.WithFilename(newFilename);
        }

        /// <inheritdoc/>
        IPurePath IPurePath.WithExtension(string newExtension)
        {
            return PurePath.WithExtension(newExtension);
        }

        /// <inheritdoc/>
        public bool HasComponents(PathComponent components)
        {
            return PurePath.HasComponents(components);
        }

        /// <inheritdoc/>
        public string GetComponents(PathComponent components)
        {
            return PurePath.GetComponents(components);
        }

        #endregion


        #region TPurePath implementation

        /// <inheritdoc/>
        public bool TrySafeJoin(string path, [NotNullWhen(true)]out TPurePath? joined)
        {
            return PurePath.TrySafeJoin(path, out joined);
        }

        /// <inheritdoc/>
        public bool TrySafeJoin(IPurePath path, [NotNullWhen(true)]out TPurePath? joined)
        {
            return PurePath.TrySafeJoin(path, out joined);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.Join(params string[] paths)
        {
            return PurePath.Join(paths);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.Join(params IPurePath[] paths)
        {
            return PurePath.Join(paths);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.NormCase()
        {
            return PurePath.NormCase();
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.NormCase(CultureInfo currentCulture)
        {
            return PurePath.NormCase(currentCulture);
        }

        /// <inheritdoc/>
        TPurePath? IPurePath<TPurePath>.Parent()
        {
            return PurePath.Parent();
        }

        /// <inheritdoc/>
        TPurePath? IPurePath<TPurePath>.Parent(int nthParent)
        {
            return PurePath.Parent(nthParent);
        }

        /// <inheritdoc/>
        IEnumerable<TPurePath> IPurePath<TPurePath>.Parents()
        {
            return PurePath.Parents();
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.Relative()
        {
            return PurePath.Relative();
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.RelativeTo(IPurePath parent)
        {
            return PurePath.RelativeTo(parent);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.WithDirname(string newDirName)
        {
            return PurePath.WithDirname(newDirName);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.WithDirname(IPurePath newDirName)
        {
            return PurePath.WithDirname(newDirName);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.WithFilename(string newFilename)
        {
            return PurePath.WithFilename(newFilename);
        }

        /// <inheritdoc/>
        TPurePath IPurePath<TPurePath>.WithExtension(string newExtension)
        {
            return PurePath.WithExtension(newExtension);
        }

        #endregion


        #region TPath implementation

        /// <inheritdoc/>
        public TPath? Join(params string[] paths)
        {
            return PathFactory(PurePath.Join(paths));
        }

        /// <inheritdoc/>
        public TPath? Join(params IPurePath[] paths)
        {
            return PathFactory(PurePath.Join(paths));
        }

        /// <inheritdoc/>
        public TPath? NormCase()
        {
            return PathFactory(PurePath.NormCase());
        }

        /// <inheritdoc/>
        public TPath? NormCase(CultureInfo currentCulture)
        {
            return PathFactory(PurePath.NormCase(currentCulture));
        }

        /// <inheritdoc/>
        public TPath? Parent()
        {
            return PathFactory(PurePath.Parent());
        }

        /// <inheritdoc/>
        public TPath? Parent(int nthParent)
        {
            return PathFactory(PurePath.Parent(nthParent));
        }

        /// <inheritdoc/>
        public IEnumerable<TPath> Parents()
        {
            return PurePath.Parents().Select(PathFactory).Where(p => p != null).Cast<TPath>();
        }

        /// <inheritdoc/>
        public TPath? Relative()
        {
            return PathFactory(PurePath.Relative());
        }

        /// <inheritdoc/>
        public TPath? RelativeTo(IPurePath parent)
        {
            return PathFactory(PurePath.RelativeTo(parent));
        }

        /// <inheritdoc/>
        public TPath? WithDirname(string newDirName)
        {
            return PathFactory(PurePath.WithDirname(newDirName));
        }

        /// <inheritdoc/>
        public TPath? WithDirname(IPurePath newDirName)
        {
            return PathFactory(PurePath.WithDirname(newDirName));
        }

        /// <inheritdoc/>
        public TPath? WithFilename(string newFilename)
        {
            return PathFactory(PurePath.WithFilename(newFilename));
        }

        /// <inheritdoc/>
        public TPath? WithExtension(string newExtension)
        {
            return PathFactory(PurePath.WithExtension(newExtension));
        }

        #endregion

        public bool Equals(IPath? other)
        {
            return PurePath switch
            {
                null => false,
                PurePosixPath lppp when other is PosixPath rppp => lppp.Equals(rppp.PurePath),
                PureWindowsPath lpwp when other is WindowsPath rpwp => lpwp.Equals(rpwp.PurePath),
                _ => false
            };
        }
    }
}
