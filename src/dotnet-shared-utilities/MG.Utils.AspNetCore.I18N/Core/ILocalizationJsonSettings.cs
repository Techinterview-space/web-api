using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MG.Utils.I18N;

namespace MG.Utils.AspNetCore.I18N.Core
{
    public interface ILocalizationJsonSettings
    {
        CultureInfo CultureInfo { get; }

        IReadOnlyCollection<Translate> Translates();

        Task LoadAsync(CancellationToken cancellationToken);
    }
}