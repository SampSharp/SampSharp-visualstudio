using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SampSharp.VisualStudio.Debugger
{
    /// <summary>
    ///     Represents the server configuration file
    /// </summary>
    public class ServerConfig
    {
        private readonly List<string> _keyOrder = new List<string>();
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        /// <summary>
        ///     Gets the value with the specified key.
        /// </summary>
        public string this[string key] => Get(key);

        /// <summary>
        ///     Reads the config file.
        /// </summary>
        public void Read(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            _values.Clear();
            _keyOrder.Clear();

            if (!File.Exists(path))
                return;

            foreach (
                var parts in
                File.ReadAllLines(path)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.TrimStart().Split(new[] { ' ' }, 2)))
                Set(parts[0].Trim(), parts.Length <= 1 ? string.Empty : parts[1]);
        }

        /// <summary>
        ///     Writes the config file.
        /// </summary>
        public void Write(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var config = _keyOrder
                .Where(_values.ContainsKey)
                .Select(k => $"{k} {_values[k]}");

            File.WriteAllLines(path, config);
        }

        /// <summary>
        ///     Gets the configuration value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="trimSpaces">If set to <c>true</c> trim white-space characters.</param>
        /// <returns>
        ///     The value.
        /// </returns>
        public string Get(string key, bool trimSpaces = true)
        {
            return _values.ContainsKey(key) ? (trimSpaces ? _values[key].Trim() : _values[key]) : null;
        }

        /// <summary>
        ///     Gets the configuration value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="trimSpaces">If set to <c>true</c> trim white-space characters.</param>
        /// <returns>
        ///     The value.
        /// </returns>
        public string Get(string key, string defaultValue, bool trimSpaces = true)
        {
            return Get(key, trimSpaces) ?? (trimSpaces ? defaultValue.Trim() : defaultValue);
        }

        /// <summary>
        ///     Sets the configuration value with the specified keyn.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">key</exception>
        public void Set(string key, string value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (!_keyOrder.Contains(key))
                _keyOrder.Add(key);

            _values[key] = value;
        }
    }
}