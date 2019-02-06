using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models.Enums;

namespace Freecon.Server.Core.Interfaces
{
    public interface IDbIdIoService
    {
        Task<IDbIdData> GetIdDataAsync(IDTypes IdType);

        IDbIdData GetIdData(IDTypes idType);

        Task SaveIdDataAsync(IDbIdData idData);

    }
}
