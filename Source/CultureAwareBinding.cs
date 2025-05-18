// This file is part of Friendly Set Play Time. A Playnite extension to simplify the input of play time values.
// Copyright CanRanBan, 2022-2025, Licensed under the EUPL-1.2 or later.

using System.Globalization;
using System.Windows.Data;

namespace FriendlySetPlayTime
{
    internal class CultureAwareBinding : Binding
    {
        public CultureAwareBinding(string path) : base(path)
        {
            ConverterCulture = CultureInfo.CurrentCulture;
        }
    }
}
