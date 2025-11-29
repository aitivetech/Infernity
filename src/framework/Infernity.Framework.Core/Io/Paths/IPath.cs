using System.ComponentModel;
using System.Globalization;

using Infernity.Framework.Core.Io.Paths.Converters;

namespace Infernity.Framework.Core.Io.Paths
{
    /// <summary>
    /// IPath implementations are platform dependent and
    /// should not be used on any platform but the one
    /// they were designed for.
    /// </summary>
    [TypeConverter(typeof(PathFactoryConverter))]
    public interface IPath : IPurePath, IEquatable<IPath>
    {
        /// <summary>
        /// Retrieve the FileInfo object for this path or null
        /// if exists and not a file.
        /// </summary>
        FileInfo? FileInfo { get; }

        /// <summary>
        /// Retrieve the DirectoryInfo object for this path or null
        /// if exists and not a file.
        /// </summary>
        DirectoryInfo? DirectoryInfo { get; }

        /// <summary>
        /// Return true if the path exists.
        /// </summary>
        /// <returns></returns>
        bool Exists();

        /// <summary>
        /// Return true if the path is a directory.
        /// </summary>
        /// <returns></returns>
        bool IsDir();

        /// <summary>
        /// List the files in the directory.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPath> ListDir();

        /// <summary>
        /// Glob the given pattern in the directory
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     The path being globbed is a file, not a directory.
        /// </exception>
        /// <param name="pattern">
        /// A pattern to match. The special character '*' will match any
        /// number of characters while '?' will match one character.
        /// </param>
        /// <returns></returns>
        IEnumerable<IPath> ListDir(string pattern);

        /// <summary>
        /// Glob the given pattern in the directory, with the specified scope.
        /// </summary>
        /// <exception cref="ArgumentException">Glob was called on a file.</exception>
        /// <param name="scope">Whether to search in subdirectories or not.</param>
        /// <returns></returns>
        IEnumerable<IPath> ListDir(SearchOption scope);

        /// <summary>
        /// Glob the given pattern in the directory, with the specified scope.
        /// </summary>
        /// <exception cref="ArgumentException">Glob was called on a file.</exception>
        /// <param name="pattern">
        /// A pattern to match. The special character '*' will match any
        /// number of characters while '?' will match one character.
        /// </param>
        /// <param name="scope">Whether to search in subdirectories or not.</param>
        /// <returns></returns>
        IEnumerable<IPath> ListDir(string pattern, SearchOption scope);

        /// <summary>
        /// Return true if the path is a file.
        /// </summary>
        /// <returns></returns>
        bool IsFile();

        /// <summary>
        /// Create a new directory at the given path.
        /// </summary>
        /// <param name="makeParents"></param>
        void Mkdir(bool makeParents = false);

        /// <summary>
        /// Delete the file or directory represented by the path.
        /// If a directory, recursively delete all child files too.
        /// </summary>
        /// <param name="recursive"></param>
        void Delete(bool recursive = false);

        /// <summary>
        /// Open a file pointed to by the path.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        FileStream Open(FileMode mode);

        /// <summary>
        /// Reads the file and returns its contents in a string.
        /// </summary>
        /// <returns></returns>
        string ReadAsText();

        /// <summary>
        /// Expand a leading ~ into the user's home directory.
        /// </summary>
        /// <returns></returns>
        IPath ExpandUser();

        /// <summary>
        /// Expand a leading ~ into the given home directory.
        /// </summary>
        /// <param name="homeDir"></param>
        /// <returns></returns>
        IPath? ExpandUser(IPath homeDir);

        /// <summary>
        /// Expand all environment variables in the path.
        /// </summary>
        /// <returns></returns>
        IPath? ExpandEnvironmentVars();

        /// <summary>
        /// Set the current working directory to this path. Upon dispose,
        /// resets to the original working directory only if the current
        /// directory has not been changed in the meantime.
        /// </summary>
        /// <returns></returns>
        IDisposable SetCurrentDirectory();

        #region IPurePath override

        new IPath? Join(params string[] paths);

        new IPath? Join(params IPurePath[] paths);

        public static IPath operator/ (IPath lvalue, IPath rvalue)
        {
            return lvalue.Join(rvalue) ?? throw new InvalidOperationException($"Cannot join {lvalue} with {rvalue}.");
        }

        public static IPath operator/ (IPath lvalue, string rvalue)
        {
            return lvalue.Join(rvalue) ?? throw new InvalidOperationException($"Cannot join {lvalue} with {rvalue}.");
        }

        new IPath? NormCase();

        new IPath? NormCase(CultureInfo currentCulture);

        new IPath? Parent();

        new IPath? Parent(int nthParent);

        new IEnumerable<IPath> Parents();

        new IPath? Relative();

        new IPath? RelativeTo(IPurePath parent);

        new IPath? WithDirname(string newDirName);

        new IPath? WithDirname(IPurePath newDirName);

        new IPath? WithFilename(string newFilename);

        new IPath? WithExtension(string newExtension);

        #endregion
    }

    /// <summary>
    /// IPath implementations are platform dependent and
    /// should not be used on any platform but the one
    /// they were designed for.
    /// </summary>
    /// <typeparam name="TPath"></typeparam>
    /// <typeparam name="TPurePath"></typeparam>
    public interface IPath<out TPath, TPurePath> : IPath , IPurePath<TPurePath>
        where TPath : IPath
        where TPurePath : IPurePath
    {
        /// <summary>
        /// List the files in the directory.
        /// </summary>
        /// <returns></returns>
        new IEnumerable<TPath> ListDir();

        /// <summary>
        /// Glob the given pattern in the directory
        /// </summary>
        /// <exception cref="ArgumentException">Glob was called on a file.</exception>
        /// <param name="pattern"></param>
        /// <returns></returns>
        new IEnumerable<TPath> ListDir(string pattern);

        /// <summary>
        /// Expand a leading ~ into the user's home directory.
        /// </summary>
        /// <returns></returns>
        new TPath? ExpandUser();

        /// <summary>
        /// Expand a leading ~ into the given home directory.
        /// </summary>
        /// <param name="homeDir"></param>
        /// <returns></returns>
        new TPath? ExpandUser(IPath homeDir);

        /// <summary>
        /// Expand all environment variables in the path.
        /// </summary>
        /// <returns></returns>
        new TPath? ExpandEnvironmentVars();

        #region IPurePath override

        new TPath? Join(params string[] paths);

        new TPath? Join(params IPurePath[] paths);


        public static IPath<TPath, TPurePath> operator/ (IPath<TPath, TPurePath> lvalue, IPath<TPath, TPurePath> rvalue)
        {
            return (IPath<TPath, TPurePath>?)lvalue.Join(rvalue) ?? throw new InvalidOperationException($"Cannot join {lvalue} with {rvalue}.");
        }

        public static IPath<TPath, TPurePath> operator/ (IPath<TPath, TPurePath> lvalue, string rvalue)
        {
            return (IPath<TPath, TPurePath>?)lvalue.Join(rvalue) ?? throw new InvalidOperationException($"Cannot join {lvalue} with {rvalue}.");
        }

        new TPath? NormCase();
        new TPath? NormCase(CultureInfo currentCulture);
        new TPath? Parent();
        new TPath? Parent(int nthParent);

        new IEnumerable<TPath> Parents();

        new TPath? Relative();

        new TPath? RelativeTo(IPurePath parent);

        new TPath? WithDirname(string newDirName);

        new TPath? WithDirname(IPurePath newDirName);

        new TPath? WithFilename(string newFilename);

        new TPath? WithExtension(string newExtension);

        #endregion
    }
}
