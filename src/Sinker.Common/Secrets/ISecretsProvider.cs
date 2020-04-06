using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sinker.Common
{
    public interface ISecretsProvider
    {
        Task<IEnumerable<ISecret>> GetAsync();
    }
}
