using System;
using System.IO;
using System.Collections.Generic;

using OPath = System.IO.Path;

namespace XChess
{
    /// <summary>
    /// A very small abstraction representing a path in the file system.
    /// </summary>
    public struct Path
    {
        public Path(string Path)
        {
            this._Path = Path;
        }

        /// <summary>
        /// Gets a sub-path with the specified name.
        /// </summary>
        public Path this[string Name]
        {
            get
            {
                return new Path(this._Path + System.IO.Path.DirectorySeparatorChar + Name);
            }
        }

        /// <summary>
        /// Gets the parent path for this path.
        /// </summary>
        public Path Parent
        {
            get
            {
                return new Path(Directory.GetParent(this._Path).FullName);
            }
        }

        /// <summary>
        /// Gets the parent path for this path if any exists.
        /// </summary>
        public Path? MaybeParent
        {
            get
            {
                DirectoryInfo di = Directory.GetParent(this._Path);
                if (di != null)
                {
                    return new Path(di.FullName);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Finds the absolute path for the specified relative path.
        /// </summary>
        public Path Lookup(string Relative)
        {
            Relative = Relative.Replace('/', OPath.DirectorySeparatorChar).Replace('\\', OPath.DirectorySeparatorChar);
            return new Path(OPath.GetFullPath(this._Path + OPath.DirectorySeparatorChar + Relative));
        }

        /// <summary>
        /// Gets if this path is currently a valid file.
        /// </summary>
        public bool ValidFile
        {
            get
            {
                return File.Exists(this._Path);
            }
        }

        /// <summary>
        /// Gets if this path is currently a valid directory.
        /// </summary>
        public bool ValidDirectory
        {
            get
            {
                return Directory.Exists(this._Path);
            }
        }

        /// <summary>
        /// Gets the string representation for the path.
        /// </summary>
        public string PathString
        {
            get
            {
                return this._Path;
            }
        }

        /// <summary>
        /// Gets the file of the current process.
        /// </summary>
        public static Path ProcessFile
        {
            get
            {
                string path = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                return new Path(System.IO.Path.GetFullPath(path));
            }
        }

        /// <summary>
        /// Gets the path for the resources directory.
        /// </summary>
        public static Path Resources
        {
            get
            {
                if (_Resources._Path == null)
                {
                    Path? cur = ProcessFile.Parent;
                    while (cur != null)
                    {
                        Path curval = cur.Value;
                        Path resources = curval["Resources"];
                        if (resources.ValidDirectory)
                        {
                            _Resources = resources;
                        }
                        cur = curval.MaybeParent;
                    }
                }
                return _Resources;
                throw new Exception("No resources folder");
            }
        }

        private static Path _Resources = new Path(null);

        /// <summary>
        /// Reads the entire text from the file located at the specified path.
        /// </summary>
        public static string ReadText(Path Path)
        {
            return File.ReadAllText(Path._Path);
        }

        public static implicit operator string(Path Path)
        {
            return Path._Path;
        }

        private string _Path;
    }
}